using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Item/Equipment")]
public class Equipment : Item
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
