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
using EditorScriptingRageAndFrustrationMitigator;
#endif

namespace MyHierarchy
{
    public enum Alignment { Left, Center, Right }
    public static class AlignmentExtensionMethods
    {
        public static TextAnchor ToTextAnchor(this Alignment alignment)
        {
          TextAnchor anchor = default;
          switch (alignment)
          {
            case Alignment.Left:
              anchor = TextAnchor.MiddleLeft;
              break;
            case Alignment.Center:
              anchor = TextAnchor.MiddleCenter;
              break;
            case Alignment.Right:
              anchor = TextAnchor.MiddleRight;
              break;
          }

          return anchor;
        }
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class HierarchyRenderer
    {
        private const int HierarchyItemRectRightMargin = 15;

        /// <summary>
        /// how much the x position of the hierarchy's item rect shifts to the right side per depth  
        /// a level 1 depth means having 1 parent will shift the hierarchy's item rect X pos to this amount to the right
        /// </summary>
        private const int HierarchyItemXShiftPerDepth = 14;
        private const int SceneVisibilityAndPickabilityControlXMax = 32;

        /// <summary>
        /// The x position of the hierarchy item at the root level of hierarchy, further parented to other objects get's will make their x position shift to the right
        /// </summary>
        private const int HierarchyRootItemXMin = 60;
        private const int DepthNumberMargin = 2;
        private const float ParentToChildLineXPosLeftShift = 22.5f;
        private const float ParentToChildLineHeight = 2;
        private const int ParentToChildLineLength = 8;
        private const float ParentToLastIndexChildLineLenghtMultiplier = 2.5f;
        private const int ParentToChildVerticalLineYPosAddition = 7;
        private const int DividerLineWidth = 1;
        private const float DividerLineSpaceFromLabel = 4;
        private static readonly Vector2 staticIndicatorSize = new Vector2(8, 8);
        // private const float LabelFixedWidth = 60;
        private const float GameObjectHierarchyItemIconWidth = 15;
        private const float IconToGroupHeaderLabelSpace = 3;
        private static readonly Color ToGameobjectItemLineColor_Unselected = new Color(0.5f, 0.5f, 0.5f, 1);
        private static readonly Color ToGameobjectItemLineColor_Selected = new Color(0.85f, 0.85f, 0.85f, 1);
        private static readonly Color SameParentLineColor_InActive = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        private static readonly Color SameParentLineColor_Active = Color.yellow * 0.9f;
        private const float SameParentLineYMargin = 1.5f;
        private const float SameParentLineLeftShift = 14f;
        private static int SelectedObjectItemDepth = -1;
        private static bool IsCurrentlyDrawingSelectedObject = false;
        private static bool IsSelectedObjectChild = false;
        private static readonly Color DividerLineColor = Color.gray;
        private static readonly Color NonStaticColor = new Color(0, 0.75f, 0, 1);
        private static readonly Color StaticColor = new Color(0.4f, 0.4f, 0.4f, 1);
        private static readonly Color ComponentIconBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
        private static readonly Color inactiveObjectColorTint = new Color(0f, 0f, 0f, 0.3f);

        /// <summary>
        /// Checks if parent of the selected object is the parent of this game object thus checking this gameobject is a sibling of the current selected gameobject
        /// </summary>
        private static bool IsSiblingOfSelectedObject = false;
        private static bool HasNextSibling = false; 
        private static MyHierarchySettings settings;

        static HierarchyRenderer() => EditorApplication.hierarchyWindowItemOnGUI += OnGameObjectItemRender;
        static void OnGameObjectItemRender(int instanceID, Rect selectionRect)
        {
            bool isHeaderGroup = false;
            IsCurrentlyDrawingSelectedObject = false;
            IsSelectedObjectChild = false;
            HasNextSibling = false;

            if (settings == null)
                settings = GetAsset_SO<MyHierarchySettings>("MyHierarchySettings", "My Hierarchy Settings");

            if (!settings.activate)
                return;

            GameObject go = (GameObject)EditorUtility.InstanceIDToObject(instanceID);
            if (go == null)
                return;

            GameObject selectedGO = Selection.activeGameObject; 

            if (selectedGO != null){
                if (selectedGO.scene.IsValid()) // check if the selected go is from the hierarchy
                {
                    if (selectedGO == go )
                        IsCurrentlyDrawingSelectedObject = true;

                    if (go.transform.IsChildOf(Selection.activeGameObject.transform))
                        IsSelectedObjectChild = true;

                    if ( selectedGO.transform.parent != null && go.transform.IsChildOf(selectedGO.transform.parent))
                        IsSiblingOfSelectedObject = true;
                    else {
                        IsSiblingOfSelectedObject = false;
                    }
                }
            }
            else
            {
                SelectedObjectItemDepth = -1;
                IsSiblingOfSelectedObject = false;
            }

            if (go.transform.parent != null)
                HasNextSibling = go.transform.GetSiblingIndex() != go.transform.parent.childCount-1;

            if (go.TryGetComponent<MyHierarchyHeader>(out MyHierarchyHeader header))
            {
                DrawHeader(header, selectionRect, go.transform);
                return;
            }

            if (go.TryGetComponent<MyHierarchyGroup>(out MyHierarchyGroup group)){
                isHeaderGroup = true;
                DrawGroupHeader(selectionRect, go.name, group, go.activeSelf);
            }
                
            // draw only on gameobjects that is parented
            if (selectionRect.xMin > HierarchyRootItemXMin)
                DrawObjectRelationshipLines(selectionRect, go.transform);

            if (settings.showComponents)
                DrawComponents(selectionRect, go.transform, isHeaderGroup);

            if (isHeaderGroup && !settings.showLabelsOnGroup)
                return;

            if (settings.showStaticObjects)
                DrawIsStaticLabel(selectionRect, go.isStatic);

            if (settings.showLayers)
                DrawLayerLabel(selectionRect, go.layer);

            if (settings.showTags)
                DrawTagLabel(selectionRect, go.tag);

            // DrawRectXMax(selectionRect);
            // DrawRectXMin(selectionRect);
            // DrawRectYMin(selectionRect);
            // DrawRectYMax(selectionRect);
        }

        private static void DrawObjectRelationshipLines( Rect rect, Transform goTransform )
        {
            if (settings.showRelationshipLines) {
                Color color = IsSelectedObjectChild && settings.highlightSelectedChildren 
                ? ToGameobjectItemLineColor_Selected 
                : ToGameobjectItemLineColor_Unselected;

                DrawVerticalLine(rect, color);
                DrawHorizontalLine(rect, goTransform.childCount > 0, color);
            }

            DrawDepth(rect, goTransform);
        }

        #region Header Draws ============================================================================================================================

        private static float GetAllLabelsXMin(Rect rect)
        {
            float dividerSpaceMultiplier = 0;
            dividerSpaceMultiplier += Convert.ToInt32(settings.showLayers) * 2; // each label contains a space at each side
            dividerSpaceMultiplier += Convert.ToInt32(settings.showStaticObjects) * 2;
            dividerSpaceMultiplier += Convert.ToInt32(settings.showTags) * 2;

            float labelWdithMultiplier = 0;
            labelWdithMultiplier += Convert.ToInt32(settings.showLayers); // each label contains a space at each side
            labelWdithMultiplier += Convert.ToInt32(settings.showTags);

            Rect allLabelsRect = rect;
            allLabelsRect.xMin = rect.xMax - (settings.labelWidth * labelWdithMultiplier) - // x pos
            (settings.showStaticObjects ? staticIndicatorSize.x : 0) - (DividerLineSpaceFromLabel * dividerSpaceMultiplier); // -> indicator width;
            allLabelsRect.size = new Vector2(settings.labelWidth, allLabelsRect.size.y);
            return allLabelsRect.xMin;
        }

        private static void DrawGroupHeader( Rect rect, string goName, MyHierarchyGroup groupHeader, bool isActive )
        {
            GUIStyle groupLabelStyle = new GUIStyle(EditorStyles.label);
            groupLabelStyle.normal.textColor = groupHeader.fontColor;
            groupLabelStyle.fontStyle = settings.groupFontStyle;

            Rect headerRect = rect;
            headerRect.xMax = settings.showLabelsOnGroup ? GetAllLabelsXMin(rect) : rect.xMax;
            headerRect.xMin = rect.xMin + GameObjectHierarchyItemIconWidth + IconToGroupHeaderLabelSpace;


            EditorGUI.DrawRect(headerRect, groupHeader.backgroundColor);
            EditorGUI.LabelField(headerRect, goName, groupLabelStyle);

            if (!isActive){
                Rect inactiveRect = headerRect;
                EditorGUI.DrawRect(inactiveRect, inactiveObjectColorTint);
            }
        }

        private static void DrawHeader(MyHierarchyHeader header, Rect rect, Transform goTransform)
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.label);
            headerStyle.normal.textColor = header.fontColor;
            headerStyle.alignment = settings.headerAlignment.ToTextAnchor();
            headerStyle.fontStyle = settings.headerFontStyle;

            rect.xMin = SceneVisibilityAndPickabilityControlXMax;
            rect.xMax = rect.xMax + HierarchyItemRectRightMargin;

            if (goTransform.childCount > 0 || goTransform.parent != null)
            {
                EditorGUI.DrawRect(rect, Color.red);
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = Color.white;
               
                if (goTransform.childCount > 0)  {
                    EditorGUI.LabelField(rect, "Headers shouldn't have child gameobjects".ToUpper() , headerStyle);
                    Debug.LogError($"My Hierarychy Header's shouldn't have child gameobjects!, Unparent all gameobjects from ({goTransform.name})");
                } else {
                    EditorGUI.LabelField(rect, "Headers shouldn't be parented".ToUpper() , headerStyle);
                    Debug.LogError($"My Hierarychy Header's shouldn't parented to other gameobject, Unparent from ({goTransform.name})");
                }
            } else {
                EditorGUI.DrawRect(rect, header.backgroundColor);
                EditorGUI.LabelField(rect, string.Format(" {0} ", goTransform.name), headerStyle);                
            }
        }
        #endregion Header Draws ============================================================================================================================

        #region Visibility Controlled Properties =============================================================================================================
    
        // private static void DrawRectXMin(Rect rect)
        // {
        //     Rect newRect = rect;
        //     newRect.size = new Vector2(1, newRect.size.y);
        //     newRect.x = rect.xMin;
        //     EditorGUI.DrawRect(newRect, Color.gray);
        // }

        // private static void DrawRectXMax(Rect rect)
        // {
        //     Rect newRect = rect;
        //     newRect.size = new Vector2(1, newRect.size.y);
        //     newRect.x = rect.xMax - 1f;
        //     EditorGUI.DrawRect(newRect, Color.gray);
        // }

        // private static void DrawRectYMin(Rect rect)
        // {
        //     Rect newRect = rect;
        //     newRect.size = new Vector2(rect.width, 1);
        //     newRect.y = rect.yMin;
        //     EditorGUI.DrawRect(newRect, Color.gray);            
        // }

        // private static void DrawRectYMax(Rect rect)
        // {
        //     Rect newRect = rect;
        //     newRect.size = new Vector2(rect.width, 1);
        //     newRect.y = rect.yMax - 1;
        //     EditorGUI.DrawRect(newRect, Color.gray);            
        // }

        private static void DrawIsStaticLabel(Rect rect, bool isStatic) 
        {   
            Rect indicatorRect = rect;
            indicatorRect.xMin = rect.xMax - staticIndicatorSize.x - DividerLineSpaceFromLabel; // x pos
            indicatorRect.yMin = rect.yMin +  ( (rect.yMax - rect.yMin) / 4 ); 
            // y pos -> / 4 because we need to position the start of drawing of the box not from the center which would be / 2 but at 1/4 of the way, just before the center
            indicatorRect.size = staticIndicatorSize;

            EditorGUI.DrawRect(indicatorRect, isStatic ? StaticColor : NonStaticColor);
            DrawLineDivider(indicatorRect.x, rect);
        }

        
        private static void DrawLayerLabel(Rect rect, LayerMask layer)
        {
            string layerString = LayerMask.LayerToName(layer);
            Rect layerRect = rect;

            layerRect.xMin = rect.xMax - settings.labelWidth - // x pos
            (settings.showStaticObjects ? staticIndicatorSize.x  : 0) - (DividerLineSpaceFromLabel * (settings.showStaticObjects ? 3 : 1)); // -> indicator width;
            layerRect.size = new Vector2(settings.labelWidth, layerRect.size.y);

            EditorGUI.LabelField(layerRect, layerString, new GUIStyle(EditorStyles.label));
            DrawLineDivider(layerRect.xMin, rect);
        }

        private static void DrawTagLabel(Rect rect, string tag)
        {
            float dividerSpaceMultiplier = 1;
            dividerSpaceMultiplier += Convert.ToInt32(settings.showLayers) * 2; // each label contains a space at each side
            dividerSpaceMultiplier += Convert.ToInt32(settings.showStaticObjects) * 2;

            Rect tagRect = rect;
            tagRect.xMin = rect.xMax - (settings.labelWidth * (settings.showLayers ? 2 : 1)) - // x pos
            (settings.showStaticObjects ? staticIndicatorSize.x : 0) - (DividerLineSpaceFromLabel * dividerSpaceMultiplier); // -> indicator width;
            tagRect.size = new Vector2(settings.labelWidth, tagRect.size.y);

            EditorGUI.LabelField(tagRect, tag, new GUIStyle(EditorStyles.label));
            DrawLineDivider(tagRect.xMin, rect);
        }

        private static void DrawDepth(Rect rect, Transform goTransoform) // draws the number on how deep the gameobject in the hierarchy 
        {
            if (!settings.showDepth && !settings.showRelationshipLines)
                return;

            float nameXRightPosShift = rect.xMin - HierarchyRootItemXMin; 
            float shiftCount = nameXRightPosShift / HierarchyItemXShiftPerDepth;

            if (IsCurrentlyDrawingSelectedObject)
                SelectedObjectItemDepth = (int)shiftCount;

            if (shiftCount > 1 && settings.showRelationshipLines)
                DrawSiblingConnectionLines(rect, (int)shiftCount, goTransoform);

            if (!settings.showDepth)
                return;

            Rect depthRect = rect;
            depthRect.xMin = SceneVisibilityAndPickabilityControlXMax + DepthNumberMargin;
            EditorGUI.LabelField(depthRect, shiftCount.ToString(), new GUIStyle(EditorStyles.boldLabel));
        }

        private static void DrawComponents(Rect rect, Transform goTransform, bool isGroupHeader)
        {
            GUIStyle textStyle = EditorStyles.label;
            Vector2 textSize = textStyle.CalcSize(new GUIContent(goTransform.name));

            Component[] components = goTransform.GetComponents(typeof(Component));
            float shift = GetAllLabelsXMin(rect); 
            bool customComponentDisplayed = false; // only display iconless components once

            for (int i = 0; i < components.Length; i++)
            {
                if (components.Length <= 1) // if there is one component in the object just don't render any icon we know it's either transform or rect transform
                    break;

                if (components[i] == null) {
                    Debug.LogWarning($"Missing/Invalid script found at Gameobject ({goTransform.name}), Skipped drawing it's icon");
                    continue;
                }

                Type componentType = components[i].GetType();
                
                // Do not render transform or Rect Transform icons since we they are not that important to know
                if (componentType == typeof(Transform) || componentType == typeof(RectTransform))
                    continue;

                Texture componentIcon = EditorGUIUtility.ObjectContent(null, componentType).image;

                // components that have no icons are like custom mono scripts that uh....has no icon assigned
                if (settings.hideIconlessComponents && componentIcon == null)
                    continue;

                shift -= 20; // shift the position of the icon to the left from right 

                if (componentIcon == null && !customComponentDisplayed) {
                    componentIcon =  EditorGUIUtility.IconContent("cs Script Icon").image;
                    customComponentDisplayed = true; 
                } else if (componentIcon == null && customComponentDisplayed) { 
                    shift += 20; // if we already displayed the custom icon then shift back to the right since we are only displaying it once
                    continue; // skip drawing more than 1 custom component
                }

                Rect componentRect = rect;
                componentRect.xMin = shift;
                componentRect.size = new Vector2(rect.size.y, rect.size.y);

                if (isGroupHeader) // if we are drawing inside a group header then draw a gray background first so we can see the icons properly 
                    EditorGUI.DrawRect(componentRect, ComponentIconBackgroundColor);

                GUI.DrawTexture(componentRect, componentIcon);
            }
        }

        private static void DrawLineDivider(float labelXPos, Rect selectionRect)
        {
            EditorGUI.DrawRect(new Rect(labelXPos - DividerLineSpaceFromLabel, selectionRect.yMin, DividerLineWidth, selectionRect.size.y), DividerLineColor);
        }
        #endregion Visibility Controlled Properties =============================================================================================================


        #region Shape Draws =============================================================================================================
        private static void DrawHorizontalLine(Rect rect, bool hasChild, Color color)
        {
            rect.xMin -= ParentToChildLineXPosLeftShift;
            rect.yMin += ParentToChildVerticalLineYPosAddition;
            rect.size = new Vector2(hasChild ? ParentToChildLineLength : ParentToChildLineLength * ParentToLastIndexChildLineLenghtMultiplier, ParentToChildLineHeight);
            EditorGUI.DrawRect(rect, color);
        }

        private static void DrawVerticalLine(Rect rect, Color color)
        {
            Rect lineRect = rect;
            lineRect.xMin -= ParentToChildLineXPosLeftShift;
            lineRect.size = new Vector2(ParentToChildLineHeight, rect.size.y - ParentToChildLineLength);

            // extends the line further down to connect this item to it's sibling item gameobect
            if (HasNextSibling)
                lineRect.yMax = rect.yMax;

            EditorGUI.DrawRect(lineRect, color);
        }

        private static void DrawSiblingConnectionLines(Rect rect, int shiftCOunt, Transform goTransform)
        {
            Transform grandParent = goTransform.transform.parent.parent;
            Transform parent = goTransform.transform.parent;

            for (int i = 1; i < shiftCOunt; i++)
            {
                if (grandParent == null)
                    continue;

                if (parent == null)
                    continue;

                bool isNotLastIndex = parent.GetSiblingIndex() != grandParent.childCount-1;
                // exclude rendering lines from children that is an only child or at the last index of the child group since they got no sibling to connect
                // the line to
                // NOTE: only draws the connecting lines from first to last child

                // Logic for drawing lines this gameobject item to it's sibling 
                if ( grandParent.childCount > 1 && isNotLastIndex )
                {
                    // Calculate positioning and sizing values
                    Rect lineRect = rect;
                    lineRect.xMin -= ParentToChildLineXPosLeftShift + SameParentLineLeftShift * i;
                    lineRect.yMin = rect.yMin + SameParentLineYMargin;
                    lineRect.yMax = rect.yMax - SameParentLineYMargin;
                    lineRect.size = new Vector2(ParentToChildLineHeight, lineRect.size.y);

                    int selectedObjectDepth_Reverse = shiftCOunt - SelectedObjectItemDepth;

                    // Draws all the connecting lines of parent to child (even the imprecise ones that leads to nothing)
                    // EditorGUI.DrawRect(lineRect, SameParentLineColor_InActive ); 

                    // Draws all the connecting lines of parent to child (even the imprecise ones that leads to nothing)
                    // EditorGUI.DrawRect(lineRect, SelectedGameobjectItemDepth != -1 && i <= sameDepthAsSelectedObjectParent && IsSelectedObjectChild ? Color.yellow : SameParentLineColor_InActive ); 

                    // Colors the 
                    // if (i == sameDepthAsSelectedObjectParent){
                    //     EditorGUI.DrawRect( lineRect, 
                    //         SelectedGameobjectItemDepth != -1 && IsDirectSiblingOfSelectedObject
                    //         ? Color.yellow : SameParentLineColor_InActive 
                    //     );
                    // }

                    if (i == selectedObjectDepth_Reverse && IsSiblingOfSelectedObject && settings.highlightSelectedSiblings)
                    {
                        EditorGUI.DrawRect( lineRect, SameParentLineColor_Active);
                    } else {
                        EditorGUI.DrawRect( lineRect, SameParentLineColor_InActive ); 
                    }
                }
                    
                // climb up the relationship hierarchy
                grandParent = grandParent.parent;
                parent = parent.parent;
            }
        }

        #endregion Shape Draws =============================================================================================================

        public static T GetAsset_SO<T>(string soClassName, string soFileName) where T : ScriptableObject
        {
            string[] guid = AssetDatabase.FindAssets( $"t:{soClassName} {soFileName}" );

            if (guid.Length <= 0) 
                throw new Exception($"Asset ({soFileName}) with a type of ({soClassName}) NOT FOUND");
  
            string assetPath = AssetDatabase.GUIDToAssetPath(guid[0]);
            return AssetDatabase.LoadAssetAtPath<T>( assetPath );
        } 
    }
#endif
}
