using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public float weight;
    }

    public EnemySpawnData[] enemySpawnData;

    // Start is called before the first frame update
    void Start()
    {
        float totalWeight = 0;

        foreach (var enemy in enemySpawnData)
        {
            totalWeight += enemy.weight;
        }

        float randomPick = Random.Range(0, totalWeight);
        float testWeight = 0;

        foreach (var enemy in enemySpawnData)
        {
            testWeight += enemy.weight;
            if (testWeight >= randomPick)
            {
                Instantiate(enemy.enemyPrefab, transform.position, transform.rotation);
                break;
            } 
        }
    }
}
