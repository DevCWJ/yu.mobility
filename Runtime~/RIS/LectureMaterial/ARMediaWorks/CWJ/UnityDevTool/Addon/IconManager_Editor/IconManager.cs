﻿#if UNITY_EDITOR
using System;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace TNRD.Utilities
{
    public static class IconManager
    {
        private static MethodInfo setIconForObjectMethodInfo;

        public static void SetIcon(GameObject gameObject, LabelIcon labelIcon)
        {
            SetIcon(gameObject, $"sv_label_{(int)labelIcon}");
        }

        public static void SetIcon(GameObject gameObject, ShapeIcon shapeIcon)
        {
            SetIcon(gameObject, $"sv_icon_dot{(int)shapeIcon}_pix16_gizmo");
        }

        public static void SetIcon(GameObject gameObject, string contentName)
        {
            GUIContent iconContent = EditorGUIUtility.IconContent(contentName);
            if (iconContent == null || iconContent.image == null) return;
            SetIconForObject(gameObject, (Texture2D)iconContent.image);
        }

        public static void RemoveIcon(GameObject gameObject)
        {
            SetIconForObject(gameObject, null);
        }

        public static void SetIconForObject(GameObject obj, Texture2D icon)
        {
            if (setIconForObjectMethodInfo == null)
            {
                Type type = typeof(EditorGUIUtility);
                setIconForObjectMethodInfo =
                    type.GetMethod("SetIconForObject", BindingFlags.Static | BindingFlags.NonPublic);
            }

            setIconForObjectMethodInfo.Invoke(null, new object[] { obj, icon });
        }
    }
} 
#endif