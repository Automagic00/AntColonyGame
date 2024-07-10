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
    public string[] startingRoomTypes;

    private List<Room> allRooms = new List<Room>(),
        traderRooms = new List<Room>(),
        itemRooms = new List<Room>(),
        nurseRooms = new List<Room>(),
        courierRooms = new List<Room>();

    private int[] roomCount;



    private Tilemap fg, bg, plat;

    private float seed;


    private Vector3 worldmin;
    private Vector3 worldmax;
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
            if (objs.Find("Nurse") != null)
                nurseRooms.Add(room);
            if (objs.Find("Courier") != null)
                courierRooms.Add(room);
            if (objs.Find("Item") != null && objs.Find("Item").GetComponent<SpawnChance>() == null)
                itemRooms.Add(room);
        }

    }
    public bool pullSettingsFromGameProgression = true;

    private void PullMapGenSettings()
    {
        if (!pullSettingsFromGameProgression) return;

        // NPCs
        if (!Globals.nurseEnabled)
        {
            if (GameObject.Find("Nurse") != null)
                Destroy(GameObject.Find("Nurse"));
            foreach (Room room in nurseRooms)
                room.maxAmount = 0;
        }
        if (!Globals.courierEnabled)
        {
            if (GameObject.Find("Courier") != null)
                Destroy(GameObject.Find("Courier"));
            foreach (Room room in courierRooms)
                room.maxAmount = 0;
        }

        // Trading
        if (QueenAnt.queenWants != null)
            targetItem = QueenAnt.queenWants[System.Math.Clamp(Globals.gameProgression - 1, 0, QueenAnt.queenWants.Count - 1)];
        else targetItem = allItems[2];

        // Map gen
        int minItems;

        if (Globals.gameProgression <= 3)
        {
            minDepth = 0.5f;
            maxDepth = 1.5f;
            requiredRoomDepth = 0;
            requiredRoomIncrement = 0.25f;
            npcTradeLength = 2;
            minItems = 1;
        }
        else
            switch (Globals.gameProgression)
            {
                case 4:
                    minDepth = 1.0f;
                    maxDepth = 2.5f;
                    requiredRoomDepth = 0.5f;
                    requiredRoomIncrement = 0.5f;
                    npcTradeLength = 2;
                    minItems = 2;
                    break;
                case 5:
                    minDepth = 2f;
                    maxDepth = 3.5f;
                    requiredRoomDepth = 1.0f;
                    requiredRoomIncrement = 0.75f;
                    npcTradeLength = 3;
                    minItems = 2;
                    break;
                case 6:
                    minDepth = 3f;
                    maxDepth = 4.5f;
                    requiredRoomDepth = 1.5f;
                    requiredRoomIncrement = 1.0f;
                    npcTradeLength = 3;
                    minItems = 3;
                    break;
                default:
                    minDepth = 5f;
                    maxDepth = 6f;
                    requiredRoomDepth = 2.5f;
                    requiredRoomIncrement = 1.25f;
                    npcTradeLength = 4;
                    minItems = 4;
                    break;
            }

        if (traderRooms.Count > 0)
        {
            foreach (Room r in traderRooms)
                r.minAmount = 0;
            for (int i = 0; i <= npcTradeLength; i++)
                traderRooms[Random.Range(0, traderRooms.Count)].minAmount++;
            foreach (Room r in traderRooms)
                r.maxAmount = r.minAmount + 1;
        }

        if (itemRooms.Count > 0)
        {
            foreach (Room r in itemRooms)
                r.minAmount = 0;
            for (int i = 0; i < minItems; i++)
                itemRooms[Random.Range(0, itemRooms.Count)].minAmount++;
        }
    }

    public IEnumerator DelayMakeItemTradeRoute()
    {
        yield return new WaitForSeconds(.2f);
        MakeItemTradeRoute();
    }
    private void MakeItemTradeRoute()
    {
        if (traderRooms.Count == 0 || itemRooms.Count == 0) return;

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
        for (int i = 1; i < npcTradeLength; i++)
        {
            Item tradeItem = possibleTrades[0];
            tradeRoute.Add(tradeItem);
            possibleTrades.Remove(tradeItem);
        }
        tradeRoute.Add(targetItem);

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
        Item giveWeapon = null;
        if (items.Count > 0)
            for (int i = 0; i < items.Count || i < traders.Count; i++)
            {

                if (i < traders.Count && valuables.Count > 0)
                {
                    giveWeapon = valuables[0];
                    valuables.Remove(giveWeapon);
                }

                if (i < items.Count)
                    items[i].item = allItems[0];
                if (i < traders.Count)
                {
                    traders[i].want = tradeRoute[Random.Range(0, tradeRoute.Count / 2)];
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

    private List<Node> rootNodes = new List<Node>();
    private List<Node> acceptedNodes = new List<Node>();
    private List<BoundsInt> blockedArea = new List<BoundsInt>();

    void GenerateMap()
    {
        // Find all doors
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
        tilemap.RefreshAllTiles();

        worldmin = tilemap.transform.TransformPoint(tilemap.localBounds.min);
        worldmax = tilemap.transform.TransformPoint(tilemap.localBounds.max) + new UnityEngine.Vector3(0, 16, 0);

        Globals.mapBounds = new Bounds();
        Globals.mapBounds.SetMinMax(worldmin, worldmax);
    }

    public bool allRequiredRoomsDone()
    {
        for (int i = 0; i < rooms.Count(); i++)
            if (roomCount[i] < rooms[i].minAmount)
                return false;

        return true;
    }
    public string requiredRoomsToString()
    {
        string ret = "";
        for (int i = 0; i < rooms.Count(); i++)
            if (roomCount[i] < rooms[i].minAmount)
                ret += rooms[i].name + ": " + rooms[i].minAmount + " (" + roomCount[i] + ")\n";

        return ret;
    }
    public bool willComleteRequiredRooms(Room r)
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

            GetValidRooms();
        }
        void GetValidRooms()
        {
            foreach (Room room in root.allRooms)
            {
                if (parent == null && root.startingRoomTypes.Length > 0
                && !root.startingRoomTypes.Contains(room.roomType)) continue;

                switch (from)
                {
                    case Room.G_LEFT:
                        if (!room.hasRight) continue;
                        // Match room types
                        if (parent != null && room.allowedRoomsR.Length > 0
                        && !room.allowedRoomsR.Contains(parent.room.roomType)) continue;
                        if (parent != null && parent.room.allowedRoomsL.Length > 0
                        && !parent.room.allowedRoomsL.Contains(room.roomType)) continue;
                        break;
                    case Room.G_RIGHT:
                        if (!room.hasLeft) continue;
                        // Match room types
                        if (parent != null && room.allowedRoomsL.Length > 0
                        && !room.allowedRoomsL.Contains(parent.room.roomType)) continue;
                        if (parent != null && parent.room.allowedRoomsR.Length > 0
                        && !parent.room.allowedRoomsR.Contains(room.roomType)) continue;
                        break;
                    case Room.G_UP:
                        if (!room.hasDown) continue;
                        // Match room types
                        if (parent != null && room.allowedRoomsD.Length > 0
                        && !room.allowedRoomsD.Contains(parent.room.roomType)) continue;
                        if (parent != null && parent.room.allowedRoomsU.Length > 0
                        && !parent.room.allowedRoomsU.Contains(room.roomType)) continue;
                        break;
                    case Room.G_DOWN:
                        if (!room.hasUp) continue;
                        // Match room types
                        if (parent != null && room.allowedRoomsU.Length > 0
                        && !room.allowedRoomsU.Contains(parent.room.roomType)) continue;
                        if (parent != null && parent.room.allowedRoomsD.Length > 0
                        && !parent.room.allowedRoomsD.Contains(room.roomType)) continue;
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
            // foreach (Node node in root.rootNodes) Debug.Log(node.toString());

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

                        TileBase pfg = room.fg.GetTile(new Vector3Int(x, y));
                        if (pfg != null) root.fg.SetTile(pos, pfg);
                    }
                bounds = room.bg.cellBounds;
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        Vector3Int pos = new Vector3Int(x, y) + roomLoc;

                        TileBase pbg = room.bg.GetTile(new Vector3Int(x, y));
                        if (pbg != null) root.bg.SetTile(pos, pbg);
                    }
                bounds = room.plat.cellBounds;
                for (int x = bounds.xMin; x < bounds.xMax; x++)
                    for (int y = bounds.yMin; y < bounds.yMax; y++)
                    {
                        Vector3Int pos = new Vector3Int(x, y) + roomLoc;

                        TileBase pplat = room.plat.GetTile(new Vector3Int(x, y));
                        if (pplat != null) root.plat.SetTile(pos, pplat);
                    }
            }

            foreach (Node n in children)
                n.ApplyToMap();
        }

        public string toString()
        {
            if (room == null) return "_";

            string name = room.name + ", [";
            foreach (Node n in children)
                name += n.toString();
            name = name.Substring(0, name.Length - 1);
            return name + "]";
        }
    }

    public TileBase[] doorTiles;

    public Item[] allItems;
    public Item[] allValuables;

}