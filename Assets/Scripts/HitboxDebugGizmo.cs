using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxDebugGizmo : MonoBehaviour
{
    public Vector2 origin;
    public Vector2 size;

    public HitboxDebugGizmo(Vector2 originIn, Vector2 sizeIn)
    {
        origin = originIn;
        size = sizeIn;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(origin.x, origin.y, 0), new Vector3(size.x, size.y, 1));
    }

}
