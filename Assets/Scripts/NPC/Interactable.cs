using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{

    const float OUTLINE_SIZE = 2.0f;

    public const float INTERACT_DISTANCE = 3.0f;

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
            childSpr.flipX = parentSpr.flipX;
            childSpr.flipY = parentSpr.flipY;

            outline.transform.GetChild(i).transform.localPosition *= OUTLINE_SIZE / parentSpr.sprite.pixelsPerUnit;
        }
    }

    public static Interactable closestInteractable(Vector3 position)
    {

        Interactable interact = null;
        float dist = Interactable.INTERACT_DISTANCE;

        foreach (Interactable i in FindObjectsOfType<Interactable>())
        {
            if (!i.interactor.canInteract) continue;

            float iDist = Vector3.Distance(i.transform.position, position);
            if (iDist < dist)
            {
                interact = i;
                dist = iDist;
            }
        }
        return interact;
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
