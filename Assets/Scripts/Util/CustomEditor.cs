
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
            ItemBehavior item = (ItemBehavior)target;
            item.item = item.item;
            Debug.Log(item.item);
        }
    }
}