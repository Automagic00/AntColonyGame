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
        if (canAdvanceDialouge && Input.GetKeyDown(KeyCode.F)) AdvanceDialogue();
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
            AdvanceDialogue();
            return;
        }

        // Setup textbox
        bool lastDia = true;
        for (int i = diaStep + 1; i < dialogue.Count; i++)
            if (dialogue[i].text != null)
            {
                lastDia = false;
                break;
            }

        transform.Find("Text").GetComponent<TextMeshProUGUI>().text = item.text;
        transform.Find("ContinueText").GetComponent<TextMeshProUGUI>().text = lastDia ? "[F] done" : "[F] continue...";
        canAdvanceDialouge = true;
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