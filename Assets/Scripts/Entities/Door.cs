using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : Interactable
{

    public string text, sceneName;

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
        overhead.color = Color.white.WithAlpha(0);
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
            overhead.color = overhead.color.WithAlpha(overhead.color.a + Time.deltaTime / time);
            yield return null;
        }
    }
    private IEnumerator FadeTextOut(float time)
    {
        while (overhead.color.a > 0.0f)
        {
            overhead.color = overhead.color.WithAlpha(overhead.color.a - Time.deltaTime / time);
            yield return null;
        }
    }
}
