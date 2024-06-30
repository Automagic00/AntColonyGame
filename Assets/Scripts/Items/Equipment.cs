using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Item/Equipment")]
public class Equipment : Item
{
    public enum Rarity
    {
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Mythic = 4,
        Legendary = 5
    }
    public enum Modifiers
    {
        StoneSkin = 0, //+Def -Spd
        FeatherWeight = 1, //-Def +Jmp
        SprintersSpikes = 2, //+Spd -Jmp
        GlassCanon = 3,//+Dmg -Hp
        RolyPolySoul = 4, //+RollSpd +Def -Dmg
        ScarabBeetleSoul = 5, //+Dmg +Kb -Spd
        WaspSoul = 6, //Spd+ DblJmp+ -Def
        Blunt = 7, //-Dmg +Kb
        Sharp = 8, //+Dmg -Kb
        Multishot = 9, //More Projectiles -Dmg
        Piercing = 10 //+Pierce , -Kb
    }
    //Base Variables
    public Rarity rarity = Rarity.Common;
    public List<Modifiers> baseModifiers = new List<Modifiers>();
    public List<Modifiers> modifiers = new List<Modifiers>();

    // Multiplicative
    public float groundspeedMod = 1;
    public float airspeedMod = 1;
    public float damageMod = 1;
    public float jumpHeightMod = 1;
    public float hpMod = 1;
    public float defMod = 1;
    public float knockbackMod = 1;
    public float rollSpeedMod = 1;
    public float attackSpeedMod = 1;


    // Additive
    public int doubleJumpMod = 0;
    public int multishotMod = 0;
    public int pierceMod = 0;

    public virtual void Init(Item item, List<Modifiers> _baseModifiers, float _groundSpeedMod, float _airSpeedMod, float _jumpSpeedMod, int _doubleJumpMod, float _damageMod, float _hpMod, float _defenseMod, float _knockbackMod, float _rollspeedMod, int _multishotMod, int _pierceMod, float _attackspeedMod)
    {
        baseModifiers = _baseModifiers;

        name = item.name;
        sprite = item.sprite;
        flavorText = item.flavorText;
        weight = item.weight;
        throwDamage = item.throwDamage;
        xSize = item.xSize;
        ySize = item.ySize;

        groundspeedMod = _groundSpeedMod;
        airspeedMod = _airSpeedMod;
        jumpHeightMod = _jumpSpeedMod;
        doubleJumpMod = _doubleJumpMod;
        damageMod = _damageMod;
        hpMod = _hpMod;
        defMod = _defenseMod;
        knockbackMod = _knockbackMod;
        rollSpeedMod = _rollspeedMod;
        multishotMod = _multishotMod;
        pierceMod = _pierceMod;
        attackSpeedMod = _attackspeedMod;
        
    }

    public static Equipment CreateEquipment(Equipment baseData)
    {
        var equipment = CreateInstance<Equipment>();
        equipment.Init(baseData,baseData.baseModifiers,baseData.groundspeedMod, baseData.airspeedMod, baseData.jumpHeightMod, baseData.doubleJumpMod, baseData.damageMod, baseData.hpMod, baseData.defMod, baseData.knockbackMod, baseData.rollSpeedMod, baseData.multishotMod, baseData.pierceMod, baseData.attackSpeedMod);

        return equipment;
    }

}
