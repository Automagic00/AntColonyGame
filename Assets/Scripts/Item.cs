

using System;
using UnityEngine;



[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public new string name;
    public Sprite sprite;

}


public abstract class Equipment : Item
{

    // Multiplicative
    public float groundspeedMod = 1;
    public float airspeedMod = 1;
    public float damageMod = 1;
    public float jumpHeightMod = 1;
    public float hpMod = 1;


    // Additive
    public int doubleJumpMod = 0;


}

public abstract class Weapon : Equipment
{

}

public abstract class Ring : Equipment
{

}