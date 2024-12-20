﻿#if UNITY_EDITOR
using System.Collections;

using UnityEditor;

using UnityEngine;

namespace CWJ.Unity.EditorCor.Editor
{
    public static class EditorCoroutineExtension
    {
        /// <summary>
        /// Start an <see cref="Coroutine_Editor">EditorCoroutine</see>, owned by the calling <see cref="EditorWindow">EditorWindow</see> instance.
        /// <code> 
        /// using System.Collections;
        /// using CWJ.Unity.EditorCor.Editor;
        /// using UnityEditor;
        ///
        /// public class ExampleWindow : EditorWindow
        /// {
        ///     void OnEnable()
        ///     {
        ///         this.StartCoroutine(CloseWindowDelayed());
        ///     }
        ///
        ///     IEnumerator CloseWindowDelayed() //close the window after 1000 frames have elapsed
        ///     {
        ///         int count = 1000;
        ///         while (count > 0)
        ///         {
        ///             yield return null;
        ///         }
        ///         Close();
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public static Coroutine_Editor StartCoroutine(this EditorWindow window, IEnumerator routine)
        {
            return new Coroutine_Editor(routine, window);
        }

        /// <summary>
        /// Immediately stop an <see cref="Coroutine_Editor">EditorCoroutine</see> that was started by the calling <see cref="EditorWindow"/> instance. This method is safe to call on an already completed <see cref="Coroutine_Editor">EditorCoroutine</see>.
        /// <code>
        /// using System.Collections;
        /// using CWJ.Unity.EditorCor.Editor;
        /// using UnityEditor;
        /// using UnityEngine;
        ///
        /// public class ExampleWindow : EditorWindow
        /// {
        ///     EditorCoroutine coroutine;
        ///     void OnEnable()
        ///     {
        ///         coroutine = this.StartCoroutine(CloseWindowDelayed());
        ///     }
        ///
        ///     private void OnDisable()
        ///     {
        ///         this.StopCoroutine(coroutine);
        ///     }
        ///
        ///     IEnumerator CloseWindowDelayed()
        ///     {
        ///         while (true)
        ///         {
        ///             Debug.Log("Running");
        ///             yield return null;
        ///         }
        ///     }
        /// }
        /// </code>
        /// </summary>
        /// <param name="coroutine"></param>
        public static void StopCoroutine(this EditorWindow window, Coroutine_Editor coroutine)
        {
            if (coroutine == null)
            {
                Debug.LogAssertion("Provided EditorCoroutine handle is null.");
                return;
            }

            if (coroutine.m_Owner == null)
            {
                Debug.LogError("The EditorCoroutine is ownerless. Please use EditorCoroutineEditor.StopCoroutine to terminate such coroutines.");
                return;
            }

            if (!coroutine.m_Owner.IsAlive)
                return; //The EditorCoroutine's owner was already terminated execution will cease next time it is processed

            var owner = coroutine.m_Owner.Target as EditorWindow;

            if (owner == null || owner != null && owner != window)
            {
                Debug.LogErrorFormat("The EditorCoroutine is owned by another object: {0}.", coroutine.m_Owner.Target);
                return;
            }

            EditorCoroutineUtil.StopCoroutine(coroutine);
        }
    }
} 
#endif