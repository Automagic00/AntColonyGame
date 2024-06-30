using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Item/Weapon")]
public class Weapon : Equipment
{
    public enum WeaponType
    {
        Melee = 0,
        Ranged = 1,
        Hybrid = 2
    }

    public Vector2 holdPosition = Vector2.zero;
    public WeaponType weaponType = WeaponType.Melee;
    public float baseDamage = 0;
    //public float attackSpeed = 1;
    public Splash splash;
    public Projectile Projectile;


    private void WeponInit(Weapon baseData)
    {
        holdPosition = baseData.holdPosition;
        weaponType = baseData.weaponType;
        baseDamage = baseData.baseDamage;
        //attackSpeed = baseData.attackSpeed;
        splash = baseData.splash;
        Projectile = baseData.Projectile;
    }

    public static Weapon CreateWeapon(Weapon baseData)
    {
        var equipment = CreateInstance<Weapon>();
        equipment.Init(baseData,baseData.baseModifiers, baseData.groundspeedMod, baseData.airspeedMod, baseData.jumpHeightMod, baseData.doubleJumpMod, baseData.damageMod, baseData.hpMod, baseData.defMod, baseData.knockbackMod, baseData.rollSpeedMod, baseData.multishotMod, baseData.pierceMod, baseData.attackSpeedMod);
        equipment.WeponInit(baseData);

        return equipment;
    }

}
