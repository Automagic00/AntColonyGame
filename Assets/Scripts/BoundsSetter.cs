using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoundsSetter : MonoBehaviour
{
    // Start is called before the first frame update
    private void Awake()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        Vector3 worldmin = tilemap.transform.TransformPoint(tilemap.localBounds.min);
        Vector3 worldmax = tilemap.transform.TransformPoint(tilemap.localBounds.max) + new Vector3(0, 16, 0);

        Globals.mapBounds = new Bounds();
        Globals.mapBounds.SetMinMax(worldmin, worldmax);
        Debug.Log(Globals.mapBounds);
    }

}
