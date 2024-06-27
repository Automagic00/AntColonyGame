using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GrassyFieldsGen : MonoBehaviour
{

    public Room[] rooms;
    private List<Room> allRooms = new List<Room>();

    public TileBase[] doorTiles;


    private Tilemap fg, bg, plat;


    void Start()
    {
        bg = transform.Find("BGTiles").GetComponent<Tilemap>();
        fg = transform.Find("Tiles").GetComponent<Tilemap>();
        plat = transform.Find("Platforms").GetComponent<Tilemap>();

        allRooms.AddRange(rooms);
        foreach (Room room in rooms)
            if (room.allowMirror)
                allRooms.Add(room.mirror());

        foreach (Room room in allRooms)
            room.Prepare();

        GenerateMap();

        RemoveArrows();
        UpdateMapBounds();
    }

    private int maxDepth = 3;

    void GenerateMap()
    {
        // Find all doors
        List<Vector3Int> roomLocs = new List<Vector3Int>();

        fg.CompressBounds();
        BoundsInt bounds = fg.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y);
                TileBase tile = fg.GetTile(pos);
                if (tile != null && doorTiles.Contains(tile))
                    roomLocs.Add(pos);
            }

        // Recursively add other rooms at doors
        List<BoundsInt> blockingArea = new List<BoundsInt> { bounds };
        foreach (Vector3Int pos in roomLocs)
            addRoom(pos, blockingArea, 0);

    }
    void RemoveArrows()
    {

        BoundsInt bounds = fg.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y);
                TileBase tile = fg.GetTile(new Vector3Int(x, y));
                if (tile != null && doorTiles.Contains(tile))
                    fg.SetTile(pos, null);
            }
    }

    // TODO recursively check that each room has a valid placement. Remove or try a different room if not.
    void addRoom(Vector3Int tilePos, List<BoundsInt> block, int currentDepth)
    {

        string roomType = fg.GetTile(tilePos).name;

        // Find all rooms with matching exit
        List<Room> validRooms = new List<Room>();
        foreach (Room room in allRooms)
        {
            if (currentDepth >= maxDepth && room.exits > 1) continue;

            switch (roomType)
            {
                case Room.G_LEFT:
                    if (room.hasRight) validRooms.Add(room);
                    break;
                case Room.G_RIGHT:
                    if (room.hasLeft) validRooms.Add(room);
                    break;
                case Room.G_UP:
                    if (room.hasDown) validRooms.Add(room);
                    break;
                case Room.G_DOWN:
                    if (room.hasUp) validRooms.Add(room);
                    break;
            }
        }

        // Try to fit room into spot
        Room place = null;
        Vector3Int placeLoc = new Vector3Int();
        bool foundRoom = false;
        while (validRooms.Count > 0 && !foundRoom)
        {
            place = Room.weightedRandom(validRooms); //validRooms[Random.Range(0, validRooms.Count)];
            validRooms.Remove(place);

            // Find location to place
            switch (roomType)
            {
                case Room.G_LEFT:
                    placeLoc = tilePos + new Vector3Int(-1, 0) - place.rightExits[Random.Range(0, place.rightExits.Count)];
                    break;
                case Room.G_RIGHT:
                    placeLoc = tilePos + new Vector3Int(1, 0) - place.leftExits[Random.Range(0, place.leftExits.Count)];
                    break;
                case Room.G_UP:
                    placeLoc = tilePos + new Vector3Int(0, 1) - place.downExits[Random.Range(0, place.downExits.Count)];
                    break;
                case Room.G_DOWN:
                    placeLoc = tilePos + new Vector3Int(0, -1) - place.upExits[Random.Range(0, place.upExits.Count)];
                    break;
            }

            // Check if bounds intersects other bounds
            BoundsInt roomBounds = new BoundsInt();
            place.fg.CompressBounds();
            roomBounds.SetMinMax(place.fg.cellBounds.min + placeLoc, place.fg.cellBounds.max + placeLoc);

            bool canPlace = true;
            foreach (BoundsInt blocking in block)
                if (blocking.Intersects(roomBounds))
                {
                    canPlace = false;
                    break;
                }

            if (canPlace)
            {
                foundRoom = true;
                block.Add(roomBounds);
            }
        }
        if (!foundRoom) return;
        place.Used();

        // Place room
        foreach (Transform t in place.objs)
            Instantiate(t, new UnityEngine.Vector3(placeLoc.x, placeLoc.y, 0) + t.position, UnityEngine.Quaternion.identity);

        List<Vector3Int> roomLocs = new List<Vector3Int>();
        BoundsInt bounds = place.fg.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y) + placeLoc;

                TileBase pbg = place.bg.GetTile(new Vector3Int(x, y));
                if (pbg != null) bg.SetTile(pos, pbg);
                TileBase pfg = place.fg.GetTile(new Vector3Int(x, y));
                if (pfg != null) fg.SetTile(pos, pfg);
                TileBase pplat = place.plat.GetTile(new Vector3Int(x, y));
                if (pplat != null) plat.SetTile(pos, pplat);

                // Iterate if unmatched tile
                if (pfg != null) switch (pfg.name)
                    {
                        case Room.G_LEFT:
                            if (fg.GetTile(pos + new Vector3Int(-1, 0)) == null)
                                roomLocs.Add(pos);
                            break;
                        case Room.G_RIGHT:
                            if (fg.GetTile(pos + new Vector3Int(1, 0)) == null)
                                roomLocs.Add(pos);
                            break;
                        case Room.G_UP:
                            if (fg.GetTile(pos + new Vector3Int(0, 1)) == null)
                                roomLocs.Add(pos);
                            break;
                        case Room.G_DOWN:
                            if (fg.GetTile(pos + new Vector3Int(0, -1)) == null)
                                roomLocs.Add(pos);
                            break;
                        default: break;
                    }
            }

        foreach (Vector3Int pos in roomLocs)
            addRoom(pos, block, currentDepth + 1);
    }

    void UpdateMapBounds()
    {
        Tilemap tilemap = transform.Find("Tiles").GetComponent<Tilemap>();

        UnityEngine.Vector3 worldmin = tilemap.transform.TransformPoint(tilemap.localBounds.min);
        UnityEngine.Vector3 worldmax = tilemap.transform.TransformPoint(tilemap.localBounds.max) + new UnityEngine.Vector3(0, 16, 0);

        Globals.mapBounds = new Bounds();
        Globals.mapBounds.SetMinMax(worldmin, worldmax);

    }
}
