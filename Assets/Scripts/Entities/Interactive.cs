using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Interactive : MonoBehaviour
{
    public GameObject statWindowPrefab;
    private GameObject statWindow;

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
        else if (GetComponentInChildren<SpriteRenderer>() != null)
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

            if (sprite.sprite != null)
                outline.transform.GetChild(i).transform.localPosition *= outlineSize / sprite.sprite.pixelsPerUnit;
        }
        updateOutlineSprites();

        if (statWindowPrefab != null && GetComponent<ItemBehavior>().item != null && (GetComponent<ItemBehavior>().item.GetType() == typeof(Equipment) || GetComponent<ItemBehavior>().item.GetType() == typeof(Weapon) || GetComponent<ItemBehavior>().item.GetType() == typeof(Ring)))
        {
            statWindow = Instantiate(statWindowPrefab, transform.position, transform.rotation, transform);
            statWindow.SetActive(false);
        }
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
        if (Dialogue.dialogueIsOpen) return null;
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
        if (statWindow != null)
        {
            statWindow.SetActive(true);
            updateStatWindow();
        }
    }
    public void disableInteraction()
    {
        transform.Find("Outline").gameObject.SetActive(false);
        if (statWindow != null)
        {
            statWindow.SetActive(false);
        }
    }

    public void updateStatWindow()
    {
        Equipment itemData = (Equipment)GetComponent<ItemBehavior>().item;
        TMP_Text name = statWindow.transform.Find("Name").GetComponent<TMP_Text>();
        TMP_Text rarity = statWindow.transform.Find("Rarity").GetComponent<TMP_Text>();
        TMP_Text modifiers = statWindow.transform.Find("Modifiers").GetComponent<TMP_Text>();

        name.text = itemData.name;

        switch (itemData.rarity)
        {
            case Equipment.Rarity.Common: rarity.text = "Common"; rarity.color = Color.white; break;
            case Equipment.Rarity.Uncommon: rarity.text = "Uncommon"; rarity.color = Color.green; break;
            case Equipment.Rarity.Rare: rarity.text = "Rare"; rarity.color = Color.blue; break;
            case Equipment.Rarity.Mythic: rarity.text = "Mythic"; rarity.color = Color.magenta; break;
            case Equipment.Rarity.Legendary: rarity.text = "Legendary"; rarity.color = new Color(245f / 255f, 135f / 255f, 39f / 255f, 1); break;
            default: break;
        }


        modifiers.text = "";
        modifiers.color = rarity.color;
        foreach (Equipment.Modifiers itemMods in itemData.modifiers)
        {
            switch (itemMods)
            {
                case Equipment.Modifiers.Blunt: modifiers.text += "Blunt \n"; break;
                case Equipment.Modifiers.FeatherWeight: modifiers.text += "Feather Weight \n"; break;
                case Equipment.Modifiers.GlassCanon: modifiers.text += "Glass Cannon \n"; break;
                case Equipment.Modifiers.Multishot: modifiers.text += "Multishot \n"; break;
                case Equipment.Modifiers.Piercing: modifiers.text += "Piercing \n"; break;
                case Equipment.Modifiers.RolyPolySoul: modifiers.text += "Roly Poly Soul \n"; break;
                case Equipment.Modifiers.ScarabBeetleSoul: modifiers.text += "Scarab Beetle Soul \n"; break;
                case Equipment.Modifiers.Sharp: modifiers.text += "Sharp \n"; break;
                case Equipment.Modifiers.SprintersSpikes: modifiers.text += "Sprinters Spikes \n"; break;
                case Equipment.Modifiers.StoneSkin: modifiers.text += "Stone Skin \n"; break;
                case Equipment.Modifiers.WaspSoul: modifiers.text += "Wasp Soul \n"; break;
            }
        }

    }

}
