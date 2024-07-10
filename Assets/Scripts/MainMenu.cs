using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject cloudHolder;

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (Transform cloud in cloudHolder.transform)
        {
            RectTransform rectTransform = cloud.GetComponent<RectTransform>();
            if (rectTransform.localPosition.x >= 1150)
                rectTransform.localPosition = new Vector2(-1150, rectTransform.localPosition.y);
            else
                rectTransform.localPosition += new Vector3(4, 0);
        }
    }

    public void PlayPressed()
    {
        SceneManager.LoadScene("Tutorial");
    }
}
