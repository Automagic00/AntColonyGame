using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
//using UnityEngine.U2D;

public class MiniMapSizeToggle : MonoBehaviour
{
    float currentScale = 1;
    public PixelPerfectCamera mapCam;
    public GameObject map;
    public GameObject miniMap;

    private void Start()
    {
        mapCam = GameObject.Find("Main Camera").transform.Find("Minimap Camera").GetComponent<PixelPerfectCamera>();
        map.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!PauseController.gameIsPaused && currentScale != 1)
        {
            ToggleSize();
        }
    }

    public void ToggleSize()
    {
        if (currentScale == 1 && PauseController.gameIsPaused)
        {
            currentScale = 3;
            map.SetActive(true);
            miniMap.SetActive(false);
            mapCam.assetsPPU = 8;
            map.transform.localScale = new Vector2(currentScale, currentScale);
        }
        else
        {
            currentScale = 1;
            map.SetActive(false);
            miniMap.SetActive(true);
            mapCam.assetsPPU = 24;
            map.transform.localScale = new Vector2(currentScale, currentScale);
        }


    }
}
