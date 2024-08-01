using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public static class Globals
{
    public static Bounds mapBounds;

    public static void BoundToTilemap(Tilemap tilemap)
    {
        tilemap.CompressBounds();

        Vector3 worldmin = tilemap.transform.TransformPoint(tilemap.localBounds.min);
        Vector3 worldmax = tilemap.transform.TransformPoint(tilemap.localBounds.max) + new Vector3(0, 16, 0);

        mapBounds = new Bounds();
        mapBounds.SetMinMax(worldmin, worldmax);
    }

    private static int _majorProgression = 0, _minorProgression = 0;
    public static bool nurseEnabled, courierEnabled;

    public static int majorProgression
    {
        get => _majorProgression;
        set
        {
            _majorProgression = value;
            _minorProgression = 0;
            foreach (UnityAction a in onProgression) a();
        }
    }
    public static int minorProgression
    {
        get => _minorProgression;
        set
        {
            _minorProgression = value;
            foreach (UnityAction a in onProgression) a();
        }
    }

    private static List<UnityAction> onProgression = new List<UnityAction>();
    public static void addProgressionListener(UnityAction onProgress) => onProgression.Add(onProgress);
    public static void removeProgressionListener(UnityAction onProgress) => onProgression.Remove(onProgress);
}
