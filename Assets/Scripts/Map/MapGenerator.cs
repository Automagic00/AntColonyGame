using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;
using UnityEditor;
using UnityEngine.Assertions;

public class MapGenerator : MonoBehaviour
{

    public Room[] rooms;
    private List<Room> allRooms = new List<Room>();

    private List<Room> traderRooms = new List<Room>();
    private List<Room> itemRooms = new List<Room>();

    private int[] roomCount;


    public TileBase[] doorTiles;

    private Tilemap fg, bg, plat;

    private float seed;


    private Vector3 worldmin;
    private Vector3 worldmax;

    public Item[] allItems;
    public Item[] allValuables;

    void Start()
    {
        bg = transform.Find("BGTiles").GetComponent<Tilemap>();
        fg = transform.Find("Tiles").GetComponent<Tilemap>();
        plat = transform.Find("Platforms").GetComponent<Tilemap>();

        seed = Random.Range(0, 100f);

        LoadRoomData();
        PullMapGenSettings();

        GenerateMap();
        Debug.Assert(allRequiredRoomsDone(), "Missing some required rooms: " + requiredRoomsToString());

        RemoveArrows();
        UpdateMapBounds();

        StartCoroutine(DelayMakeItemTradeRoute());
        StartCoroutine(DelayNavMesh());
    }

    private void LoadRoomData()
    {
        roomCount = new int[rooms.Count()];

        foreach (Room r in rooms) r.refreshInitialization();
        allRooms.AddRange(rooms);
        foreach (Room room in rooms)
            if (room.allowMirror)
                allRooms.Add(room.mirror());

        foreach (Room room in allRooms)
            room.Prepare();

        foreach (Room room in rooms)
        {
            Transform objs = room.layout.transform.Find("objects");
            if (objs.Find("Trader") != null)
                traderRooms.Add(room);
            if (objs.Find("Item") != null && objs.Find("Item").GetComponent<SpawnChance>() == null)
                itemRooms.Add(room);
        }

    }
    public bool pullSettingsFromGameProgression = true;

    private void PullMapGenSettings()
    {
        if (!pullSettingsFromGameProgression) return;

        if (QueenAnt.queenWants != null)
            targetItem = QueenAnt.queenWants[System.Math.Clamp(Globals.gameProgression - 1, 0, QueenAnt.queenWants.Count - 1)];
        else targetItem = allItems[2];

        int minItems;

        if (Globals.gameProgression <= 3)
        {
            minDepth = 0.5f;
            maxDepth = 2f;
            requiredRoomDepth = 0.1f;
            requiredRoomIncrement = 0.25f;
            npcTradeLength = 1;
            minItems = 1;
        }
        else
            switch (Globals.gameProgression)
            {
                case 4:
                    minDepth = 1.0f;
                    maxDepth = 3f;
                    requiredRoomDepth = 0.5f;
                    requiredRoomIncrement = 0.5f;
                    npcTradeLength = 1;
                    minItems = 2;
                    break;
                case 5:
                    minDepth = 2f;
                    maxDepth = 5f;
                    requiredRoomDepth = 1.0f;
                    requiredRoomIncrement = 0.75f;
                    npcTradeLength = 2;
                    minItems = 2;
                    break;
                case 6:
                    minDepth = 3f;
                    maxDepth = 7f;
                    requiredRoomDepth = 1.5f;
                    requiredRoomIncrement = 1.0f;
                    npcTradeLength = 2;
                    minItems = 3;
                    break;
                default:
                    minDepth = 5f;
                    maxDepth = 9f;
                    requiredRoomDepth = 2.5f;
                    requiredRoomIncrement = 1.25f;
                    npcTradeLength = 3;
                    minItems = 3;
                    break;
            }

        foreach (Room r in traderRooms)
            r.minAmount = 0;
        for (int i = 0; i <= npcTradeLength; i++)
            traderRooms[Random.Range(0, traderRooms.Count)].minAmount++;
        foreach (Room r in traderRooms)
            r.maxAmount = r.minAmount + 1;

        foreach (Room r in itemRooms)
            r.minAmount = 0;
        for (int i = 0; i < minItems; i++)
            itemRooms[Random.Range(0, itemRooms.Count)].minAmount++;
        foreach (Room r in itemRooms)
            r.maxAmount = r.minAmount + 1;

    }

    public IEnumerator DelayMakeItemTradeRoute()
    {
        yield return new WaitForSeconds(.2f);
        MakeItemTradeRoute();
    }
    private void MakeItemTradeRoute()
    {
        /// Find all items and traders in the map (shuffle for random order)
        List<ItemBehavior> items = FindObjectsOfType<ItemBehavior>()
        .Where((item) => item.GetComponent<SpawnChance>() == null).ToList();
        items.Shuffle();
        List<Trader> traders = FindObjectsOfType<Trader>().ToList();
        traders.Shuffle();

        // Make list of valid trade items
        List<Item> possibleTrades = new List<Item>(allItems);
        possibleTrades.Shuffle();
        possibleTrades.Remove(targetItem);
        List<Item> valuables = new List<Item>(allValuables);
        valuables.Shuffle();

        /// Make expected trade route
        // Always start trades with leaf
        List<Item> tradeRoute = new List<Item> { allItems[0] };
        possibleTrades.Remove(allItems[0]);
        for (int i = 0; i < npcTradeLength; i++)
        {
            Item tradeItem = possibleTrades[0];
            tradeRoute.Add(tradeItem);
            possibleTrades.Remove(tradeItem);
        }
        tradeRoute.Add(targetItem);
        Debug.Log(tradeRoute.toString());

        // BACKUP: If missing traders in route, spawn some at start
        for (int i = 0; i + traders.Count < tradeRoute.Count - 1; i++)
        {
            traders.Add(Instantiate(traders[0], new Vector3(i, 4.8f, 0), Quaternion.identity));
        }

        /// Set random item to starting trade, and setup traders
        items[0].item = tradeRoute[0];
        items.Remove(items[0]);
        for (int i = 0; i < tradeRoute.Count - 1; i++)
        {
            traders[0].want = tradeRoute[i];
            traders[0].give = tradeRoute[i + 1];
            traders.Remove(traders[0]);
        }


        /// Use up remaining items & traders for weapons
        Item goodTradeItem = null, giveWeapon = null;
        if (items.Count > 0)
            for (int i = 0; i < items.Count || i < traders.Count; i++)
            {

                // Only choose a new trade item if it can set one
                if (i < items.Count && possibleTrades.Count > 0)
                {
                    goodTradeItem = possibleTrades[0];
                    possibleTrades.Remove(goodTradeItem);
                }
                else
                {
                    goodTradeItem = allItems[0];
                }
                // Only choose a new weapon if it can give one
                if (i < traders.Count && valuables.Count > 0)
                {
                    giveWeapon = valuables[0];
                    valuables.Remove(giveWeapon);
                }

                if (i < items.Count)
                    items[i].item = goodTradeItem;
                if (i < traders.Count)
                {
                    traders[i].want = goodTradeItem;
                    traders[i].give = giveWeapon;
                }
            }
    }


    private Item targetItem;
    private int npcTradeLength;

    public float minDepth = 4, maxDepth = 6;
    public float requiredRoomDepth = 2f;
    public float requiredRoomIncrement = 0.5f;
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

        blockedArea.Add(new BoundsInt(bounds.xMin, bounds.yMin, bounds.zMin,
        bounds.xMax, bounds.yMax + 8, bounds.zMax));

        // Recursively add other rooms at doors
        foreach (Node node in rootNodes) genQueue.Add(node);
        while (genQueue.Count > 0)
        {
            genQueue.Sort((Node a, Node b) => (int)(a.depth - b.depth));
            Node n = genQueue[0];
            genQueue.Remove(n);
            n.ChooseRoom();
        }

        foreach (Node n in rootNodes)
            n.ApplyToMap();
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
        tilemap.CompressBounds();

        worldmin = tilemap.transform.TransformPoint(tilemap.localBounds.min);
        worldmax = tilemap.transform.TransformPoint(tilemap.localBounds.max) + new UnityEngine.Vector3(0, 16, 0);

        Globals.mapBounds = new Bounds();
        Globals.mapBounds.SetMinMax(worldmin, worldmax);
    }

    bool allRequiredRoomsDone()
    {
        for (int i = 0; i < rooms.Count(); i++)
            if (roomCount[i] < rooms[i].minAmount)
                return false;

        return true;
    }
    string requiredRoomsToString()
    {
        string ret = "";
        for (int i = 0; i < rooms.Count(); i++)
            if (roomCount[i] < rooms[i].minAmount)
                ret += rooms[i].name + ": " + rooms[i].minAmount + " (" + roomCount[i] + ")\n";

        return ret;
    }
    bool willComleteRequiredRooms(Room r)
    {
        for (int i = 0; i < rooms.Count(); i++)
            if (roomCount[i] + (rooms[i] == r.unmirrored ? 1 : 0) < rooms[i].minAmount)
                return false;

        return true;

    }

    public IEnumerator DelayNavMesh()
    {
        yield return new WaitForSeconds(.2f);
        UpdateNavMesh();
    }
    public void UpdateNavMesh()
    {
        GridGraph grid = AstarPath.active.data.gridGraph;

        Vector3 size = worldmax - worldmin;
        Vector3 center = (worldmax + worldmin) / 2;

        //Mathf.RoundToInt()
        grid.center = center;
        grid.SetDimensions(Mathf.RoundToInt(size.x), Mathf.RoundToInt(size.y), 1);
        Bounds bound = new Bounds();
        bound.SetMinMax(worldmin, worldmax);

        Tilemap tilemap = transform.Find("Tiles").GetComponent<Tilemap>();

        /*CompositeCollider2D col = transform.Find("Tiles").GetComponent<CompositeCollider2D>();
        col.GenerateGeometry();
        Physics2D.SyncTransforms();*/
        AstarPath.active.Scan();
    }
    class Node
    {

        MapGenerator root;
        Room room;
        BoundsInt bounds;
        Vector3Int roomLoc;
        string from;
        Vector3Int pos;
        public float depth;
        Node parent;
        List<Node> children = new List<Node>();

        List<Room> validRooms = new List<Room>(), lowPriorityValidRooms = new List<Room>();
        public Node(MapGenerator root, Node parent, Vector3Int pos, string from, float depth)
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
                // Don't exit early
                if (depth < root.minDepth && room.exits <= 1)
                {
                    lowPriorityValidRooms.Add(room);
                    continue;
                }
                // Don't continue late
                if (depth >= root.maxDepth && room.exits > 1)
                {
                    // lowPriorityValidRooms.Add(room);
                    continue;
                }
                // Don't repeat rooms
                if (parent != null && room.unmirrored == parent.room.unmirrored)
                {
                    lowPriorityValidRooms.Add(room);
                    continue;
                }

                validRooms.Add(room);
            }

        }
        private void PrioritizeRequiredRooms()
        {
            bool stillNeedsRooms = !root.allRequiredRoomsDone();
            for (int i = 0; i < validRooms.Count; i++)
            {
                Room r = validRooms[i];
                int roomIndex = System.Array.FindIndex(root.rooms, match => match == r.unmirrored);
                if (root.roomCount[roomIndex] >= r.unmirrored.maxAmount && r.unmirrored.maxAmount >= 0)
                {
                    validRooms.Remove(r);
                    i--;
                }
                else if (depth >= root.requiredRoomDepth && root.roomCount[roomIndex] >= r.unmirrored.minAmount && r.exits <= 1 && stillNeedsRooms)
                {
                    validRooms.Remove(r);
                    i--;
                }
                // Force complete
                else if (root.genQueue.Count == 1 && !root.willComleteRequiredRooms(r) && r.exits <= 1)
                {
                    validRooms.Remove(r);
                    i--;
                }
            }
            for (int i = 0; i < lowPriorityValidRooms.Count; i++)
            {
                Room r = lowPriorityValidRooms[i];
                int roomIndex = System.Array.FindIndex(root.rooms, match => match == r.unmirrored);
                if (root.roomCount[roomIndex] >= r.unmirrored.maxAmount && r.unmirrored.maxAmount >= 0)
                {
                    lowPriorityValidRooms.Remove(r);
                    i--;
                }
                else if (depth >= root.requiredRoomDepth && root.roomCount[roomIndex] >= r.unmirrored.minAmount && r.exits <= 1 && stillNeedsRooms)
                {
                    lowPriorityValidRooms.Remove(r);
                    i--;
                }
                // Force complete
                else if (root.genQueue.Count == 1 && !root.willComleteRequiredRooms(r) && r.exits <= 1)
                {
                    lowPriorityValidRooms.Remove(r);
                    i--;
                }
            }

            if (depth < root.requiredRoomDepth) return;

            List<Room> stillRequired = new List<Room>();
            foreach (Room r in validRooms)
            {
                int roomIndex = System.Array.FindIndex(root.rooms, match => match == r.unmirrored);
                if (root.roomCount[roomIndex] < r.unmirrored.minAmount &&
                depth >= root.requiredRoomDepth + root.roomCount[roomIndex] * root.requiredRoomIncrement)
                    stillRequired.Add(r);
            }
            foreach (Room r in lowPriorityValidRooms)
            {
                int roomIndex = System.Array.FindIndex(root.rooms, match => match == r.unmirrored);
                if (root.roomCount[roomIndex] < r.unmirrored.minAmount &&
                depth >= root.requiredRoomDepth + root.roomCount[roomIndex] * root.requiredRoomIncrement)
                    stillRequired.Add(r);
            }

            if (stillRequired.Count > 0)
            {
                lowPriorityValidRooms = lowPriorityValidRooms.Except(stillRequired).ToList();
                lowPriorityValidRooms.AddRange(validRooms.Except(stillRequired));
                validRooms = stillRequired;
            }
        }

        public void ChooseRoom()
        {
            PrioritizeRequiredRooms();

            while (room == null && validRooms.Count > 0)
            {

                Room place = Room.weightedRandom(validRooms);
                validRooms.Remove(place);

                if (validRooms.Count == 0)
                {
                    validRooms.AddRange(lowPriorityValidRooms);
                    lowPriorityValidRooms.Clear();
                }

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
                roomBounds.SetMinMax(place.fg.cellBounds.min + roomLoc, place.fg.cellBounds.max + roomLoc + new Vector3Int(0, place.overheadBounds));
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

            // No valid room options. Remove parent room's selection from pool
            if (room == null)
            {
                if (parent != null)
                {
                    parent.RemoveRoomOption();
                    parent.ChooseRoom();
                }
                return;
            }

            // ROOM CHOSEN. Set data
            room.Used(root.maxDepth);
            root.blockedArea.Add(bounds);
            int roomIndex = System.Array.FindIndex(root.rooms, r => r == room.unmirrored);
            root.roomCount[roomIndex]++;


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

        public void RemoveRoomOption()
        {
            if (room == null) return;
            room.UndoUsed(root.maxDepth);
            root.blockedArea.Remove(bounds);

            int roomIndex = System.Array.FindIndex(root.rooms, r => r == room.unmirrored);
            root.roomCount[roomIndex]--;

            foreach (Node c in children) c.RemoveRoomOption();
            foreach (Node c in children) root.genQueue.Remove(c);
            children.Clear();

            room = null;
        }

        public void ApplyToMap()
        {
            if (room != null)
            {
                float spawnSeed = Random.Range(0, 100f);
                foreach (Transform t in room.objs)
                {
                    SpawnChance chance = t.GetComponent<SpawnChance>();
                    if (chance != null && !chance.SpawnCheck(spawnSeed, root.seed)) continue;

                    Transform spawned = Instantiate(t, new UnityEngine.Vector3(roomLoc.x, roomLoc.y, 0) + t.position, UnityEngine.Quaternion.identity);
                    // if (room.isMirrored)
                    //     spawned.localScale = new UnityEngine.Vector3(spawned.localScale.x * -1, spawned.localScale.y, spawned.localScale.z);
                }

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


    }
}