using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [System.Serializable]
    public class ItemRarityData
    {
        public Equipment.Rarity rarity;
        public float weight;
    }



    public ItemSpawnData[] itemSpawnData;
    public ItemRarityData[] itemRarityData;
    private GameObject itemSpawned;

    // Start is called before the first frame update
    void Start()
    {
        //TODO: Stop enemies from spawning twice on mapGen
        //Delay is a temp fix
        StartCoroutine(Delay());
    }
    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(1f);
        float totalWeight = 0;
        float totalRarityWeight = 0;
        foreach (var item in itemSpawnData)
        {
            totalWeight += item.weight;
        }
        foreach (var item in itemRarityData)
        {
            totalRarityWeight += item.weight;
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

        randomPick = Random.Range(0, totalRarityWeight);
        testWeight = 0;

        foreach (var item in itemRarityData)
        {
            testWeight += item.weight;
            if (totalRarityWeight >= randomPick)
            {
                if (itemSpawned != null && (itemSpawned.GetComponent<ItemBehavior>().item.GetType() == typeof(Equipment) || itemSpawned.GetComponent<ItemBehavior>().item.GetType() == typeof(Weapon) || itemSpawned.GetComponent<ItemBehavior>().item.GetType() == typeof(Ring)))
                {
                    //Debug.Log("IsEquipment");
                    if (itemSpawned.GetComponent<ItemBehavior>().item.GetType() == typeof(Weapon))
                    {
                        Weapon baseData = (Weapon)itemSpawned.GetComponent<ItemBehavior>().item;
                        Weapon equipment = Weapon.CreateWeapon(baseData);
                        itemSpawned.GetComponent<ItemBehavior>().item = equipment;

                        equipment.rarity = item.rarity;
                        Debug.Log(equipment.rarity);
                        //Add Modifiers
                        for (int i = 1; i < (int)item.rarity; i++)
                        {
                            Debug.Log("RollMod");
                            RollModifier(equipment);
                        }


                    }
                    else if (itemSpawned.GetComponent<ItemBehavior>().item.GetType() == typeof(Ring))
                    {
                        Ring baseData = (Ring)itemSpawned.GetComponent<ItemBehavior>().item;
                        Ring equipment = Ring.CreateRing(baseData);
                        itemSpawned.GetComponent<ItemBehavior>().item = equipment;

                        equipment.rarity = item.rarity;
                        //Add Modifiers
                        for (int i = 1; i < (int)item.rarity; i++)
                        {
                            RollModifier(equipment);
                        }
                    }

                }
                break;
            }
        }


    }

    private void RollModifier(Equipment equip)
    {
        List<Equipment.Modifiers> combinedModList;
        if (equip.baseModifiers != null)
        {
            combinedModList = equip.baseModifiers.Concat(equip.modifiers).ToList();
        }
        else
        {
            combinedModList = equip.modifiers;
        }

        int modRolled = Random.Range(0, System.Enum.GetNames(typeof(Equipment.Modifiers)).Length);

        if (combinedModList != null)
        {
            //No Duplicate modifiers
            if (combinedModList.Contains((Equipment.Modifiers)modRolled))
            {
                RollModifier(equip);
                return;
            }
            //Blunt & Sharp Mutually Exclusive
            else if ((modRolled == (int)Equipment.Modifiers.Sharp && combinedModList.Contains(Equipment.Modifiers.Blunt)) || (modRolled == (int)Equipment.Modifiers.Blunt && combinedModList.Contains(Equipment.Modifiers.Sharp)))
            {
                RollModifier(equip);
                return;
            }
            else
            {
                equip.modifiers.Add((Equipment.Modifiers)modRolled);
            }
        }
        else
        {
            equip.modifiers = new List<Equipment.Modifiers>();
            equip.modifiers.Add((Equipment.Modifiers)modRolled);
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
