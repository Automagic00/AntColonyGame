using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : Interactable
{

    public string text, sceneName;
    Color fade;
    private TextMeshPro overhead;

    public override void interact()
    {
        SceneManager.LoadScene(sceneName);
    }

    void Start()
    {
        overhead = transform.Find("Overhead").GetComponent<TextMeshPro>();
        overhead.text = text.ToUpper();

        // Fades in
        overhead.color = new Color(1, 1, 1, 0);
        fade = overhead.color;
    }

    private Coroutine currentCoroutine;

    public override void enterInteractionRange()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FadeTextIn(0.5f));
    }
    public override void exitInteractionRange()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(FadeTextOut(0.5f));
    }
    private IEnumerator FadeTextIn(float time)
    {
        while (overhead.color.a < 1.0f)
        {
            //fade.a = overhead.color.a;
            fade.a = overhead.color.a + Time.deltaTime / time;
            overhead.color = fade;
            yield return null;
        }
    }
    private IEnumerator FadeTextOut(float time)
    {
        while (overhead.color.a > 0.0f)
        {
            fade.a = overhead.color.a - Time.deltaTime / time;
            overhead.color = fade;
            yield return null;
        }
    }
}
