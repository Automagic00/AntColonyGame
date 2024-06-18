using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NewBehaviourScript : MonoBehaviour
{

    public GameObject loungePf, nextRoomPf;
    private Tilemap fg, bg;

    void Awake()
    {
        Globals.addProgressionListener(updateMap);
    }
    void Start()
    {
        bg = transform.Find("BGTiles").GetComponent<Tilemap>();
        fg = transform.Find("Tiles").GetComponent<Tilemap>();
    }

    void updateMap()
    {
        if (Globals.gameProgression == 1)
            add(loungePf);
        if (Globals.gameProgression == 2)
            add(nextRoomPf);

    }

    void add(GameObject prefab)
    {
        Tilemap addBG = prefab.transform.Find("pfBGTiles").GetComponent<Tilemap>();
        Tilemap addFG = prefab.transform.Find("pfTiles").GetComponent<Tilemap>();
        addTilemaps(addBG, bg);
        addTilemaps(addFG, fg);
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
