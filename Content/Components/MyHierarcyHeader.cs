using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using EditorScriptingRageAndFrustrationMitigator;
#endif

namespace MyHierarchy
{
    [System.Serializable]
    public class MyHierarcyHeader : MonoBehaviour
    {
        public string m_name = "My Header";
        public Color backgroundColor = Color.gray;
        public Color fontColor = Color.white;

        [MenuItem("GameObject/My Hierarychy/Header", false, 10)]
        public static void CreateHeader(MenuCommand menu)
        {
            GameObject go = new GameObject();
            go.AddComponent<MyHierarcyHeader>();
            GameObjectUtility.SetParentAndAlign(go, menu.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Created MyHierarchy GO: " + go.name);
            Selection.activeObject = go;

            #if UNITY_EDITOR
            EditorApplication.RepaintHierarchyWindow();
            #endif
        }

        void OnValidate() {
            gameObject.tag = HierarchyRenderer.editorOnlyTag;

            #if UNITY_EDITOR
            EditorApplication.delayCall += ()=> {

                if (this == null) 
                    return;

                EditorApplication.RepaintHierarchyWindow();
            };
            #endif
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MyHierarcyHeader)), CanEditMultipleObjects]
    public class MyHierarcyHeaderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUIStyle gs = new GUIStyle(EditorStyles.helpBox);
            gs.fontStyle = FontStyle.Bold;
            gs.alignment = TextAnchor.MiddleCenter;
            gs.fontSize = 13;
            gs.SetFontColor_AllStates(Color.yellow);

            base.OnInspectorGUI();
            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("WARNING!: Do not parent another gameobject into this gameobject nor parent this to another since it will removed at builds.", gs);
            EditorGUILayout.Space(85);
            Flippin.FlippingINF(new Vector2(EditorGUIUtility.currentViewWidth / 2 + 50, 140));
        }
    }
#endif
}

