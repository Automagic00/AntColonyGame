
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Node
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
            // if (parent == null && root.startingRooms.Length > 0
            // && !root.startingRooms.Contains(room.roomType)) continue;

            switch (from)
            {
                case Room.G_LEFT:
                    if (!room.hasRight) continue;
                    // if (parent != null && parent.room.allowedRoomsL.Length > 0
                    // && !parent.room.allowedRoomsL.Contains(room.roomType)) continue;
                    break;
                case Room.G_RIGHT:
                    if (!room.hasLeft) continue;
                    // if (parent != null && parent.room.allowedRoomsR.Length > 0
                    // && !parent.room.allowedRoomsR.Contains(room.roomType)) continue;
                    break;
                case Room.G_UP:
                    if (!room.hasDown) continue;
                    // if (parent != null && parent.room.allowedRoomsU.Length > 0
                    // && !parent.room.allowedRoomsU.Contains(room.roomType)) continue;
                    break;
                case Room.G_DOWN:
                    if (!room.hasUp) continue;
                    // if (parent != null && parent.room.allowedRoomsD.Length > 0
                    // && !parent.room.allowedRoomsD.Contains(room.roomType)) continue;
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

    private Transform Instantiate(Transform t, Vector3 vector3, Quaternion identity)
    {
        throw new System.NotImplementedException();
    }
}