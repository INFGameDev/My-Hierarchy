// Copyright (C) 2023 INF

// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

#if UNITY_EDITOR
    using UnityEditor;
    using EGL = UnityEditor.EditorGUILayout;
    using GL = UnityEngine.GUILayout;
    using EG = UnityEditor.EditorGUI;
    using EditorScriptingRageAndFrustrationMitigator;


    namespace EditorScriptingRageAndFrustrationMitigator
    {
        /// <summary>
        /// Some helper methods and stuff that makes writing editor code less painful and depression inducing moment
        /// [ Features ]
        /// | 1 Makes your editor code looks less like garbage
        /// | 2 Mitigates the probably of you commiting suicide from axienty producde during the process of writing said code.
        /// | 3 Reduces hate inducing rage during the processing of writing editor code 
        /// | 4- Offers methods that helps draw GUI quickly in less lines than having a renundant huge bullsh*t block spanning over 10 lines            
        /// 
        /// (╯°□°）╯︵ Made by: INF
        /// </summary>
        public sealed class ScriptingBandAid : Editor
        {
            public Color defaultBackgroundColor;
            public float viewWidth => EditorGUIUtility.currentViewWidth;
            public float viewWidthHalf => EditorGUIUtility.currentViewWidth / 2;
            public float viewWidth3rd => EditorGUIUtility.currentViewWidth / 3;

            public void CacheDefaultColors() => defaultBackgroundColor = GUI.backgroundColor;
            public float GetViewWidth(float rightMarginAddition = 0) => viewWidth - rightMarginAddition;

            public void OverwriteScriptableObject<T>(T objectBeingOverwritten, T objectToCopyFrom) where T : ScriptableObject
            {
                SerializedObject copyingAsset = new SerializedObject( objectToCopyFrom );
                SerializedObject savedAsset = new SerializedObject(objectBeingOverwritten);
                
                var it = copyingAsset.GetIterator();
                if (!it.NextVisible(true)) 
                    return;
                //Descends through serialized property children & allows us to edit them.
                do
                {
                    if (it.propertyPath == "m_Script" && savedAsset.targetObject != null)
                        continue;
                        
                    savedAsset.CopyFromSerializedProperty(it);
                }
                while (it.NextVisible(false));

                savedAsset.ApplyModifiedProperties();
                EditorUtility.SetDirty(objectBeingOverwritten);
            }

            public void ResetBackgroundColor() => GUI.backgroundColor = defaultBackgroundColor;
            public void SetBackgroundColor(Color newColor) => GUI.backgroundColor = newColor;

            // =========================================================================================================

            public void CreateToggle(
                bool condition, 
                Color trueColor, 
                Color falseColor, 
                GUIContent content, 
                System.Action OnClick, 
                GUIStyle style = null, 
                params GUILayoutOption[] options
                )
            {
                if (condition) 
                    SetBackgroundColor(trueColor);
                else 
                    SetBackgroundColor(falseColor);

                if (style != null){
                    if ( GL.Button( content, style ) )
                        OnClick();
                } else if (options != null) {
                    if ( GL.Button( content, options ) )
                        OnClick();                    
                } else if (style != null && options != null){
                    if ( GL.Button( content, style , options) )
                        OnClick();                    
                } else {
                    if ( GL.Button(content) )
                        OnClick();                      
                }

                ResetBackgroundColor();
            }

            public static Vector3 GetLabelSize(string text, GUIStyle gs) => gs.CalcSize(new GUIContent(text));
        }

        public enum GroupDir
        {
            Horizontal,
            Vertical
        }

        public abstract class DirectionalGroups
        {

            public DirectionalGroups() {}
            public abstract IDisposable Constraint(params GUILayoutOption[] gUILayouts);
            public abstract IDisposable Constraint();
        }

            public class GroupConstraint : IDisposable
            {
                private GroupDir direction;   
                private float rightMargin;
                public Rect rect;

                public GroupConstraint(GroupDir direction, params GUILayoutOption[] GuiLayouts)
                {

                    if (direction == GroupDir.Horizontal){
                        rect = EGL.BeginHorizontal(GuiLayouts);
                    } else {
                        rect = EGL.BeginVertical(GuiLayouts);
                    }  
                }

                 public GroupConstraint(GroupDir direction, GUIStyle style)
                {
                    if (direction == GroupDir.Horizontal){
                        rect = EGL.BeginHorizontal(style);
                    } else {
                        rect = EGL.BeginVertical(style);
                    }  
                }

                public GroupConstraint(GroupDir direction, GUIStyle style, params GUILayoutOption[] GuiLayouts)
                {
                    if (direction == GroupDir.Horizontal){
                        rect = EGL.BeginHorizontal(style, GuiLayouts);
                    } else {
                        rect = EGL.BeginVertical(style, GuiLayouts);
                    }  
                }

                public GroupConstraint(GroupDir direction)
                {
                    if (direction == GroupDir.Horizontal){
                        rect = EGL.BeginHorizontal();
                    } else {
                        rect = EGL.BeginVertical();
                    }  
                }

                public GroupConstraint(GroupDir direction, out Rect r)
                {
                    if (direction == GroupDir.Horizontal){
                        rect = EGL.BeginHorizontal();
                        r = rect;
                    } else {
                        rect = EGL.BeginVertical();
                        r = rect;
                    }  
                }

                public IDisposable GetRect(out Rect r)
                {
                    r = rect;
                    return (IDisposable)this;
                }
            

                public void Dispose()
                {
                    if (direction == GroupDir.Horizontal){
                        GL.Space(rightMargin);
                        EGL.EndHorizontal();
                    } else {
                        EGL.EndVertical();
                    }
                }
        }

        public static class ExtensionMethods
        {
            public static GUIStyle SetBackground_AllStates(this GUIStyle style, Texture2D texture)
            {
                style.active.background = texture;
                style.hover.background = texture;
                style.normal.background = texture;
                return style;
            }

            public static GUIStyle SetFontColor_AllStates(this GUIStyle style, Color color)
            {
                style.active.textColor = color;
                style.hover.textColor = color;
                style.normal.textColor = color;
                return style;
            }
        }

        public static class _
        {
            public static void Print(params object[] f)
            {
                string message = string.Empty;
                for (int i = 0; i < f.Length; i++)
                {
                    message += f[i] + " | ";
                }
                Debug.Log(message);
            }

            public static Texture2D CreateTexture(Color color, Vector2Int size)
            {
                Texture2D texture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false);
                
                for (int i = 0; i < texture.width; i++) {
                    for (int j = 0; j < texture.height; j++) {
                        texture.SetPixel(i,j, color);
                    }
                }

                texture.Apply();
                return texture;
            }

            public static Texture2D CreateTexture_2x2(Color color)
            {
                Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                
                for (int i = 0; i < texture.width; i++) {
                    for (int j = 0; j < texture.height; j++) {
                        texture.SetPixel(i,j, color);
                    }
                }

                texture.Apply();
                return texture;
            }

        }
    }
#endif
