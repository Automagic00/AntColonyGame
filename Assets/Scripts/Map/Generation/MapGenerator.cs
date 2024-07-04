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

    [HideInInspector]
    public List<Room> allRooms = new List<Room>(),
        traderRooms = new List<Room>(),
        itemRooms = new List<Room>(),
        nurseRooms = new List<Room>(),
        courierRooms = new List<Room>();

    [HideInInspector]
    public int[] roomCount;


    public TileBase[] doorTiles;

    [HideInInspector]
    public Tilemap fg, bg, plat;

    [HideInInspector]
    public float seed;


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

    public string[] startingRooms;

    [HideInInspector]
    public List<Node> genQueue = new List<Node>();

    [HideInInspector]
    public List<Node> acceptedNodes = new List<Node>();
    [HideInInspector]
    public List<BoundsInt> blockedArea = new List<BoundsInt>();

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
}