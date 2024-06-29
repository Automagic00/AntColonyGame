using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLife : MonoBehaviour
{

    GameObject owner;
    List<GameObject> entitiesHit = new List<GameObject>();
    int pierce = 0;
    public IEnumerator Lifetime(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        DestroyProjectile();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != owner.tag && !entitiesHit.Contains(collision.gameObject) && collision.tag != "Projectile")
        {
            if (collision.GetComponent<Entity>() != null && collision.GetComponent<Entity>().GetInvuln() == false)
            {
                entitiesHit.Add(collision.gameObject);
                pierce--;
            }
        }

        if (collision.tag == "Ground")
        {
            DestroyProjectile();
        }

        if (pierce < 0)
        {
            DestroyProjectile();
        }
    }


    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }

    public void SetPierce(int pierceIn)
    {
        pierce = pierceIn;
    }
    public void SetOwner(GameObject ownerIn)
    {
        owner = ownerIn;
    }
}
