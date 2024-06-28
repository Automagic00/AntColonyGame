using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Splash", menuName = "Splash")]
public class Splash : ScriptableObject
{
    public Material splashMaterial;
    public Vector2 splashScale = Vector2.one;
    public float speed = 0.25f;
    
}
