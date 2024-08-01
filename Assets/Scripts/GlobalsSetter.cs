using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalsSetter : MonoBehaviour
{

    public int skipToProgression = -1;

    public bool enableNurse, enableCourier;
    void Awake()
    {
        if (Globals.majorProgression < skipToProgression)
            Globals.majorProgression = skipToProgression;
        if (enableNurse)
            Globals.nurseEnabled = true;
        if (enableCourier)
            Globals.courierEnabled = true;
    }

}
