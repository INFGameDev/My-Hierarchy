// Copyright (C) 2023 INF

// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using EditorScriptingRageAndFrustrationMitigator;
#endif

namespace MyHierarchy
{
    [System.Serializable]
    public class MyHierarchyHeader : MonoBehaviour
    {
        public Color fontColor = Color.white;
        public Color backgroundColor = Color.gray;

        #if UNITY_EDITOR
        [MenuItem("GameObject/My Hierarychy/Header", false, 10)]
        public static void CreateHeader(MenuCommand menu)
        {
            GameObject go = new GameObject();
            go.AddComponent<MyHierarchyHeader>();
            GameObjectUtility.SetParentAndAlign(go, menu.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Created MyHierarchy GO: " + go.name);
            Selection.activeObject = go;
            EditorApplication.RepaintHierarchyWindow();
        }
        #endif

        void OnValidate() 
        {
            #if UNITY_EDITOR
            gameObject.tag = HierarchyRenderer.EditorOnlyTag;
            EditorApplication.delayCall += ()=> {

                if (this == null) 
                    return;

                EditorApplication.RepaintHierarchyWindow();
            };
            #endif
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MyHierarchyHeader)), CanEditMultipleObjects]
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
            gs.SetBackground_AllStates(_.CreateTexture_2x2(new Color(0.25f, 0.25f, 0.25f, 1)));

            base.OnInspectorGUI();
            EditorGUILayout.Space(5);

        
            EditorGUILayout.LabelField(
                "WARNING!:\n - Do not parent another gameobject into this nor parent this to another" + 
                "\n - Do not add any other component either" +
                "\n - Don't reference this gameobject on anything" +
                "\n\n [ This gameobject will be removed at builds ]"
                , 
            gs);

            EditorGUILayout.Space(5);

            EditorGUILayout.LabelField("Created By: (╯°□°)╯︵ INF", EditorStyles.boldLabel);
        }
    }
#endif
}

