using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject ItemPrefab;
    [System.Serializable]
    public class ItemSpawnData
    {
        public Item itemData;
        public float weight;
    }

    public ItemSpawnData[] itemSpawnData;
    private GameObject itemSpawned;
    private GameObject enemyPrefabRespawn;
    private bool respawning = false;
    private Coroutine respawnRoutine;

    // Start is called before the first frame update
    void Start()
    {
        float totalWeight = 0;

        foreach (var item in itemSpawnData)
        {
            totalWeight += item.weight;
        }

        float randomPick = Random.Range(0, totalWeight);
        float testWeight = 0;

        foreach (var item in itemSpawnData)
        {
            testWeight += item.weight;
            if (testWeight >= randomPick)
            {
                if (item.itemData != null)
                {
                    itemSpawned = Instantiate(ItemPrefab, transform.position, transform.rotation);
                    itemSpawned.GetComponent<ItemBehavior>().item = item.itemData;
                }
                break;
            }
        }
    }

   /* private void Update()
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
    }*/
}
