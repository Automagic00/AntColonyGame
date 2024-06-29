using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateProjectile : MonoBehaviour
{
    //public Projectile proj;
    public static void Create(Projectile proj, Entity owner, Vector2 angle)
    {

        GameObject projObj = new GameObject(proj.name);
        projObj.transform.position = owner.transform.position;

        // Assign Hitbox Data
        HitboxData data = projObj.AddComponent<HitboxData>();
        data.damage = proj.damage;
        data.knockback = proj.knockback;
        data.pierce = proj.pierce;
        data.owner = owner.gameObject;

        if (angle == Vector2.zero)
        {
            projObj.transform.localScale = proj.size * new Vector3(Mathf.Sign(owner.transform.localScale.x), 1, 1);
            angle = new Vector2(Mathf.Sign(owner.transform.localScale.x), 0);
        }
        else
        {
            float angleRotation = Vector2.Angle(Vector2.right, angle);
            projObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angleRotation));
        }

        projObj.tag = "Projectile";
        projObj.layer = 9;

        BoxCollider2D hitbox = projObj.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;
        hitbox.size = proj.size;

        
        //Assign Velo
        Rigidbody2D rb = projObj.AddComponent<Rigidbody2D>();
        
        rb.velocity = new Vector2( proj.speed * angle.x , proj.speed * angle.y);
        rb.drag = 0;
        rb.gravityScale = 0;

        //Assign sprite
        SpriteRenderer spriteRenderer = projObj.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = proj.sprites[Random.Range(0,proj.sprites.Length)];

        //Assign projectile lifetime
        ProjectileLife projLife = projObj.AddComponent<ProjectileLife>();
        projLife.StartCoroutine(projLife.Lifetime(proj.lifetime));
        projLife.SetPierce(proj.pierce);
        projLife.SetOwner(owner.gameObject);

    }
}
