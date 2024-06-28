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

        foreach (Room r in rooms) r.refreshInitialization();
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

    private int childCount = 0;

    private float minDepth = 2, maxDepth = 5;
    private List<Node> genQueue = new List<Node>();
    private List<BoundsInt> blockedArea = new List<BoundsInt>();
    void GenerateMap()
    {
        // Find all doors
        List<Node> rootNodes = new List<Node>();

        fg.CompressBounds();
        BoundsInt bounds = fg.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y);
                TileBase tile = fg.GetTile(pos);
                if (tile != null && doorTiles.Contains(tile))
                    rootNodes.Add(new Node(this, null, pos, tile.name, 0));
            }
        blockedArea.Add(bounds);

        // Recursively add other rooms at doors
        foreach (Node node in rootNodes) genQueue.Add(node);
        while (genQueue.Count > 0)
        {
            Node n = genQueue[0];
            genQueue.Remove(n);
            n.ChooseRoom();
        }

        foreach (Node n in rootNodes)
            n.ApplyToMap();

        Debug.Log(childCount);
        Debug.Log(blockedArea.Count);
        foreach (Node n in rootNodes)
            Debug.Log(n.Count());

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

    void UpdateMapBounds()
    {
        Tilemap tilemap = transform.Find("Tiles").GetComponent<Tilemap>();

        UnityEngine.Vector3 worldmin = tilemap.transform.TransformPoint(tilemap.localBounds.min);
        UnityEngine.Vector3 worldmax = tilemap.transform.TransformPoint(tilemap.localBounds.max) + new UnityEngine.Vector3(0, 16, 0);

        Globals.mapBounds = new Bounds();
        Globals.mapBounds.SetMinMax(worldmin, worldmax);

    }
    class Node
    {

        GrassyFieldsGen root;
        Room room;
        BoundsInt bounds;
        Vector3Int roomLoc;
        string from;
        Vector3Int pos;
        float depth;
        Node parent;
        List<Node> children = new List<Node>();

        List<Room> validRooms = new List<Room>(), backupValidRooms = new List<Room>();
        public Node(GrassyFieldsGen root, Node parent, Vector3Int pos, string from, float depth)
        {
            this.root = root;
            this.parent = parent;
            this.pos = pos;
            this.from = from;
            this.depth = depth;

            // Adjust to be next to arrow
            switch (from)
            {
                case Room.G_LEFT:
                    this.pos += new Vector3Int(-1, 0);
                    break;
                case Room.G_RIGHT:
                    this.pos += new Vector3Int(1, 0);
                    break;
                case Room.G_UP:
                    this.pos += new Vector3Int(0, 1);
                    break;
                case Room.G_DOWN:
                    this.pos += new Vector3Int(0, -1);
                    break;
            }

            // Find rooms with matching exit
            foreach (Room room in root.allRooms)
            {
                switch (from)
                {
                    case Room.G_LEFT:
                        if (!room.hasRight) continue;
                        break;
                    case Room.G_RIGHT:
                        if (!room.hasLeft) continue;
                        break;
                    case Room.G_UP:
                        if (!room.hasDown) continue;
                        break;
                    case Room.G_DOWN:
                        if (!room.hasUp) continue;
                        break;
                }
                if (depth < root.minDepth && room.exits <= 1)
                    backupValidRooms.Add(room);
                else if (depth >= root.maxDepth && room.exits > 1)
                    backupValidRooms.Add(room);
                else
                    validRooms.Add(room);
            }
        }

        public void Remove()
        {
            if (room == null) return;
            room.UndoUsed();
            root.blockedArea.Remove(bounds);

            foreach (Node c in children) c.Remove();
            foreach (Node c in children) root.genQueue.Remove(c);
            children.Clear();
            room = null;
        }

        public void ChooseRoom()
        {
            while (room == null && (validRooms.Count > 0 || backupValidRooms.Count > 0))
            {
                if (validRooms.Count == 0 && backupValidRooms.Count > 0)
                {
                    validRooms.AddRange(backupValidRooms);
                    backupValidRooms.Clear();
                }
                Room place = Room.weightedRandom(validRooms);
                validRooms.Remove(place);

                // Find location to place
                switch (from)
                {
                    case Room.G_LEFT:
                        roomLoc = pos - place.rightExits[Random.Range(0, place.rightExits.Count)];
                        break;
                    case Room.G_RIGHT:
                        roomLoc = pos - place.leftExits[Random.Range(0, place.leftExits.Count)];
                        break;
                    case Room.G_UP:
                        roomLoc = pos - place.downExits[Random.Range(0, place.downExits.Count)];
                        break;
                    case Room.G_DOWN:
                        roomLoc = pos - place.upExits[Random.Range(0, place.upExits.Count)];
                        break;
                    default: roomLoc = new Vector3Int(0, 0, 0); break;
                }

                // Check if bounds intersects other bounds
                BoundsInt roomBounds = new BoundsInt();
                place.fg.CompressBounds();
                roomBounds.SetMinMax(place.fg.cellBounds.min + roomLoc, place.fg.cellBounds.max + roomLoc);
                foreach (BoundsInt blocking in root.blockedArea)
                    if (blocking.Intersects(roomBounds))
                    {
                        place = null;
                        break;
                    }

                if (place != null)
                {
                    room = place;
                    bounds = roomBounds;
                }
            }

            // No valid room options changes parent room
            if (room == null)
            {
                if (parent != null)
                {
                    parent.Remove();
                    parent.ChooseRoom();
                }
                return;
            }

            room.Used();
            Debug.Log(root.blockedArea.Contains(bounds));
            root.blockedArea.Add(bounds);

            // Repeat on exits
            Vector3Int entrance = pos - roomLoc;
            foreach (Vector3Int exit in room.leftExits)
                if (exit != entrance)
                    children.Add(new Node(root, this, roomLoc + exit, Room.G_LEFT, depth + room.depth));
            foreach (Vector3Int exit in room.rightExits)
                if (exit != entrance)
                    children.Add(new Node(root, this, roomLoc + exit, Room.G_RIGHT, depth + room.depth));
            foreach (Vector3Int exit in room.upExits)
                if (exit != entrance)
                    children.Add(new Node(root, this, roomLoc + exit, Room.G_UP, depth + room.depth));
            foreach (Vector3Int exit in room.downExits)
                if (exit != entrance)
                    children.Add(new Node(root, this, roomLoc + exit, Room.G_DOWN, depth + room.depth));

            foreach (Node child in children)
                root.genQueue.Add(child);
        }
        public void ApplyToMap()
        {
            if (room != null)
            {
                foreach (Transform t in room.objs)
                    Instantiate(t, new UnityEngine.Vector3(roomLoc.x, roomLoc.y, 0) + t.position, UnityEngine.Quaternion.identity);

                BoundsInt bounds = room.fg.cellBounds;
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        Vector3Int pos = new Vector3Int(x, y) + roomLoc;

                        TileBase pbg = room.bg.GetTile(new Vector3Int(x, y));
                        if (pbg != null) root.bg.SetTile(pos, pbg);
                        TileBase pfg = room.fg.GetTile(new Vector3Int(x, y));
                        if (pfg != null) root.fg.SetTile(pos, pfg);
                        TileBase pplat = room.plat.GetTile(new Vector3Int(x, y));
                        if (pplat != null) root.plat.SetTile(pos, pplat);
                    }
            }

            foreach (Node n in children)
                n.ApplyToMap();
        }
        public int Count()
        {
            int c = 1;
            foreach (Node n in children) c += n.Count();
            return c;
        }
    }
}