using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactive : MonoBehaviour
{


    public float outlineSize = 2;
    public float interactDistance = 3.0f;

    public Color outlineColor = Color.white;

    private bool inRange;

    public Interactable interactor;

    private GameObject outline;
    private SpriteRenderer sprite;
    private Sprite lastUpdatedSprite;


    void Start()
    {
        // Setup outline to use sprite
        outline = transform.Find("Outline").gameObject;
        
        if (GetComponent<SpriteRenderer>() != null)
        {
            sprite = GetComponent<SpriteRenderer>();
        }
        else if(GetComponentInChildren<SpriteRenderer>() != null)
        {
            sprite = GetComponentInChildren<SpriteRenderer>();
        }

        // Set constants from parent sprite renderer
        for (int i = 0; i < outline.transform.childCount; i++)
        {
            SpriteRenderer childSpr = outline.transform.GetChild(i).GetComponent<SpriteRenderer>();
            childSpr.color = outlineColor;
            childSpr.sortingLayerID = sprite.sortingLayerID;
            childSpr.sortingOrder = sprite.sortingOrder - 1;
            childSpr.flipX = sprite.flipX;
            childSpr.flipY = sprite.flipY;

            outline.transform.GetChild(i).transform.localPosition *= outlineSize / sprite.sprite.pixelsPerUnit;
        }
        updateOutlineSprites();
    }

    public void SetOutlineColor(Color color)
    {
        if (color == outlineColor) return;
        outlineColor = color;

        for (int i = 0; i < outline.transform.childCount; i++)
        {
            SpriteRenderer childSpr = outline.transform.GetChild(i).GetComponent<SpriteRenderer>();
            childSpr.color = outlineColor;
        }
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

    public static Interactive closestInteractable(Vector3 position)
    {
        Interactive interact = null;
        float dist = 999;


        foreach (Interactive i in FindObjectsOfType<Interactive>())
        {
            float iDist = Vector3.Distance(i.transform.position, position);

            // Enable / disable interaction range
            if (iDist < i.interactDistance && !i.inRange)
            {
                i.inRange = true;
                i.interactor.enterInteractionRange();
            }
            if (iDist >= i.interactDistance && i.inRange)
            {
                i.inRange = false;
                i.interactor.exitInteractionRange();
            }

            if (iDist > i.interactDistance) continue;
            if (!i.interactor.canInteract) continue;

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
    }
    public void disableInteraction()
    {
        transform.Find("Outline").gameObject.SetActive(false);
    }
}
