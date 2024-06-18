using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxData
{
    public Vector2 origin;
    public Vector2 size;
    public float duration;
    public float damage;
    public float knockback;

    public HitboxData(Vector2 originIn,Vector2 sizeIn, float durationIn, float dmgIn, float kbIn)
    {
        origin = originIn;
        size = sizeIn;
        duration = durationIn;
        damage = dmgIn;
        knockback = kbIn;
    }


}

public class Hitbox : MonoBehaviour
{
    static MonoBehaviour instance;
    private void Awake()
    {
        instance = this;
    }
    public static void CreateHitbox(HitboxData hitboxData, Entity owner)
    {
        GameObject hitboxObj = new GameObject("Hitbox");
        hitboxObj.transform.position = owner.transform.position;
        hitboxObj.transform.localScale = hitboxData.size;
        BoxCollider2D hitbox = hitboxObj.AddComponent<BoxCollider2D>();

        var gizmo = hitboxObj.AddComponent<HitboxDebugGizmo>();
        gizmo.origin = hitboxData.origin;
        gizmo.size = hitboxData.size;

        /*SpriteRenderer debugImage = hitboxObj.AddComponent<SpriteRenderer>();
        debugImage.size = hitboxData.size;
        Texture2D tex;
        debugImage.sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height),new Vector2(0.5f, 0.5f),100f);*/


        //hitbox.offset = hitboxData.origin;
        //hitbox.size = hitboxData.size;
        hitbox.isTrigger = true;

        owner.StartCoroutine(owner.DestroyHitbox(hitboxData.duration, hitbox));

        //Gizmos.DrawCube(new Vector3(hitboxData.origin.x, hitboxData.origin.y, 0), new Vector3(hitboxData.size.x, hitboxData.size.y, 1));
    }

    /*public void StartDestroyRoutine(float lifetime, BoxCollider2D hitbox)
    {
        StartCoroutine(DestroyHitbox(lifetime, hitbox));
    }
    IEnumerator DestroyHitbox(float lifetime, BoxCollider2D hitbox)
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(hitbox);
    }*/
}
