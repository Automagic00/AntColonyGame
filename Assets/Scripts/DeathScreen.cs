using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    PlayerController player;
    public GameObject display;

    private void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        display.SetActive(false);
    }

    private void Update()
    {
        if (player.GetCurrentSubState() == Entity.EntitySubStates.Dead && display.activeSelf == false)
        {
            display.SetActive(true);
        }
    }
    public void ReturnToColony()
    {
        Inventory inventory = GameObject.Find("Player").GetComponent<Inventory>();
        inventory.ClearInventory();

        SceneManager.LoadScene("Anthill");
    }
}
