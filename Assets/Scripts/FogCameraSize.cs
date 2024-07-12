using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogCameraSize : MonoBehaviour
{
    public Material FogMat;
    private Camera cam;
    private float projectionSizeMult = 21.3333f;
    private float fogSizeMult = 2;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(Globals.mapBounds.center.x, Globals.mapBounds.center.y, -14);

        float xBoundSize = Globals.mapBounds.max.x - Globals.mapBounds.min.x;
        float yBoundSize = Globals.mapBounds.max.y - Globals.mapBounds.min.y;

        if (xBoundSize > yBoundSize)
        {
            float scalar = (xBoundSize / 100) * 2.75f;
            transform.localScale = new Vector3(scalar, scalar, 1);
            FogMat.SetFloat("_Radius", 0.01f + (scalar / 1000));
        }
        else
        {
            float scalar = (yBoundSize / 100) * 2.75f;
            transform.localScale = new Vector3(scalar, scalar, 1);
            FogMat.SetFloat("_Radius", 0.01f + (scalar / 1000));
        }
        
        cam = GetComponent<Camera>();
        cam.orthographicSize = transform.localScale.x * projectionSizeMult;


    }
}
