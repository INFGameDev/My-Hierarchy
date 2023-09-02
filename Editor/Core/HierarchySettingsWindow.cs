// Copyright (C) 2023 INF

// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;
    using EGL = UnityEditor.EditorGUILayout;
    using GL = UnityEngine.GUILayout;
    using EG = UnityEditor.EditorGUI;
    using EditorScriptingRageAndFrustrationMitigator;
#endif

namespace MyHierarchy
{
    public class HierarchySettingsWindow : EditorWindow
    {
        private ScriptingBandAid bandAid;
        private readonly Color onColor = Color.green;
        private readonly Color offColor = Color.grey;
        private static MyHierarchySettings settings;
        private static HierarchySettingsWindow window;
        private static SerializedObject settingsSO;

        [MenuItem("My Hierarchy/Settings")]
        private static void ShowWindow() {
            window = GetWindow<HierarchySettingsWindow>();
            window.titleContent = new GUIContent("Hierarchy Settings");
            window.minSize = new Vector2( 450, 700 );
            window.maxSize = window.minSize;
            window.Show();
        }

        private void OnEnable() => bandAid = new ScriptingBandAid();

        private void OnGUI()
        {
            if ( settings == null )
                settings = HierarchyRenderer.GetAsset_SO<MyHierarchySettings>("MyHierarchySettings", "My Hierarchy Settings");

            if (settingsSO == null)
                settingsSO = new SerializedObject(settings);

            settingsSO.Update();
            bandAid.CacheDefaultColors();
            EditorGUI.BeginChangeCheck();

            float buttonWidthx2 = bandAid.viewWidth / 2 - 5;
            float buttonWidthx3 = bandAid.viewWidth / 3 - 5;


            GL.Space(10);
            EGL.PropertyField(settingsSO.FindProperty( nameof(settings.headerFontStyle) ));
            EGL.PropertyField(settingsSO.FindProperty( nameof(settings.headerAlignment) ));
            EGL.PropertyField(settingsSO.FindProperty( nameof(settings.groupFontStyle) ));
            EGL.PropertyField(settingsSO.FindProperty( nameof(settings.labelWidth)  ));


            EGL.Space(10);
            var gs = new GUIStyle(EditorStyles.boldLabel);
            gs.alignment = TextAnchor.MiddleCenter;
            EGL.LabelField("VISIBILITY:", gs);

            // ====================================================================================================================

            gs.alignment = TextAnchor.MiddleLeft;
            EGL.LabelField("Labels:", gs);
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

                using (new GroupConstraint(GroupDir.Horizontal))
                {
                    bandAid.CreateToggle(
                        settings.showComponents, 
                        onColor, 
                        offColor, 
                        new GUIContent("Show Components"), 
                        ()=> settings.showComponents = !settings.showComponents,
                        null,
                        new GUILayoutOption[] {GUILayout.Width(buttonWidthx2), GUILayout.Height(30)}
                    );

                    bandAid.CreateToggle(
                        settings.hideIconlessComponents, 
                        onColor, 
                        offColor, 
                        new GUIContent("Hide Iconless Components"), 
                        ()=> settings.hideIconlessComponents = !settings.hideIconlessComponents,
                        null,
                        new GUILayoutOption[] {GUILayout.Width(buttonWidthx2), GUILayout.Height(30)}
                    );   
                }

                // ====================================================================================================================
                EGL.Space(10);
                EGL.LabelField("Object Relationship Lines", gs);

                using (new GroupConstraint(GroupDir.Horizontal))
                {
                    bandAid.CreateToggle(
                        settings.highlightSelectedSiblings, 
                        onColor, 
                        offColor, 
                        new GUIContent("Highlight Selected's Siblings"), 
                        ()=> settings.highlightSelectedSiblings = !settings.highlightSelectedSiblings,
                        null,
                        new GUILayoutOption[] {GUILayout.Width(buttonWidthx2), GUILayout.Height(30)}
                    );

                    bandAid.CreateToggle(
                        settings.highlightSelectedChildren, 
                        onColor, 
                        offColor, 
                        new GUIContent("Highlight Selected's Children"), 
                        ()=> settings.highlightSelectedChildren = !settings.highlightSelectedChildren,
                        null,
                        new GUILayoutOption[] {GUILayout.Width(buttonWidthx2), GUILayout.Height(30)}
                    );   
                } 

                using (new GroupConstraint(GroupDir.Horizontal))
                {
                    bandAid.CreateToggle(
                        settings.showRelationshipLines,
                        onColor, 
                        offColor, 
                        new GUIContent("Show Object Relationship"), 
                        ()=> settings.showRelationshipLines = !settings.showRelationshipLines,
                        null,
                        new GUILayoutOption[] {GUILayout.Width(buttonWidthx2*2+4), GUILayout.Height(30)}
                    ); 
                }
    
                // ====================================================================================================================

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

                // ====================================================================================================================

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

            // ====================================================================================================================
            EGL.Space(100);
            Flippin.FlippingINF(new Vector2(180, allRect.yMax + 30));

            // ====================================================================================================================
    
            if ( EditorGUI.EndChangeCheck() )
            {
                EditorUtility.SetDirty( settings );
                EditorApplication.RepaintHierarchyWindow();
            }
                
            settingsSO.ApplyModifiedProperties();
        }
    }

}

