using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    PlayerController player;
    public GameObject display;
    public AudioClip[] mouseOverSounds;
    public AudioClip confirmSound;

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
        // TODO re-enable after jam
        // inventory.ClearInventory();
        inventory.ClearInventory(false, true, false);

        AudioSource.PlayClipAtPoint(confirmSound, inventory.transform.position);
        SceneManager.LoadScene("Anthill");
    }
    public void MouseOver()
    {
        AudioClip clip = mouseOverSounds[Random.Range(0, mouseOverSounds.Length)];
        AudioSource.PlayClipAtPoint(clip, GameObject.Find("Player").transform.position);
    }
}
