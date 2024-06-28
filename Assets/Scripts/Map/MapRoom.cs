

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Room", menuName = "Room")]
public class Room : ScriptableObject
{
    public const string G_LEFT = "gen_left";
    public const string G_RIGHT = "gen_right";
    public const string G_UP = "gen_up";
    public const string G_DOWN = "gen_down";

    public GameObject layout;
    public float weight = 1;
    public float depth = 1;
    public int overheadBounds = 8;
    protected float currentWeight;

    public bool allowMirror = true;

    private bool _init;

    private List<Vector3Int> _leftExits = new List<Vector3Int>(),
    _rightExits = new List<Vector3Int>(),
    _upExits = new List<Vector3Int>(),
    _downExits = new List<Vector3Int>();

    private Tilemap _bg, _fg, _plat;

    private List<Transform> _objs = new List<Transform>();

    public void refreshInitialization()
    {
        _init = false;
        _leftExits.Clear();
        _rightExits.Clear();
        _upExits.Clear();
        _downExits.Clear();
        _objs.Clear();
    }
    private void loadExits()
    {
        _init = true;

        _bg = layout.transform.Find("pfBGTiles").GetComponent<Tilemap>();
        _fg = layout.transform.Find("pfTiles").GetComponent<Tilemap>();
        _plat = layout.transform.Find("pfPlatforms").GetComponent<Tilemap>();
        for (int i = 0; i < layout.transform.Find("objects").childCount; i++)
            _objs.Add(layout.transform.Find("objects").GetChild(i));

        BoundsInt bounds = _fg.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y);
                TileBase tile = _fg.GetTile(pos);
                if (tile == null) continue;
                switch (tile.name)
                {
                    case G_LEFT:
                        _leftExits.Add(pos);
                        break;
                    case G_RIGHT:
                        _rightExits.Add(pos);
                        break;
                    case G_UP:
                        _upExits.Add(pos);
                        break;
                    case G_DOWN:
                        _downExits.Add(pos);
                        break;
                    default: break;
                }
            }
    }

    public List<Transform> objs
    {
        get
        {
            if (!_init)
                loadExits();
            return _objs;
        }
    }

    public Tilemap bg
    {
        get
        {
            if (!_init)
                loadExits();
            return _bg;
        }
    }
    public Tilemap fg
    {
        get
        {
            if (!_init)
                loadExits();
            return _fg;
        }
    }

    public Tilemap plat
    {
        get
        {
            if (!_init)
                loadExits();
            return _plat;
        }
    }


    public List<Vector3Int> leftExits
    {
        get
        {
            if (!_init)
                loadExits();
            return _leftExits;
        }
    }
    public List<Vector3Int> rightExits
    {
        get
        {
            if (!_init)
                loadExits();
            return _rightExits;
        }
    }
    public List<Vector3Int> upExits
    {
        get
        {
            if (!_init)
                loadExits();
            return _upExits;
        }
    }
    public List<Vector3Int> downExits
    {
        get
        {
            if (!_init)
                loadExits();
            return _downExits;
        }
    }

    public bool hasLeft
    {
        get
        {
            return leftExits.Count > 0;
        }
    }
    public bool hasRight
    {
        get
        {
            return rightExits.Count > 0;
        }
    }

    public bool hasUp
    {
        get
        {
            return upExits.Count > 0;
        }
    }
    public bool hasDown
    {
        get
        {
            return downExits.Count > 0;
        }
    }
    public int exits
    {
        get
        {
            return leftExits.Count + rightExits.Count + upExits.Count + downExits.Count;
        }
    }

    public BoundsInt bounds
    {
        get
        {
            return fg.cellBounds;
        }
    }
    public Room mirror()
    {
        if (!allowMirror) return null;


        Room mirrored = ScriptableObject.CreateInstance<Room>();
        mirrored.weight = weight;
        mirrored.allowMirror = false;
        // Create a clone of the layout
        mirrored.layout = Instantiate(layout);
        Destroy(mirrored.layout);
        mirrored.layout.name = layout.name + "_Mirrored";

        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y);
                Vector3Int mirrorPos = new Vector3Int(bounds.xMax - 1 - (x - bounds.xMin), y);
                mirrored.bg.SetTile(pos, bg.GetTile(mirrorPos));
                mirrored.fg.SetTile(pos, fg.GetTile(mirrorPos));
                mirrored.plat.SetTile(pos, plat.GetTile(mirrorPos));

                // Invert arrows
                TileBase tile = mirrored.fg.GetTile(pos);
                if (tile == doors[0])
                    mirrored.fg.SetTile(pos, doors[1]);
                else if (tile == doors[1])
                    mirrored.fg.SetTile(pos, doors[0]);
            }
        foreach (Transform obj in mirrored.objs)
            obj.position = new Vector3(bounds.xMax - (obj.position.x - bounds.xMin), obj.position.y, obj.position.z);

        mirrored.refreshInitialization();
        return mirrored;
    }

    public void Prepare()
    {
        currentWeight = weight;
    }
    public void Used(float maxDepth)
    {
        currentWeight -= 1.0f / maxDepth;
    }
    public void UndoUsed(float maxDepth)
    {
        currentWeight += 1.0f / maxDepth;

    }

    public static Room weightedRandom(List<Room> options)
    {
        float totalWeight = 0;

        foreach (Room r in options) totalWeight += Math.Max(0.1f, r.currentWeight);

        float weight = UnityEngine.Random.Range(0, totalWeight);

        foreach (Room r in options)
        {
            weight -= Math.Max(0.1f, r.currentWeight);
            if (weight <= 0) return r;
        }
        return options.Last();
    }


    static TileBase[] _doors;
    static TileBase[] doors
    {
        get
        {
            if (_doors == null) _doors = FindFirstObjectByType<MapGenerator>().doorTiles;
            return _doors;
        }
    }
}
