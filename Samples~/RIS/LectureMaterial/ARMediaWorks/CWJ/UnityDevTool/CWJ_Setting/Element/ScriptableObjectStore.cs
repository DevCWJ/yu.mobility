#if UNITY_EDITOR
using UnityEngine;
using CWJ.Serializable;
using UnityEditor;
using System;
using System.IO;

namespace CWJ.AccessibleEditor
{
    [System.Serializable] public class StrScriptableObjDictionary : SerializedDictionary<string, CWJScriptableObject> { }

    [CreateAssetMenu(fileName = "ScriptableObjectStore", menuName = "CWJ/Editor/Cache/InitCacheThisFolder(Dangerous)")]
    public sealed class ScriptableObjectStore : CWJScriptableObject
    {
        private static ScriptableObjectStore _Instance = null;
        public static ScriptableObjectStore Instanced
        {
            get
            {
                if (!_Instance)
                {
                    //string path = MyPath;
                    string path = CacheFilePath<ScriptableObjectStore>();
                    var obj = AssetDatabase.LoadAssetAtPath<ScriptableObjectStore>(path);
                    if (!obj)
                    {
                        //if (!MyPath.IsFolderExists(false, isPrintLog: false))
                        //{
                        //    CWJ_EditorEventHelper.OnUnityDevToolDelete();
                        //    typeof(ScriptableObjectStore).PrintLogWithClassName($"CWJ.UnityDevTool is Deleted.\nor {nameof(ScriptableObjectStore)}'s PATH is Wrong", LogType.Error);
                        //}
                        ////obj = CreateScriptableObj<ScriptableObjectStore>(MyPath);
                        EditorApplication.update += CreateMySelf;
                        return null;
                    }
                    _Instance = obj;
                }
                return _Instance;
            }
        }

        static void CreateMySelf()
        {
            EditorApplication.update -= CreateMySelf;
            string path =CacheFilePath<ScriptableObjectStore>();
            _Instance = AssetDatabase.LoadAssetAtPath<ScriptableObjectStore>(path);
            if (!_Instance)
            {
                _Instance = ScriptableObject.CreateInstance<ScriptableObjectStore>();
                AssetDatabase.CreateAsset(_Instance, /*AssetDatabase.GenerateUniqueAssetPath(*/path/*)*/);
                _Instance.OnConstruct();
                EditorUtility.SetDirty(_Instance);
            }
        }
        public static string CacheFilePath<T>() where T : CWJScriptableObject
        {
            return CacheFilePath<T>(typeof(T).Name);
        }

        public static string CacheFilePath<T>(string typeName) where T : CWJScriptableObject
        {
            var asstes = AssetDatabase.FindAssets($"t:{typeName}");
            if (asstes.LengthSafe() > 0)
                return AssetDatabase.GUIDToAssetPath(asstes[0]);
            else
            {
                var storeAssets = AssetDatabase.FindAssets($"t:{nameof(ScriptableObjectStore)}");
                if (storeAssets.LengthSafe() == 0)
                {
                    return null;
                }
                else
                {
                    return  Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(storeAssets[0])) + $"/{typeName}.asset";
                }
            }
        }

        //const string CachePathFormat = "Assets/CWJ/UnityDevTool/_Cache/{0}.asset";

        //public readonly static string MyPath = string.Format(CachePathFormat, nameof(ScriptableObjectStore));

        private static T CreateScriptableObj<T>(string path) where T : CWJScriptableObject
        {
            if (string.IsNullOrEmpty(path))
            {
                if (!AccessibleEditorUtil.TryGetScriptPath(typeof(T).Name, out path))
                    return null;
                else
                    path = path + ".assset";
            }
            T ins = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(ins, /*AssetDatabase.GenerateUniqueAssetPath(*/path/*)*/);
            ins.OnConstruct();
            ins.SaveScriptableObj();
            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();
            return ins;
        }

        [SerializeField, SerializableDictionary(isReadonly: true)] StrScriptableObjDictionary scriptableObjDic = new StrScriptableObjDictionary();

        const string SearchFileTypeFormat = "t:{0}";

        // readonly static string[] CWJFolderPath = new[] { "Assets/CWJ" };
        public T GetScriptableObj<T>() where T : CWJScriptableObject
        {
            string key = typeof(T).FullName;
            bool hasKey;
            if ((hasKey = scriptableObjDic.TryGetValue(key, out var value)) && !value.IsNullOrMissing())
            {
                try
                {
                    T returnVal = (T)value;
                    return returnVal;
                }
                catch (System.InvalidCastException e)
                {
                    Debug.LogError("CWJ폴더를 삭제후 다시 import해주세요\n" + e.ToString());
                }
            }
            string typeName = typeof(T).Name;
            string path = CacheFilePath<T>(typeName);
            T obj;
            if (string.IsNullOrEmpty(path))
            {
                obj = CreateScriptableObj<T>(path);
            }
            else
            {
                obj = DelegateUtil.ManyConditions(
                    checkNotNull: (o) => o,
                () => AssetDatabase.LoadAssetAtPath<T>(path),
                () =>
                {
                    string[] paths = AssetDatabase.FindAssets(string.Format(SearchFileTypeFormat, typeName)).ConvertAll(AssetDatabase.GUIDToAssetPath);
                    return paths.Length > 0 ? AssetDatabase.LoadAssetAtPath<T>(paths[0]) : CreateScriptableObj<T>(path);
                }
            );
            }


            if (!hasKey)
                scriptableObjDic.Add(key, obj);
            else
                scriptableObjDic[key] = obj;

            SaveScriptableObj();
            return obj;
        }


    }

}
#endif