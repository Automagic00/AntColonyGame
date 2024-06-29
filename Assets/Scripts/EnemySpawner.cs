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
    public float respawnTime;

    public EnemySpawnData[] enemySpawnData;
    public GameObject enemySpawned;
    private GameObject enemyPrefabRespawn;
    private bool respawning = false;
    private Coroutine respawnRoutine;

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
                if (enemy.enemyPrefab != null)
                {
                    enemySpawned = Instantiate(enemy.enemyPrefab, transform.position, transform.rotation);
                    enemyPrefabRespawn = enemy.enemyPrefab;
                }
                break;
            }
        }
    }

    private void Update()
    {
        if (enemySpawned != null)
        {
            // Debug.Log("is visible: " + enemySpawned.GetComponentInChildren<SpriteRenderer>().isVisible);
            if (!enemySpawned.GetComponentInChildren<SpriteRenderer>().isVisible && enemySpawned.GetComponent<EnemyController>().GetCurrentSubState() == EnemyController.EntitySubStates.Dead && respawning == false)
            {
                respawning = true;
                respawnRoutine = StartCoroutine(TryRespawn());

            }
            else if (enemySpawned.GetComponentInChildren<SpriteRenderer>().isVisible)
            {
                if (respawnRoutine != null)
                    StopCoroutine(respawnRoutine);
                respawning = false;
            }
        }
        else
        {
            if (respawnRoutine != null)
                StopCoroutine(respawnRoutine);
            respawning = false;
        }
    }

    private IEnumerator TryRespawn()
    {
        yield return new WaitForSeconds(respawnTime);
        enemySpawned = Instantiate(enemyPrefabRespawn, transform.position, transform.rotation);
    }

}