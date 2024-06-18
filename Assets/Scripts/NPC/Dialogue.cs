using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{

    private Dialoguer currentDia = null;
    private int diaStep = -1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) AdvanceDialogue();
    }


    public static void OpenDialogue(Dialoguer target)
    {
        FindObjectOfType<Dialogue>(true).StartDialogue(target);
    }
    private void StartDialogue(Dialoguer target)
    {
        gameObject.SetActive(true);

        currentDia = target;
        diaStep = -1;

        AdvanceDialogue();
    }
    private void AdvanceDialogue()
    {
        if (currentDia == null) return;

        diaStep++;
        Debug.Log(diaStep);

        // Finish if past count
        if (diaStep >= currentDia.getDialogueCount())
        {
            FinishDialogue();
            return;
        }

        bool finish = diaStep + 1 >= currentDia.getDialogueCount();

        // Set current dialogue on screen
        transform.Find("Name").GetComponent<TextMeshProUGUI>().text = currentDia.getName();
        transform.Find("Text").GetComponent<TextMeshProUGUI>().text = currentDia.getDialogueContent(diaStep);
        transform.Find("Image").GetComponent<Image>().sprite = currentDia.getPicture(diaStep);
        transform.Find("ContinueText").GetComponent<TextMeshProUGUI>().text = finish ? "[F] done" : "[F] continue...";
    }
    private void FinishDialogue()
    {
        Debug.Log("FINISH");
        gameObject.SetActive(false);

        currentDia.dialogueFinished();
        currentDia = null;
        diaStep = -1;
    }
}

public interface Dialoguer
{

    int getDialogueCount();

    String getName();
    String getDialogueContent(int i);
    Sprite getPicture(int i);



    void dialogueFinished();
}
