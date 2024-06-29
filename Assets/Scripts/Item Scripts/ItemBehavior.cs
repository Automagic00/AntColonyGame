
using System;
using UnityEngine;

public class ItemBehavior : Interactable
{
    [SerializeField]
    private Item _item;
    public Inventory player;

    public virtual void Start()
    {
        player = GameObject.Find("Player").GetComponent<Inventory>();
        if (_item != null) item = _item;
    }
    public Item item
    {
        get => _item;
        set
        {
            _item = value;
            GetComponent<SpriteRenderer>().sprite = _item.sprite;
        }
    }


    // Pick up on interact
    public override void interact()
    {
        if (_item.GetType() == typeof(Weapon))
        {
            if (player.weapon != null)
                player.dropWeapon(true);

            player.weapon = (Weapon)_item;
            Destroy(this.gameObject);
        }
        else
        {
            if (player.carry != null)
                player.dropCarry(true);

            player.carry = _item;
            Destroy(this.gameObject);
        }
    }
}