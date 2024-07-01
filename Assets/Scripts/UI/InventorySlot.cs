using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{

    public enum Slot
    {
        Carry,
        Weapon,
        Ring,
        Mouse,
    }
    public Slot slot;
    public int ringNumber;

    private Inventory player;
    private InventorySlot mouse;

    private Image sprite;

    private Item lastItem;
    private Canvas canvas;

    void Start()
    {
        canvas = FindAnyObjectByType<Canvas>();
        player = FindAnyObjectByType<Inventory>();
        mouse = transform.parent.Find("Mouse Slot").GetComponent<InventorySlot>();
        sprite = transform.Find("item").GetComponent<Image>();
    }


    void Update()
    {
        // Update sprite if item changes
        if (lastItem != GetItem())
        {
            lastItem = GetItem();
            setSprite(lastItem);
        }

        // Track mouse
        if (slot == Slot.Mouse)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out pos);
            transform.position = canvas.transform.TransformPoint(pos);
        }
    }


    public void Clicked()
    {
        if (slot == Slot.Mouse)
            return;

        // Swap current and mouse
        Item prev = GetItem();
        Item put = mouse.GetItem();
        if (SetItem(put))
            mouse.SetItem(prev);

    }


    private bool SetItem(Item set)
    {
        switch (slot)
        {
            case Slot.Mouse:
                player.mouseHold = set;
                return true;
            case Slot.Carry:
                player.carry = set;
                return true;
            case Slot.Weapon:
                if (set == null || set is Weapon)
                {
                    player.weapon = (Weapon)set;
                    return true;
                }
                return false;
            case Slot.Ring:
                if (set == null || set is Ring)
                {
                    player.SetRing((Ring)set, ringNumber);
                    return true;
                }
                return false;

        }
        return false;
    }
    public Item GetItem()
    {
        switch (slot)
        {
            case Slot.Mouse:
                return player.mouseHold;
            case Slot.Carry:
                return player.carry;
            case Slot.Weapon:
                return player.weapon;
            case Slot.Ring:
                return player.GetRing(ringNumber);
        }
        return null;

    }
    private void setSprite(Item set)
    {
        if (set == null) sprite.color = Color.clear;
        else
        {
            sprite.color = Color.white;
            sprite.sprite = set.sprite;
        }
    }

    
}
