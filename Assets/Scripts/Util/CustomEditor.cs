#if (UNITY_EDITOR) 

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemBehavior))]
public class CustomInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (target is ItemBehavior)
        {
            // Trigger getter to refresh sprite
            ItemBehavior item = (ItemBehavior)target;
            item.item = item.item;
        }
    }
}

#endif