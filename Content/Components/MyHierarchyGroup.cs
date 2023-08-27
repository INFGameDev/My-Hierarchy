using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class MyHierarchyGroup : MonoBehaviour
{
    public Color fontColor = Color.white;
    public Color backgroundColor = Color.gray;

    #if UNITY_EDITOR
	[MenuItem("GameObject/My Hierarychy/Group", false, 9)]
    public static void CreateHeaderGroup(MenuCommand menu)
    {
        GameObject go = new GameObject();
        go.AddComponent<MyHierarchyGroup>();
        GameObjectUtility.SetParentAndAlign(go, menu.context as GameObject);
        Undo.RegisterCreatedObjectUndo(go, "Created MyHierarchy GO: " + go.name);
        Selection.activeObject = go;


        EditorApplication.RepaintHierarchyWindow();
    }
    #endif

    void OnValidate()
    {
#if UNITY_EDITOR
        EditorApplication.delayCall += () =>
        {
            if (this == null)
                return;

            EditorApplication.RepaintHierarchyWindow();
        };
#endif
    }
}
