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
    public float attackSpeed = 1;
    public Splash splash;
    public Projectile Projectile;
}
