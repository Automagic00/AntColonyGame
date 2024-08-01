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

    private const bool LOG_TRADES = true;
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

        StartCoroutine(DelaySetItems());
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
            targetItem = QueenAnt.queenWants[System.Math.Clamp(Globals.majorProgression - 1, 0, QueenAnt.queenWants.Count - 1)];
        else if (targetItem == null) targetItem = allItems[2]; // bottlecap

        // Map gen
        int minItemRoom, minTradeRoom;

        if (Globals.majorProgression <= 3)
        {
            minDepth = 0.5f;
            maxDepth = 1.5f;
            requiredRoomDepth = 0;
            requiredRoomIncrement = 0.25f;
            minTradeRoom = 1;
            minItemRoom = 1;
        }
        else
            switch (Globals.majorProgression)
            {
                case 4:
                    minDepth = 1.0f;
                    maxDepth = 2.0f;
                    requiredRoomDepth = 0.25f;
                    requiredRoomIncrement = 0.25f;
                    minTradeRoom = 1;
                    minItemRoom = 2;
                    break;
                case 5:
                    minDepth = 1.5f;
                    maxDepth = 2.5f;
                    requiredRoomDepth = 0.5f;
                    requiredRoomIncrement = 0.5f;
                    minTradeRoom = 2;
                    minItemRoom = 2;
                    break;
                case 6:
                    minDepth = 2.0f;
                    maxDepth = 3.0f;
                    requiredRoomDepth = 0.75f;
                    requiredRoomIncrement = 0.5f;
                    minTradeRoom = 2;
                    minItemRoom = 3;
                    break;
                default:
                    minDepth = 3.0f;
                    maxDepth = 3.5f;
                    requiredRoomDepth = 1.0f;
                    requiredRoomIncrement = 0.75f;
                    minTradeRoom = 3;
                    minItemRoom = 3;
                    break;
            }

        if (traderRooms.Count > 0)
        {
            foreach (Room r in traderRooms)
                r.minAmount = 0;
            for (int i = 0; i <= minTradeRoom; i++)
                traderRooms[Random.Range(0, traderRooms.Count)].minAmount++;
        }

        if (itemRooms.Count > 0)
        {
            foreach (Room r in itemRooms)
                r.minAmount = 0;
            for (int i = 0; i < minItemRoom; i++)
                itemRooms[Random.Range(0, itemRooms.Count)].minAmount++;
        }
    }

    public IEnumerator DelaySetItems()
    {
        yield return new WaitForSeconds(.2f);
        SetItems();
    }

    public GameObject itemPrefab, traderPrefab;
    public Vector2 roomOrigin;
    private void SetItems()
    {
        // Leaf and pebble
        Item[] startingItems = new Item[] { allItems[0], allItems[1] };
        float[] startingItemsWeight = new float[] { 0.75f, 0.25f };
        // Bottlecap, nectar, twig
        Item[] intermediateItems = new Item[] { allItems[2], allItems[3], allValuables[0] };
        float[] intermediateItemsWeight = new float[] { 1.25f, 0.25f, 0.5f };
        // Sugar cube
        Item[] finalItems = new Item[] { allItems[4] };
        float[] finalItemsWeight = new float[] { 1f };

        // Pick required items & traders based off current mission
        List<Item> requiredItems = new List<Item>();
        List<Item[]> requiredTrades = new List<Item[]>();

        if (startingItems.Contains(targetItem))
        {
            requiredItems.Add(targetItem);
        }
        else if (intermediateItems.Contains(targetItem))
        {
            Item requiredStarter = startingItems[RandomUtil.weightedRandom(startingItemsWeight)];

            requiredItems.Add(requiredStarter);
            requiredTrades.Add(new Item[] { requiredStarter, targetItem });
        }
        else if (finalItems.Contains(targetItem))
        {
            Item requiredStarter = startingItems[RandomUtil.weightedRandom(startingItemsWeight)];
            Item requiredIntermediate = intermediateItems[RandomUtil.weightedRandom(intermediateItemsWeight)];

            requiredItems.Add(requiredStarter);
            requiredTrades.Add(new Item[] { requiredStarter, requiredIntermediate });
            requiredTrades.Add(new Item[] { requiredIntermediate, targetItem });
        }

        if (LOG_TRADES)
        {
            Debug.Log("Target: " + targetItem);
            Debug.Log("Items: " + string.Join(", ", requiredItems));
            Debug.Log("Trades: ");
            foreach (Item[] trade in requiredTrades)
                Debug.Log(trade[0] + " : " + trade[1]);
        }


        /// Find all items and traders in the map
        List<ItemBehavior> items = FindObjectsOfType<ItemBehavior>()
        .Where((item) => item.GetComponent<SpawnChance>() == null).ToList();
        List<Trader> traders = FindObjectsOfType<Trader>().ToList();
        HashSet<Item> usedItems = new HashSet<Item>();

        // Spawn missing items & traders at start
        float spawnOrigin = 0;
        while (items.Count < requiredItems.Count)
        {
            ItemBehavior spawnedItem = Instantiate(itemPrefab, new Vector3(roomOrigin.x + spawnOrigin, roomOrigin.y), Quaternion.identity)
                .GetComponent<ItemBehavior>();
            spawnedItem.item = allItems[0];
            items.Add(spawnedItem);

            spawnOrigin += 1;
        }
        while (traders.Count < requiredTrades.Count)
        {
            Trader spawnedTrader = Instantiate(traderPrefab, new Vector3(roomOrigin.x - spawnOrigin, roomOrigin.y), Quaternion.identity)
                .GetComponent<Trader>();
            spawnedTrader.want = allItems[0];
            spawnedTrader.give = allItems[1];
            traders.Add(spawnedTrader);

            spawnOrigin += 1;

        }
        // Shuffle for random quest order
        items.Shuffle();
        traders.Shuffle();

        // Remove any set items from list
        foreach (Item i in requiredItems)
        {
            items[0].item = i;
            usedItems.Add(i);
            items.RemoveAt(0);
        }
        foreach (Item[] trade in requiredTrades)
        {
            traders[0].want = trade[0];
            traders[0].give = trade[1];
            usedItems.Add(traders[0].give);
            traders.RemoveAt(0);
        }

        // Spawn remaining things
        const float valuableSpawnChance = 0.05f;
        while (items.Count > 0)
        {
            if (Random.Range(0, 1f) <= valuableSpawnChance)
                items[0].item = allValuables[RandomUtil.weightedRandom(allValuablesWeights)];
            else
                items[0].item = startingItems[RandomUtil.weightedRandom(startingItemsWeight)];
            usedItems.Add(items[0].item);

            items.RemoveAt(0);
        }

        const float advancedTradeChance = 0.4f;
        const float criticalBeginnerTrade = 0.1f;
        const float criticalAdvancedTrade = 0.25f;
        while (traders.Count > 0)
        {
            List<Item> fromChoices = new List<Item>();
            List<float> fromChoiceWeights = new List<float>();
            List<Item> toChoices = new List<Item>();
            List<float> toChoiceWeights = new List<float>();

            if (Random.Range(0, 1f) <= advancedTradeChance && usedItems.Union(intermediateItems).Count() > 0)
            {

                // Do an advanced trade (intermediate -> final)
                fromChoices.AddRange(intermediateItems);
                fromChoiceWeights.AddRange(intermediateItemsWeight);
                if (Random.Range(0, 1f) <= criticalAdvancedTrade)
                {
                    toChoices.AddRange(allValuables);
                    toChoiceWeights.AddRange(allValuablesWeights);
                }
                else
                {
                    toChoices.AddRange(finalItems);
                    toChoiceWeights.AddRange(finalItemsWeight);
                }
            }
            else
            {
                // Do a beginner trade (starting -> intermediate)
                fromChoices.AddRange(startingItems);
                fromChoiceWeights.AddRange(startingItemsWeight);
                if (Random.Range(0, 1f) <= criticalBeginnerTrade)
                {
                    toChoices.AddRange(allValuables);
                    toChoiceWeights.AddRange(allValuablesWeights);
                }
                else
                {
                    toChoices.AddRange(intermediateItems);
                    toChoiceWeights.AddRange(intermediateItemsWeight);
                }
            }

            // Filter by used items
            for (int i = 0; i < fromChoices.Count && fromChoices.Count > 1; i++)
            {
                if (!usedItems.Contains(fromChoices[i]))
                {
                    fromChoices.RemoveAt(i);
                    fromChoiceWeights.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < toChoices.Count && toChoices.Count > 1; i++)
            {
                if (usedItems.Contains(toChoices[i]))
                {
                    toChoices.RemoveAt(i);
                    toChoiceWeights.RemoveAt(i);
                    i--;
                }
            }

            traders[0].want = fromChoices[RandomUtil.weightedRandom(fromChoiceWeights)];
            traders[0].give = toChoices[RandomUtil.weightedRandom(toChoiceWeights)];
            usedItems.Add(traders[0].give);
            traders.RemoveAt(0);
        }

    }



    public float minDepth = 4, maxDepth = 6;
    public float requiredRoomDepth = 2f;
    public float requiredRoomIncrement = 0.5f;
    public Item targetItem;


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
                if (depth < root.minDepth && !room.letEndEarly && room.exits <= 1)
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
                if (parent != null && !room.allowRepeat && room.unmirrored == parent.room.unmirrored)
                {
                    lowPriorityValidRooms.Add(room);
                    continue;
                }

                validRooms.Add(room);
            }

            if (validRooms.Count == 0)
            {
                validRooms.AddRange(lowPriorityValidRooms);
                lowPriorityValidRooms.Clear();
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
            // foreach (Node node in root.rootNodes) Debug.Log(node.toString());
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
                    children.Add(new Node(root, this, roomLoc + exit, Room.G_LEFT, room.effectiveDepth(depth, root.maxDepth)));
            foreach (Vector3Int exit in room.rightExits)
                if (exit != entrance)
                    children.Add(new Node(root, this, roomLoc + exit, Room.G_RIGHT, room.effectiveDepth(depth, root.maxDepth)));
            foreach (Vector3Int exit in room.upExits)
                if (exit != entrance)
                    children.Add(new Node(root, this, roomLoc + exit, Room.G_UP, room.effectiveDepth(depth, root.maxDepth)));
            foreach (Vector3Int exit in room.downExits)
                if (exit != entrance)
                    children.Add(new Node(root, this, roomLoc + exit, Room.G_DOWN, room.effectiveDepth(depth, root.maxDepth)));

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

            string name = room.name;
            if (children.Count > 0)
            {
                name += " [";
                foreach (Node n in children)
                    name += n.toString() + ", ";
                name = name.Substring(0, name.Length - 2);
                name += "]";

            }
            return name;
        }
    }

    public TileBase[] doorTiles;

    public Item[] allItems;
    public Item[] allValuables;
    public float[] allValuablesWeights;

}