using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NewBehaviourScript : MonoBehaviour
{


    public GameObject[] gameStatePrefabs;

    public MinorPrefabs[] majorPrefabList;
    [Serializable] public struct MinorPrefabs { public GameObject[] minorPrefabs; }

    private Tilemap fg, bg, plat;

    void Awake()
    {
        Globals.BoundToTilemap(transform.Find("Tiles").GetComponent<Tilemap>());
    }

    void Start()
    {
        bg = transform.Find("BGTiles").GetComponent<Tilemap>();
        fg = transform.Find("Tiles").GetComponent<Tilemap>();
        plat = transform.Find("Platforms").GetComponent<Tilemap>();

        // Update map when game progresses
        Globals.addProgressionListener(updateMap);

        // Load existing
        addPreviousProgression();

        Globals.BoundToTilemap(transform.Find("Tiles").GetComponent<Tilemap>());
    }
    void OnDestroy()
    {
        Globals.removeProgressionListener(updateMap);
    }

    void addPreviousProgression()
    {
        // Add all previous prefabs
        for (int i = 0; i < Globals.majorProgression - 1 && i < majorPrefabList.Length; i++)
            foreach (GameObject pf in majorPrefabList[i].minorPrefabs)
                add(pf);

        // Add current minor progression
        int ii = Globals.majorProgression;
        if (ii < majorPrefabList.Length)
            for (int j = 0; j < Globals.minorProgression + 1 && j < majorPrefabList[ii].minorPrefabs.Length; j++)
                add(majorPrefabList[ii].minorPrefabs[j]);

    }

    void updateMap()
    {
        int i = Globals.majorProgression, j = Globals.minorProgression;

        if (i < majorPrefabList.Length && j < majorPrefabList[i].minorPrefabs.Length)
            add(majorPrefabList[i].minorPrefabs[j]);
    }

    void add(GameObject prefab)
    {
        if (prefab == null) return;

        Tilemap addBG = prefab.transform.Find("pfBGTiles").GetComponent<Tilemap>();
        addTilemaps(addBG, bg);
        Tilemap addFG = prefab.transform.Find("pfTiles").GetComponent<Tilemap>();
        addTilemaps(addFG, fg);
        if (prefab.transform.Find("pfPlatforms") != null)
        {
            Tilemap addPlat = prefab.transform.Find("pfPlatforms").GetComponent<Tilemap>();
            if (addPlat != null)
                addTilemaps(addPlat, plat);
        }


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
