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

        #if UNITY_EDITOR
        [MenuItem("GameObject/My Hierarychy/Header", false, 10)]
        public static void CreateHeader(MenuCommand menu)
        {
            GameObject go = new GameObject();
            go.AddComponent<MyHierarcyHeader>();
            GameObjectUtility.SetParentAndAlign(go, menu.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Created MyHierarchy GO: " + go.name);
            Selection.activeObject = go;

            EditorApplication.RepaintHierarchyWindow();
        }
        #endif

        void OnValidate() {
            #if UNITY_EDITOR
            gameObject.tag = HierarchyRenderer.editorOnlyTag;
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
            Color color = Color.yellow * 0.85f;
            gs.SetFontColor_AllStates(new Color(color.r, color.g, color.b, 1));

            base.OnInspectorGUI();
            EditorGUILayout.Space(5);

            
            Rect rect = EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(
                "WARNING!:\n - Do not parent another gameobject into this nor parent this to another" + 
                "\n - Do not add any other component either" +
                "\n - Don't reference this gameobject on anything" +
                "\n\n [ This gameobject will be removed at builds ]"
                , 
                gs);
            EditorGUILayout.EndVertical();
            Flippin.FlippingINF(new Vector2(EditorGUIUtility.currentViewWidth / 2 + 50, rect.yMax + 15));
            EditorGUILayout.Space(rect.height + 90);
        }
    }
#endif
}

