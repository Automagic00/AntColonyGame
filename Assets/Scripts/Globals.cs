using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class Globals
{
    public static Bounds mapBounds;

    private static int _progression;

    public static int gameProgression
    {
        get => _progression;
        set
        {
            _progression = value;
            foreach (UnityAction a in onProgression) a();
        }
    }

    private static List<UnityAction> onProgression = new List<UnityAction>();
    public static void addProgressionListener(UnityAction onProgress) => onProgression.Add(onProgress);
}
