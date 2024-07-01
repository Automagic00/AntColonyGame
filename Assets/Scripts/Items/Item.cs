using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item")]
public class Item : ScriptableObject
{
    public new string name;
    public Sprite sprite;
    public string flavorText;

    public float weight = 2;
    public float throwDamage = 0;

    public float xSize = 1, ySize = 1;

    private static List<Item> _items;
    public static List<Item> Items
    {
        get
        {
            if (_items == null)
            {
                _items = new List<Item>
                {
                    AssetDatabase.LoadAssetAtPath<Item>("Assets/Data/Items/Leaf.asset"),
                    AssetDatabase.LoadAssetAtPath<Item>("Assets/Data/Items/Pebble.asset"),
                    AssetDatabase.LoadAssetAtPath<Item>("Assets/Data/Items/Nectar.asset"),
                    AssetDatabase.LoadAssetAtPath<Item>("Assets/Data/Items/Bottlecap.asset"),
                    AssetDatabase.LoadAssetAtPath<Item>("Assets/Data/Items/Sugar Cube.asset"),
                };
            }
            return _items;
        }
    }
    private static List<Item> _valuables;
    public static List<Item> valuables
    {
        get
        {
            if (_valuables == null)
            {
                _valuables = new List<Item>
                {
                    AssetDatabase.LoadAssetAtPath<Weapon>("Assets/Data/Items/Weapons/Twig.asset"),
                    AssetDatabase.LoadAssetAtPath<Weapon>("Assets/Data/Items/Weapons/Needle.asset"),
                    AssetDatabase.LoadAssetAtPath<Weapon>("Assets/Data/Items/Weapons/Toothpick.asset"),
                    AssetDatabase.LoadAssetAtPath<Weapon>("Assets/Data/Items/Weapons/Paperclip.asset"),
                    AssetDatabase.LoadAssetAtPath<Weapon>("Assets/Data/Items/Weapons/Dental Pick.asset"),

                    AssetDatabase.LoadAssetAtPath<Ring>("Assets/Data/Items/Rings/Beetle Ring.asset"),
                    AssetDatabase.LoadAssetAtPath<Ring>("Assets/Data/Items/Rings/DoubleEdge Ring.asset"),
                    AssetDatabase.LoadAssetAtPath<Ring>("Assets/Data/Items/Rings/FeatherWeight Ring.asset"),
                    AssetDatabase.LoadAssetAtPath<Ring>("Assets/Data/Items/Rings/Multishot Ring.asset"),
                    AssetDatabase.LoadAssetAtPath<Ring>("Assets/Data/Items/Rings/RolyPolyRing.asset"),
                    AssetDatabase.LoadAssetAtPath<Ring>("Assets/Data/Items/Rings/Sturdy Ring.asset"),
                    AssetDatabase.LoadAssetAtPath<Ring>("Assets/Data/Items/Rings/Swift Ring.asset"),
                    AssetDatabase.LoadAssetAtPath<Ring>("Assets/Data/Items/Rings/Wasp Ring.asset"),
                };
            }
            return _valuables;
        }
    }
}
