using System;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item")]
public class Item : ScriptableObject
{
    public new string name;
    public Sprite sprite;
    public string flavorText;

    public float weight = 2;
    public float throwDamage = 0;
}

public class Ring : Equipment
{

}