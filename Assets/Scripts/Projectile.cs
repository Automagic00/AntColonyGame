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

    public Sprite sprite;
    
}
