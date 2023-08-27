using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
#if UNITY_EDITOR
using UnityEditor;
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
        public const string editorOnlyTag = "EditorOnly";
        private const int rightSidePadding = 15;

        /// <summary>
        /// how much the x position of the hierarchy's item rect shifts to the right side per depth  
        /// a level 1 depth means having 1 parent will shift the hierarchy's item rect X pos to this amount to the right
        /// </summary>
        private const int HierarchyItemXShiftPerDepth = 14;
        private const int LayersLabelMaxLenght = 14;
        private const int SceneVisibilityAndPickabilityControlXMax = 32;

        /// <summary>
        /// The x position of the hierarchy item at the root level of hierarchy, further parented to other objects get's will make their x position shift to the right
        /// </summary>
        private const int HierarchyItemRootXPos = 60;
        private const int DepthNumberMargin = 12;
        private const float ParentToChildLineXPosLeftShift = 22.5f;
        private const float ParentToChildLineWidth = 2;
        private const int ParentToChildVerticalLineLenght = 8;
        private const int ParentToChildVerticalLineYPosAddition = 7;
        private const int DividerWidth = 1;
        private const int Marker1Pos_StaticInActive = 80;
        private const int Marker2Pos_InStaticActive = 180;
        private const int Marker1Pos_StaticActive = 100;
        private const int Marker2Pos_StaticActive = 200;
        private const int DividerLeftPosShift = 5;
        private static MyHierarchySettings settings;

        static HierarchyRenderer() => EditorApplication.hierarchyWindowItemOnGUI += OnGameObjectItemRender;

        static void OnGameObjectItemRender(int instanceID, Rect selectionRect)
        {
            if (settings == null)
                settings = GetAsset_SO<MyHierarchySettings>("MyHierarchySettings", "My Hierarchy Settings");

            GameObject go = (GameObject)EditorUtility.InstanceIDToObject(instanceID);
            if (go == null)
                return;

            if (go.TryGetComponent<MyHierarcyHeader>(out MyHierarcyHeader header))
            {
                DrawHeader(header, new Rect(selectionRect), go);
                return;
            }

            if (go.TryGetComponent<MyHierarchyGroup>(out MyHierarchyGroup group))
                DrawGroupHeader(new Rect(selectionRect), go, group);
 
            if (go.transform.parent != null)
                DrawParentToChildLines(new Rect(selectionRect), go);

            DrawIsStatic(new Rect(selectionRect), go.isStatic);
            DrawLayer(new Rect(selectionRect), go.layer);
            DrawTag(new Rect(selectionRect), go.tag);
        }

        private static void DrawParentToChildLines( Rect rect, GameObject go )
        {
            DrawVerticalLine(new Rect(rect));
            DrawHorizontalLine(new Rect(rect));
            DrawDepth(new Rect(rect));
        }

        #region Header Draws ============================================================================================================================
        private static void DrawGroupHeader( Rect rect, GameObject go, MyHierarchyGroup groupHeader )
        {
            GUIStyle gs = new GUIStyle(EditorStyles.label);
            gs.normal.textColor = groupHeader.fontColor;
            gs.fontStyle = settings.groupFontStyle;

            rect.xMax = 
                (rect.xMax - (Marker2Pos_StaticActive + DividerLeftPosShift + DividerWidth) ) + 
                (( Convert.ToInt32( !settings.showLayers ) ) * Marker1Pos_StaticActive) + 
                (( Convert.ToInt32( !settings.showTags ) ) * Marker1Pos_StaticActive) + 
                (( Convert.ToInt32( !settings.showStaticObjects ) ) * 20 );

            EditorGUI.DrawRect(rect, groupHeader.backgroundColor);
            EditorGUI.LabelField(rect, " " + go.name, gs);
        }

        private static void DrawHeader(MyHierarcyHeader header, Rect rect, GameObject go)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = header.fontColor;
            style.alignment = settings.headerAlignment.ToTextAnchor();
            style.fontStyle = settings.headerFontStyle;

            rect.size = new Vector2(rect.size.x + rightSidePadding, rect.size.y);
            rect.xMin = SceneVisibilityAndPickabilityControlXMax;
            

            if (go.transform.childCount > 0 || go.transform.parent != null)
            {
                EditorGUI.DrawRect(rect, Color.red);
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.white;
               
                if (go.transform.childCount > 0)  {
                    EditorGUI.LabelField(rect, "Headers shouldn't have child gameobjects".ToUpper() , style);
                    Debug.LogError($"My Hierarychy Header's shouldn't have child gameobjects!, Unparent all gameobjects from ({header.m_name})");
                } else {
                    EditorGUI.LabelField(rect, "Headers shouldn't be parented".ToUpper() , style);
                    Debug.LogError($"My Hierarychy Header's shouldn't parented to other gameobject, Unparent from ({header.m_name})");
                }
            } else {
                EditorGUI.DrawRect(rect, header.backgroundColor);
                EditorGUI.LabelField(rect, string.Format(" {0} ", header.m_name), style);                
            }
        }
        #endregion Header Draws ============================================================================================================================

        #region Visibility Controlled Properties =============================================================================================================
        private static void DrawIsStatic(Rect rect, bool isStatic) 
        {   
            if (!settings.showStaticObjects)
                return;

            rect.x = rect.xMax;
            rect.y = rect.y + 4;
            DrawDivider(new Rect(rect), rect.x - DividerLeftPosShift);
            rect.size = new Vector2(8, 8);
            EditorGUI.DrawRect(rect, isStatic ? new Color(0.4f, 0.4f, 0.4f, 1) : new Color(0, 0.75f, 0, 1));
        }
        
        private static void DrawLayer(Rect rect, LayerMask layer)
        {
            if (!settings.showLayers)
                return;

            string layerString = LayerMask.LayerToName(layer);
            rect.x = rect.xMax - (settings.showStaticObjects ? Marker1Pos_StaticActive : Marker1Pos_StaticInActive);
            DrawDivider(new Rect(rect), rect.x - DividerLeftPosShift);
            int stringMaxLenght = layerString.Length <= LayersLabelMaxLenght ? layerString.Length : LayersLabelMaxLenght;
            EditorGUI.LabelField(rect, layerString.Substring(0, stringMaxLenght), new GUIStyle(EditorStyles.label));
        }

        private static void DrawTag(Rect rect, string tag)
        {
            if (!settings.showTags)
                return;

            float shift = 0;

            if (settings.showLayers && settings.showTags){
                if (settings.showStaticObjects) {
                    shift = Marker2Pos_StaticActive;
                } else {
                    shift = Marker2Pos_InStaticActive;
                } 
            }
            else{
                if (settings.showStaticObjects) {
                    shift = Marker1Pos_StaticActive;
                } else {
                    shift = Marker1Pos_StaticInActive;
                } 
            }

            rect.x = rect.xMax - shift;
            DrawDivider(new Rect(rect), rect.x - DividerLeftPosShift);
            int stringMaxLenght = tag.Length <= LayersLabelMaxLenght ? tag.Length : LayersLabelMaxLenght; 
            EditorGUI.LabelField(rect, tag.Substring(0, stringMaxLenght), new GUIStyle(EditorStyles.label));
        }

        private static void DrawDepth(Rect rect) // draws the number on how deep the gameobject in the hierarchy 
        {
            if (!settings.showDepth)
                return;

            float xMinPosShift = rect.x - HierarchyItemRootXPos; 
            float shiftCount = xMinPosShift / HierarchyItemXShiftPerDepth;
            rect.x = SceneVisibilityAndPickabilityControlXMax + xMinPosShift - DepthNumberMargin;

            // only display the hierarchy depth of those gameobjects above 0
            if (shiftCount > 0)
                EditorGUI.LabelField(rect, shiftCount.ToString(), new GUIStyle(EditorStyles.boldLabel));
        }
        #endregion Visibility Controlled Properties =============================================================================================================


        #region Shape Draws =============================================================================================================

        private static void DrawDivider(Rect rect, float xToLeftPosShift) // Draws the vertical line divers 
        {
            rect.x = xToLeftPosShift;
            rect.size = new Vector2(DividerWidth, rect.size.y);
            EditorGUI.DrawRect(rect, Color.gray);
        }

        private static void DrawHorizontalLine(Rect rect)
        {
            rect.x -= ParentToChildLineXPosLeftShift;
            rect.y += ParentToChildVerticalLineYPosAddition;
            rect.size = new Vector2(ParentToChildVerticalLineLenght, ParentToChildLineWidth);
            EditorGUI.DrawRect(rect, Color.gray);
        }

        private static void DrawVerticalLine(Rect rect)
        {
            rect.x -= ParentToChildLineXPosLeftShift;
            rect.size = new Vector2(ParentToChildLineWidth, rect.size.y - ParentToChildVerticalLineLenght);
            EditorGUI.DrawRect(rect, Color.grey);
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



