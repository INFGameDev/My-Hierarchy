using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
    using UnityEditor;
    using EGL = UnityEditor.EditorGUILayout;
    using GL = UnityEngine.GUILayout;
    using EG = UnityEditor.EditorGUI;
    using EditorScriptingRageAndFrustrationMitigator;
#endif

namespace MyHierarchy
{
    // [CreateAssetMenu(fileName = "My Hierarchy Settings", menuName = "My Hierarchy Settings", order = 0)]
    public class MyHierarchySettings : ScriptableObject {
        [SerializeField, HideInInspector] public bool showLayers;
        [SerializeField, HideInInspector] public bool showTags;
        [SerializeField, HideInInspector] public bool showStaticObjects;
        [SerializeField, HideInInspector] public bool showDepth;
        [SerializeField, HideInInspector] public bool activate;
        [SerializeField, HideInInspector] public bool showLabelsOnGroup;

        [Header("Header Controls:")]
        public FontStyle headerFontStyle = FontStyle.Bold;
        public Alignment headerAlignment = Alignment.Center;


        [Header("Group Header Controls:")]
        public FontStyle groupFontStyle = FontStyle.Bold; 
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(MyHierarchySettings))]
    public class MyHierarchySettingsEditor : Editor 
    {
        private ScriptingBandAid bandAid;
        private readonly Color onColor = Color.green;
        private readonly Color offColor = Color.grey;

        private void OnEnable() => bandAid = new ScriptingBandAid();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            MyHierarchySettings settings = target as MyHierarchySettings;
            serializedObject.Update();
            bandAid.CacheDefaultColors();
            
            float buttonWidthx2 = bandAid.viewWidth / 2 - 17;
            float buttonWidthx3 = bandAid.viewWidth / 3 - 12;

            EGL.Space(10);
            var gs = new GUIStyle(EditorStyles.boldLabel);
            gs.alignment = TextAnchor.MiddleCenter;
            EGL.LabelField("VISIBILITY:", gs);

            gs.alignment = TextAnchor.MiddleLeft;
            EGL.LabelField("Non-Header:", gs);
            Rect allRect;
            using (new GroupConstraint(GroupDir.Vertical).GetRect(out allRect))
            {
                using (new GroupConstraint(GroupDir.Horizontal))
                {
                    bandAid.CreateToggle(
                        settings.showLayers, 
                        onColor, 
                        offColor, 
                        new GUIContent("Show Layer"), 
                        ()=> settings.showLayers = !settings.showLayers,
                        null,
                        new GUILayoutOption[] {GUILayout.Width(buttonWidthx2), GUILayout.Height(30)}
                    );

                    bandAid.CreateToggle(
                        settings.showTags, 
                        onColor, 
                        offColor, 
                        new GUIContent("Show Tag"), 
                        ()=> settings.showTags = !settings.showTags,
                        null,
                        new GUILayoutOption[] {GUILayout.Width(buttonWidthx2), GUILayout.Height(30)}
                    ); 
                }

                using (new GroupConstraint(GroupDir.Horizontal))
                {
                    bandAid.CreateToggle(
                        settings.showStaticObjects, 
                        onColor, 
                        offColor, 
                        new GUIContent("Show Is Static"), 
                        ()=> settings.showStaticObjects = !settings.showStaticObjects,
                        null,
                        new GUILayoutOption[] {GUILayout.Width(buttonWidthx2), GUILayout.Height(30)}
                    );

                    bandAid.CreateToggle(
                        settings.showDepth, 
                        onColor, 
                        offColor, 
                        new GUIContent("Show Depth"), 
                        ()=> settings.showDepth = !settings.showDepth,
                        null,
                        new GUILayoutOption[] {GUILayout.Width(buttonWidthx2), GUILayout.Height(30)}
                    );   
                }  

                EGL.Space(10);
                EGL.LabelField("Group Header", gs);

                using (new GroupConstraint(GroupDir.Horizontal))
                {
                    bandAid.CreateToggle(
                        settings.showLabelsOnGroup,
                        onColor, 
                        offColor, 
                        new GUIContent("Show Group Header Labels"), 
                        ()=> settings.showLabelsOnGroup = !settings.showLabelsOnGroup,
                        null,
                        new GUILayoutOption[] {GUILayout.Width(buttonWidthx2*2+4), GUILayout.Height(30)}
                    ); 
                }

                EGL.Space(20);
                EGL.LabelField("All", gs);

                bandAid.CreateToggle(
                    settings.activate, 
                    onColor, 
                    offColor, 
                    new GUIContent("Toggle Activation"), 
                    ()=> settings.activate = !settings.activate,
                    null,
                    new GUILayoutOption[] {GUILayout.Width(buttonWidthx2*2+4), GUILayout.Height(30)}
                );
            }

            Flippin.FlippingINF(new Vector2(180, allRect.yMax + 30));
    
            if (GUI.changed)
            {
                EditorUtility.SetDirty(settings); 
                EditorApplication.RepaintHierarchyWindow();
            }
                
            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("My Hierarchy/Show Settings")]
        public static void LocateSettingsFile()
        {
            Selection.activeObject = HierarchyRenderer.GetAsset_SO<MyHierarchySettings>("MyHierarchySettings", "My Hierarchy Settings");
        }
    }
    #endif
}