using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseController : MonoBehaviour
{
    public static bool gameIsPaused;
    public InventoryUI pauseMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameIsPaused = !gameIsPaused;
            PauseGame();
        }
    }
    void PauseGame()
    {
        if (gameIsPaused)
        {
            Time.timeScale = 0f;
            pauseMenu.Pause();
            pauseMenu.gameObject.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            pauseMenu.gameObject.SetActive(false);
            pauseMenu.Unpause();
        }
    }
}
