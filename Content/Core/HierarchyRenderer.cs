// Copyright (C) 2023 INF

// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
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
        public const string EditorOnlyTag = "EditorOnly";
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
        private const int DepthNumberMargin = 5;
        private const float ParentToChildLineXPosLeftShift = 22.5f;
        private const float ParentToChildLineHeight = 2;
        private const int ParentToChildVerticalLineWidth = 8;
        private const int ParentToChildVerticalLineYPosAddition = 7;
        private const int DividerLineWidth = 1;
        private const int DividerLineSpaceFromLabel = 5;
        private static readonly Vector2 staticIndicatorSize = new Vector2(8, 8);
        private const float LabelFixedWidth = 90;
        private static MyHierarchySettings settings;

        static HierarchyRenderer() => EditorApplication.hierarchyWindowItemOnGUI += OnGameObjectItemRender;
        static void OnGameObjectItemRender(int instanceID, Rect selectionRect)
        {
            if (settings == null)
                settings = GetAsset_SO<MyHierarchySettings>("MyHierarchySettings", "My Hierarchy Settings");

            if (!settings.activate)
                return;

            GameObject go = (GameObject)EditorUtility.InstanceIDToObject(instanceID);
            if (go == null)
                return;

            if (go.TryGetComponent<MyHierarcyHeader>(out MyHierarcyHeader header))
            {
                DrawHeader(header, selectionRect, go);
                return;
            }

            if (go.TryGetComponent<MyHierarchyGroup>(out MyHierarchyGroup group))
                DrawGroupHeader(selectionRect, go, group);
 
            // draw only on gameobjects that is parented
            if (selectionRect.xMin > HierarchyRootItemXMin)
                DrawParentToChildLines(selectionRect, go);

            DrawIsStaticLabel(selectionRect, go.isStatic);
            DrawLayerLabel(selectionRect, go.layer);
            DrawTagLabel(selectionRect, go.tag);
            // DrawRectXMax(selectionRect);
            // DrawRectXMin(selectionRect);
            // DrawRectYMin(selectionRect);
            // DrawRectYMax(selectionRect);
        }

        private static void DrawParentToChildLines( Rect rect, GameObject go )
        {
            DrawVerticalLine(rect);
            DrawHorizontalLine(rect, go.transform.childCount > 0);
            DrawDepth(rect);
        }

        #region Header Draws ============================================================================================================================
        private static void DrawGroupHeader( Rect rect, GameObject go, MyHierarchyGroup groupHeader )
        {
            GUIStyle groupLabelStyle = new GUIStyle(EditorStyles.label);
            groupLabelStyle.normal.textColor = groupHeader.fontColor;
            groupLabelStyle.fontStyle = settings.groupFontStyle;

            float dividerSpaceMultiplier = 0;
            dividerSpaceMultiplier += Convert.ToInt32(settings.showLayers) * 2; // each label contains a space at each side
            dividerSpaceMultiplier += Convert.ToInt32(settings.showStaticObjects) * 2;
            dividerSpaceMultiplier += Convert.ToInt32(settings.showTags) * 2;

            float labelWdithMultiplier = 0;
            labelWdithMultiplier += Convert.ToInt32(settings.showLayers); // each label contains a space at each side
            labelWdithMultiplier += Convert.ToInt32(settings.showTags);

            Rect allLabelsRect = rect;
            allLabelsRect.xMin = rect.xMax - (LabelFixedWidth * labelWdithMultiplier) - // x pos
            (settings.showStaticObjects ? staticIndicatorSize.x : 0) - (DividerLineSpaceFromLabel * dividerSpaceMultiplier); // -> indicator width;
            allLabelsRect.size = new Vector2(LabelFixedWidth, allLabelsRect.size.y);
            rect.xMax = allLabelsRect.xMin;

            EditorGUI.DrawRect(rect, groupHeader.backgroundColor);
            EditorGUI.LabelField(rect, " " + go.name, groupLabelStyle);
        }

        private static void DrawHeader(MyHierarcyHeader header, Rect rect, GameObject go)
        {
            GUIStyle headerStyle = new GUIStyle(EditorStyles.label);
            headerStyle.normal.textColor = header.fontColor;
            headerStyle.alignment = settings.headerAlignment.ToTextAnchor();
            headerStyle.fontStyle = settings.headerFontStyle;

            rect.xMin = SceneVisibilityAndPickabilityControlXMax;
            rect.xMax = rect.xMax + HierarchyItemRectRightMargin;

            if (go.transform.childCount > 0 || go.transform.parent != null)
            {
                EditorGUI.DrawRect(rect, Color.red);
                headerStyle.fontStyle = FontStyle.Bold;
                headerStyle.normal.textColor = Color.white;
               
                if (go.transform.childCount > 0)  {
                    EditorGUI.LabelField(rect, "Headers shouldn't have child gameobjects".ToUpper() , headerStyle);
                    Debug.LogError($"My Hierarychy Header's shouldn't have child gameobjects!, Unparent all gameobjects from ({header.m_name})");
                } else {
                    EditorGUI.LabelField(rect, "Headers shouldn't be parented".ToUpper() , headerStyle);
                    Debug.LogError($"My Hierarychy Header's shouldn't parented to other gameobject, Unparent from ({header.m_name})");
                }
            } else {
                EditorGUI.DrawRect(rect, header.backgroundColor);
                EditorGUI.LabelField(rect, string.Format(" {0} ", header.m_name), headerStyle);                
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
            if (!settings.showStaticObjects)
                return;

            Rect indicatorRect = rect;

            indicatorRect.xMin = rect.xMax - staticIndicatorSize.x - DividerLineSpaceFromLabel; // x pos
            indicatorRect.yMin = rect.yMin +  ( (rect.yMax - rect.yMin) / 4 ); // y pos
            indicatorRect.size = staticIndicatorSize;

            EditorGUI.DrawRect(indicatorRect, isStatic ? new Color(0.4f, 0.4f, 0.4f, 1) : new Color(0, 0.75f, 0, 1));

            DrawLineDivider(indicatorRect.x, rect);
        }

        
        private static void DrawLayerLabel(Rect rect, LayerMask layer)
        {
            if (!settings.showLayers)
                return;

            string layerString = LayerMask.LayerToName(layer);
            Rect layerRect = rect;

            layerRect.xMin = rect.xMax - LabelFixedWidth - // x pos
            (settings.showStaticObjects ? staticIndicatorSize.x  : 0) - (DividerLineSpaceFromLabel * (settings.showStaticObjects ? 3 : 1)); // -> indicator width;
            layerRect.size = new Vector2(LabelFixedWidth, layerRect.size.y);

            EditorGUI.LabelField(layerRect, layerString, new GUIStyle(EditorStyles.label));
            DrawLineDivider(layerRect.xMin, rect);
        }

        private static void DrawTagLabel(Rect rect, string tag)
        {
            if (!settings.showTags)
                return;

            float dividerSpaceMultiplier = 1;
            dividerSpaceMultiplier += Convert.ToInt32(settings.showLayers) * 2; // each label contains a space at each side
            dividerSpaceMultiplier += Convert.ToInt32(settings.showStaticObjects) * 2;

            Rect tagRect = rect;
            tagRect.xMin = rect.xMax - (LabelFixedWidth * (settings.showLayers ? 2 : 1)) - // x pos
            (settings.showStaticObjects ? staticIndicatorSize.x : 0) - (DividerLineSpaceFromLabel * dividerSpaceMultiplier); // -> indicator width;
            tagRect.size = new Vector2(LabelFixedWidth, tagRect.size.y);

            EditorGUI.LabelField(tagRect, tag, new GUIStyle(EditorStyles.label));
            DrawLineDivider(tagRect.xMin, rect);
        }

        private static void DrawDepth(Rect rect) // draws the number on how deep the gameobject in the hierarchy 
        {
            if (!settings.showDepth)
                return;

            float nameXRightPosShift = rect.xMin - HierarchyRootItemXMin; 
            float shiftCount = nameXRightPosShift / HierarchyItemXShiftPerDepth;
            rect.xMin = SceneVisibilityAndPickabilityControlXMax + DepthNumberMargin;
            EditorGUI.LabelField(rect, shiftCount.ToString(), new GUIStyle(EditorStyles.boldLabel));
        }

        private static void DrawLineDivider(float labelXPos, Rect selectionRect)
        {
            EditorGUI.DrawRect(new Rect(labelXPos - DividerLineSpaceFromLabel, selectionRect.yMin, DividerLineWidth, selectionRect.size.y), Color.gray);
        }
        #endregion Visibility Controlled Properties =============================================================================================================


        #region Shape Draws =============================================================================================================
        private static void DrawHorizontalLine(Rect rect, bool hasChild)
        {
            rect.xMin -= ParentToChildLineXPosLeftShift;
            rect.yMin += ParentToChildVerticalLineYPosAddition;
            rect.size = new Vector2(hasChild ? ParentToChildVerticalLineWidth : ParentToChildVerticalLineWidth * 2.5f, ParentToChildLineHeight);
            EditorGUI.DrawRect(rect, Color.gray);
        }

        private static void DrawVerticalLine(Rect rect)
        {
            rect.xMin -= ParentToChildLineXPosLeftShift;
            rect.size = new Vector2(ParentToChildLineHeight, rect.size.y - ParentToChildVerticalLineWidth);
            EditorGUI.DrawRect(rect, Color.gray);
        }
        #endregion Shape Draws =============================================================================================================

        public static T GetAsset_SO<T>(string soClassName, string soFileName) where T : ScriptableObject
        {
            string[] guid = AssetDatabase.FindAssets( $"t:{soClassName} {soFileName}" );

            if (guid.Length <= 0) 
                throw new System.Exception($"Asset ({soFileName}) with a type of ({soClassName}) NOT FOUND");
  
            string assetPath = AssetDatabase.GUIDToAssetPath(guid[0]);
            return AssetDatabase.LoadAssetAtPath<T>( assetPath );
        } 
    }
#endif
}



