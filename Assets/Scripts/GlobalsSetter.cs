using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalsSetter : MonoBehaviour
{

    public int skipToProgression = -1;
    void Awake()
    {
        if (Globals.gameProgression < skipToProgression)
            Globals.gameProgression = skipToProgression;
    }

}
