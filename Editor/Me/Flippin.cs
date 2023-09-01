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


public static class Flippin
{
#if UNITY_EDITOR
    public static void FlippingINF(Vector2 position)
    {
        string[] guid = AssetDatabase.FindAssets( $"t:Texture INF_Logo_64x64" );

        if (guid.Length <= 0) 
            throw new System.Exception($"Me Logo Not Found!");
        
        string assetPath = AssetDatabase.GUIDToAssetPath(guid[0]);
        Texture inf = AssetDatabase.LoadAssetAtPath<Texture>( assetPath );

        float center = EditorGUIUtility.currentViewWidth / 2;
        // =============================================================================================================

        Vector2 logoSize = new Vector2(64, 64);
        Rect logoRect = new Rect(position.x, position.y, logoSize.x, logoSize.y);
        GUI.DrawTexture(logoRect, inf);

        // =============================================================================================================

        GUIStyle textStyle = new GUIStyle(EditorStyles.boldLabel);
        textStyle.fontSize = 15;
        float textWidth = 170;
        Rect textRect = new Rect(center, logoRect.yMax, 0, 0);

        textRect.height = 15;
        textRect.width = textWidth;
        textRect.x = logoRect.x - (textRect.width-10);
        textRect.y = textRect.y - textRect.height / 2;

        GUI.Label(textRect, "Created by:  (╯°□°）╯︵", textStyle);

        // =============================================================================================================

        // don't remove cause uhhhhh just don't
        // Rect lineRect = new Rect(textRect.x, textRect.yMax, textWidth -27, 2);
        // lineRect.xMin = textRect.xMin - 3;

        Rect lineRect = new Rect(0, textRect.yMax, EditorGUIUtility.currentViewWidth, 1);
        EditorGUI.DrawRect(lineRect, Color.gray);
    }
#endif
}
