using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile", menuName = "Projectile")]
public class Projectile : ScriptableObject
{
    public new string name;
    public float damage;
    public float knockback;
    public Vector2 size;
    public float speed;
    public int pierce; //How many enemies will it pierce
    public int projFired = 1;
    public float multiProjSpread;
    public float lifetime;

    public Sprite[] sprites;

    private void Init(Projectile baseData)
    {
        name = baseData.name;
        damage = baseData.damage;
        knockback = baseData.knockback;
        size = baseData.size;
        speed = baseData.speed;
        pierce = baseData.pierce;
        projFired = baseData.projFired;
        multiProjSpread = baseData.multiProjSpread;
        lifetime = baseData.lifetime;

        sprites = baseData.sprites;
    }

    public static Projectile InitProjectile(Projectile baseData)
    {
        var proj = CreateInstance<Projectile>();
        proj.Init(baseData);

        return proj;
    }
}
