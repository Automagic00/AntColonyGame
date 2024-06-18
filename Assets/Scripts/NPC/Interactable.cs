using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{

    const float OUTLINE_SIZE = 2.0f;

    public Color outlineColor = Color.white;

    public Interactor interactor;

    void Awake()
    {

        // Setup outline to use sprite
        GameObject outline = transform.Find("Outline").gameObject;
        SpriteRenderer parentSpr = GetComponent<SpriteRenderer>();

        for (int i = 0; i < outline.transform.childCount; i++)
        {
            SpriteRenderer childSpr = outline.transform.GetChild(i).GetComponent<SpriteRenderer>();
            childSpr.color = outlineColor;
            childSpr.sprite = parentSpr.sprite;
            childSpr.sortingLayerID = parentSpr.sortingLayerID;
            childSpr.sortingOrder = parentSpr.sortingOrder - 1;
            outline.transform.GetChild(i).transform.localPosition *= OUTLINE_SIZE / parentSpr.sprite.pixelsPerUnit;
        }
    }


    public void interact() => interactor.interact();

    public void enableInteraction()
    {
        transform.Find("Outline").gameObject.SetActive(true);
        interactor.enableInteraction();
    }
    public void disableInteraction()
    {
        transform.Find("Outline").gameObject.SetActive(false);
        interactor.disableInteraction();
    }
}
