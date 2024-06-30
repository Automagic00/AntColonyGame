
using System;
using UnityEngine;

public class ItemBehavior : Interactable
{
    [SerializeField]
    private Item _item;
    private Inventory player;

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
            if (_item != null)
            {
                GetComponent<SpriteRenderer>().sprite = _item.sprite;
                GetComponent<BoxCollider2D>().size = new Vector2(_item.xSize, _item.ySize);
            }
            else
            {
                GetComponent<SpriteRenderer>().sprite = null;
                GetComponent<BoxCollider2D>().size = new Vector2(1, 1);
            }
        }
    }


    // Pick up on interact
    public override void interact()
    {
        if (_item is Weapon)
        {
            if (player.weapon == null)
                player.weapon = (Weapon)_item;
            else if (player.carry == null)
                player.carry = _item;
            else
            {
                player.dropWeapon(true);
                player.weapon = (Weapon)_item;
            }
            Debug.Log(player.weapon);

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