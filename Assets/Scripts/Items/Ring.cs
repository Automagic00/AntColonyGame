using System;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Item/Ring")]
public class Ring : Equipment
{
    public static Ring CreateRing(Ring baseData)
    {
        var equipment = CreateInstance<Ring>();
        equipment.Init(baseData, baseData.baseModifiers, baseData.groundspeedMod, baseData.airspeedMod, baseData.jumpHeightMod, baseData.doubleJumpMod, baseData.damageMod, baseData.hpMod, baseData.defMod, baseData.knockbackMod, baseData.rollSpeedMod, baseData.multishotMod, baseData.pierceMod, baseData.attackSpeedMod);
        return equipment;
    }
}