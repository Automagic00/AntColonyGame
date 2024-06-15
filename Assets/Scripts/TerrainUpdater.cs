using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NewBehaviourScript : MonoBehaviour
{

    public GameObject loungePrefab;
    private Tilemap fg, bg;
    // Start is called before the first frame update
    void Start()
    {
        bg = transform.Find("BGTiles").GetComponent<Tilemap>();
        fg = transform.Find("Tiles").GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("z"))
            addLounge();
    }

    void addLounge()
    {
        Tilemap addBG = loungePrefab.gameObject.transform.Find("BGTiles").GetComponent<Tilemap>();
        Tilemap addFG = loungePrefab.gameObject.transform.Find("Tiles").GetComponent<Tilemap>();
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
