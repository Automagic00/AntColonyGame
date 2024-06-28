

using UnityEngine;

public class SpawnChance : MonoBehaviour
{
    public float minRange = 0;
    public float maxRange = 100;

    public bool SpawnCheck(float value)
    {
        return value >= minRange && value <= maxRange;
    }
}