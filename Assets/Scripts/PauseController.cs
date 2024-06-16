using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseController : MonoBehaviour
{
    public static bool gameIsPaused;
    public GameObject pauseText;

    // Update is called once per frame
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
            pauseText.gameObject.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            pauseText.gameObject.SetActive(false);
        }
    }
}
