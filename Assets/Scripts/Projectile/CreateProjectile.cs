using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateProjectile : MonoBehaviour
{
    //public Projectile proj;
    public static void Create(Projectile proj, Entity owner)
    {

        GameObject projObj = new GameObject(proj.name);
        projObj.transform.position = owner.transform.position;
        projObj.transform.localScale = proj.size * new Vector3(Mathf.Sign(owner.transform.localScale.x),1,1);
        projObj.tag = "Projectile";

        BoxCollider2D hitbox = projObj.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;

        //Assign Hitbox Data
        HitboxData data = projObj.AddComponent<HitboxData>();
        data.damage = proj.damage;
        data.knockback = proj.knockback;
        data.pierce = proj.pierce;
        data.owner = owner.gameObject;
        
        //Assign Velo
        Rigidbody2D rb = projObj.AddComponent<Rigidbody2D>();
        rb.velocity = new Vector2(proj.speed * Mathf.Sign(owner.transform.localScale.x), 0);
        rb.drag = 0;
        rb.gravityScale = 0;

        //Assign sprite
        SpriteRenderer spriteRenderer = projObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = proj.sprite;

        //Assign projectile lifetime
        ProjectileLife projLife = projObj.AddComponent<ProjectileLife>();
        projLife.StartCoroutine(projLife.Lifetime(proj.lifetime));
        projLife.SetPierce(proj.pierce);
        projLife.SetOwner(owner.gameObject);

    }
}
