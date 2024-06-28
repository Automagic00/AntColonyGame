using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{

    private List<DialogueItem> dialogue = null;
    private int diaStep = -1;
    private bool canAdvanceDialouge = false;
    public static bool dialogueIsOpen = false;


    void Update()
    {
        if (canAdvanceDialouge && Input.GetKeyDown(KeyCode.F))
        {
            bool skippedText = skipText();
            if (!skippedText)
                AdvanceDialogue();
        }

        advanceText();
    }


    public static void OpenDialogue(Dialoguer target)
    {
        FindObjectOfType<Dialogue>(true).StartDia(target);
    }
    private void StartDia(Dialoguer target)
    {
        dialogueIsOpen = true;
        gameObject.SetActive(true);

        dialogue = target.getDialogue();
        diaStep = -1;

        AdvanceDialogue();
    }
    private void AdvanceDialogue()
    {
        if (dialogue == null) return;

        canAdvanceDialouge = false;
        diaStep++;
        if (diaStep >= dialogue.Count)
        {
            FinishDia();
            return;
        }

        DialogueItem item = dialogue[diaStep];

        if (item.name != null)
            transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.name;
        if (item.picture != null)
            transform.Find("Image").GetComponent<Image>().sprite = item.picture;
        if (item.action != null)
            item.action();

        // Go to next dialogue immediately if no textbox
        if (item.text == null)
        {
            fullText = "";
            AdvanceDialogue();
            return;
        }

        // Setup textbox
        lastDialogue = true;
        for (int i = diaStep + 1; i < dialogue.Count; i++)
            if (dialogue[i].text != null)
            {
                lastDialogue = false;
                break;
            }
        SetText(item.text);

    }

    public float characterAdvanceTime = 0.04f;
    private string fullText;
    private float advanceTime = 0;
    private int charactersShown = 0;
    private bool lastDialogue;

    private void SetText(string full)
    {
        fullText = full;
        advanceTime = 0;
        charactersShown = 0;
        canAdvanceDialouge = true;
        advanceText();

    }
    private bool skipText()
    {
        if (fullText == "") return false;

        if (charactersShown < fullText.Length)
        {
            charactersShown = fullText.Length;
            return true;
        }
        return false;

    }
    private void advanceText()
    {
        if (fullText == "") return;

        advanceTime += Time.deltaTime;
        charactersShown = Math.Max(charactersShown, (int)(advanceTime / characterAdvanceTime));

        if (charactersShown >= fullText.Length)
        {
            // Complete
            transform.Find("Text").GetComponent<TextMeshProUGUI>().text = fullText;
            transform.Find("ContinueText").GetComponent<TextMeshProUGUI>().text = lastDialogue ? "[F] done" : "[F] continue...";
        }
        else
        {
            transform.Find("Text").GetComponent<TextMeshProUGUI>().text = fullText.Substring(0, charactersShown);
            transform.Find("ContinueText").GetComponent<TextMeshProUGUI>().text = "[F]";
        }

    }
    private void FinishDia()
    {
        dialogueIsOpen = false;
        gameObject.SetActive(false);

        dialogue = null;
        diaStep = -1;
        canAdvanceDialouge = false;
    }
}

public interface Dialoguer
{

    List<DialogueItem> getDialogue();
}

public class DialogueItem
{
    public string name;
    public string text;
    public Action action;
    public Sprite picture;

}