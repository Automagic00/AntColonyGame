using System;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item")]
public class Item : ScriptableObject
{
    public new string name;
    public Sprite sprite;
    public string flavorText;
}

public class Ring : Equipment
{

}