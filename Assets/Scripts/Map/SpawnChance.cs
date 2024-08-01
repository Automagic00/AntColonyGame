

using UnityEngine;

public class SpawnChance : MonoBehaviour
{
    public float roomMinRange = 0;
    public float roomMaxRange = 100;
    public float sceneMinRange = 0;
    public float sceneMaxRange = 100;
    public int afterGameProgression = 0;

    public bool SpawnCheck(float roomValue, float sceneValue)
    {
        if (Globals.majorProgression < afterGameProgression) return false;
        return roomValue >= roomMinRange && roomValue <= roomMaxRange &&
        sceneValue >= sceneMinRange && sceneValue <= sceneMaxRange;
    }
}