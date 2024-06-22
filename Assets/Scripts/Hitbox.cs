using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HitboxData : MonoBehaviour
{
    /*public class HitboxDataStruct
    {
        public Vector2 origin;
        public Vector2 size;
        public float duration;
        public float damage;
        public float knockback;
    }*/

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

    /*public HitboxData(HitboxDataStruct structIn)
    {
        origin = structIn.origin;
        size = structIn.size;
        duration = structIn.duration;
        damage = structIn.damage;
        knockback = structIn.knockback;
    }*/


}

public class Hitbox : MonoBehaviour
{
    static HitboxData hitboxData;
    public static void CreateHitbox(HitboxData hitboxDataIn, Entity owner)
    {
        //Assign Hitbox Data
        hitboxData = hitboxDataIn;

        //Create Hitbox Object and Trigger
        GameObject hitboxObj = new GameObject("Hitbox");
        hitboxObj.transform.position = owner.transform.position + new Vector3 (hitboxData.origin.x * Mathf.Sign(owner.transform.localScale.x), hitboxData.origin.y,0);
        hitboxObj.transform.localScale = hitboxData.size;
        hitboxObj.tag = "Hitbox";
        hitboxObj.transform.SetParent(owner.transform);
        BoxCollider2D hitbox = hitboxObj.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;

        //Assign Hitbox Data
        HitboxData data = hitboxObj.AddComponent<HitboxData>();
        data.damage = hitboxData.damage;
        data.knockback = hitboxData.knockback;
        

        //Debug Visual
        var gizmo = hitboxObj.AddComponent<HitboxDebugGizmo>();
        gizmo.origin = hitboxData.origin;
        gizmo.size = hitboxData.size;

        //Hitbox Lifetime
        owner.StartCoroutine(owner.DestroyHitbox(hitboxData.duration, hitbox));
    }

    public HitboxData GetHitboxData()
    {
        return hitboxData;
    }
}
