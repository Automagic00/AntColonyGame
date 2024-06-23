

using System;
using UnityEngine;


public class Item : Interactable
{
    public ItemData item;

    // These are only to set the item directly from the editor
    public string itemName;
    public Sprite itemSprite;

    public virtual void Start()
    {
        if (itemName != null)
        {
            item = new ItemData() { name = itemName, sprite = itemSprite };
        }
        GetComponent<SpriteRenderer>().sprite = item.sprite;
    }

    // Pick up on interact
    public override void interact()
    {
        Inventory player = GameObject.Find("Player").GetComponent<Inventory>();

        if (player.carry != null)
            player.dropCarry();

        player.carry = item;
        Destroy(this.gameObject);
    }
}

public class ItemData
{
    public string name;
    public Sprite sprite;

}


public abstract class Equipment : ItemData
{

    // Multiplicative
    public float groundspeedMod = 1;
    public float airspeedMod = 1;
    public float damageMod = 1;
    public float jumpHeightMod = 1;
    public float hpMod = 1;


    // Additive
    public int doubleJumpMod = 0;


}

public abstract class Weapon : Equipment
{

}

public abstract class Ring : Equipment
{

}