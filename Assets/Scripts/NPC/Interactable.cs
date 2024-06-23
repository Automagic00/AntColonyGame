using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{

    const float OUTLINE_SIZE = 2.0f;

    public float interactDistance = 3.0f;

    public Color outlineColor = Color.white;

    public Interactor interactor;

    private GameObject outline;
    private SpriteRenderer sprite;
    private Sprite lastUpdatedSprite;


    void Awake()
    {
        // Setup outline to use sprite
        outline = transform.Find("Outline").gameObject;
        sprite = GetComponent<SpriteRenderer>();

        // Set constants from parent sprite renderer
        for (int i = 0; i < outline.transform.childCount; i++)
        {
            SpriteRenderer childSpr = outline.transform.GetChild(i).GetComponent<SpriteRenderer>();
            childSpr.color = outlineColor;
            childSpr.sortingLayerID = sprite.sortingLayerID;
            childSpr.sortingOrder = sprite.sortingOrder - 1;
            childSpr.flipX = sprite.flipX;
            childSpr.flipY = sprite.flipY;

            outline.transform.GetChild(i).transform.localPosition *= OUTLINE_SIZE / sprite.sprite.pixelsPerUnit;
        }
        updateOutlineSprites();
    }
    private void updateOutlineSprites()
    {
        // Only update if new sprite
        if (lastUpdatedSprite == sprite.sprite) return;
        lastUpdatedSprite = sprite.sprite;

        for (int i = 0; i < outline.transform.childCount; i++)
        {
            SpriteRenderer childSpr = outline.transform.GetChild(i).GetComponent<SpriteRenderer>();
            childSpr.sprite = sprite.sprite;
        }
    }
    public void Update()
    {
        updateOutlineSprites();
    }

    public static Interactable closestInteractable(Vector3 position)
    {

        Interactable interact = null;
        float dist = 999;

        foreach (Interactable i in FindObjectsOfType<Interactable>())
        {
            if (!i.interactor.canInteract) continue;

            float iDist = Vector3.Distance(i.transform.position, position);
            if (iDist < i.interactDistance && iDist < dist)
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
