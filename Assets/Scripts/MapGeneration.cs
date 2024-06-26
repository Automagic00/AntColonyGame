using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGeneration : MonoBehaviour
{

    public GameObject[] dungeonRooms;
    private Tilemap fg, bg;

    // Start is called before the first frame update
    void Start()
    {
        bg = transform.Find("BGTiles").GetComponent<Tilemap>();
        fg = transform.Find("Tiles").GetComponent<Tilemap>();
        
        updateMap();
        // Set camera bounds to tilemap
        Tilemap tilemap = transform.Find("Tiles").GetComponent<Tilemap>();

        Vector3 worldmin = tilemap.transform.TransformPoint(tilemap.localBounds.min);
        Vector3 worldmax = tilemap.transform.TransformPoint(tilemap.localBounds.max) + new Vector3(0, 16, 0);

        Globals.mapBounds = new Bounds();
        Globals.mapBounds.SetMinMax(worldmin, worldmax);
    }
void updateMap()
    {
        add(dungeonRooms[0], new Vector3Int (32, 0, 0));
        add(dungeonRooms[1], new Vector3Int (32*2, 0, 0));
    }

    void add(GameObject prefab, Vector3Int Offset)
    {
        Tilemap addBG = prefab.transform.Find("pfBGTiles").GetComponent<Tilemap>();
        Tilemap addFG = prefab.transform.Find("pfTiles").GetComponent<Tilemap>();
        addTilemaps(addBG, bg, Offset);
        addTilemaps(addFG, fg, Offset);

        Transform addObjects = prefab.transform.Find("objects");
        if (addObjects != null)
            for (int i = 0; i < addObjects.childCount; i++)
            {
                Instantiate(addObjects.GetChild(i));
            }
    }

     private void addTilemaps(Tilemap fromMap, Tilemap toMap, Vector3Int Offset)
    {
        BoundsInt bounds = fromMap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = fromMap.GetTile(pos);
                if (tile == null) continue;

                if (tile.name == "removetile")
                    toMap.SetTile(pos + Offset, null);
                else
                    toMap.SetTile(pos + Offset, tile);

            }
    }
}
