using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NewBehaviourScript : MonoBehaviour
{


    public GameObject[] gameStatePrefabs;
    private Tilemap fg, bg;

    void Awake()
    {

        // Set camera bounds to tilemap
        Tilemap tilemap = transform.Find("Tiles").GetComponent<Tilemap>();

        Vector3 worldmin = tilemap.transform.TransformPoint(tilemap.localBounds.min);
        Vector3 worldmax = tilemap.transform.TransformPoint(tilemap.localBounds.max) + new Vector3(0, 16, 0);

        Globals.mapBounds = new Bounds();
        Globals.mapBounds.SetMinMax(worldmin, worldmax);
    }
    void Start()
    {
        bg = transform.Find("BGTiles").GetComponent<Tilemap>();
        fg = transform.Find("Tiles").GetComponent<Tilemap>();


        // Update map when game progresses
        Globals.addProgressionListener(updateMap);
        updateMap();
    }

    void updateMap()
    {
        if (Globals.gameProgression >= 0 && Globals.gameProgression < gameStatePrefabs.Length
        && gameStatePrefabs[Globals.gameProgression] != null)
            add(gameStatePrefabs[Globals.gameProgression]);
    }

    void add(GameObject prefab)
    {
        Tilemap addBG = prefab.transform.Find("pfBGTiles").GetComponent<Tilemap>();
        Tilemap addFG = prefab.transform.Find("pfTiles").GetComponent<Tilemap>();
        addTilemaps(addBG, bg);
        addTilemaps(addFG, fg);

        Transform addObjects = prefab.transform.Find("objects");
        if (addObjects != null)
            for (int i = 0; i < addObjects.childCount; i++)
            {
                Instantiate(addObjects.GetChild(i));
            }

        Transform removeObjects = prefab.transform.Find("removeObjects");
        if (removeObjects != null)
            for (int i = 0; i < addObjects.childCount; i++)
            {
                string removeTarget = removeObjects.GetChild(i).name + "(Clone)";
                Destroy(GameObject.Find(removeTarget));
            }
    }

    private void addTilemaps(Tilemap fromMap, Tilemap toMap)
    {
        BoundsInt bounds = fromMap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = fromMap.GetTile(pos);
                if (tile == null) continue;

                if (tile.name == "removetile")
                    toMap.SetTile(pos, null);
                else
                    toMap.SetTile(pos, tile);

            }
    }
}
