
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

        Debug.Assert(_item != null);
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
        else if (_item is Ring)
        {
            bool ringFull = false;
            for (int i = 0; i < 6; i++)
            {
                if (player.GetRing(i) == null)
                {
                    player.SetRing((Ring)_item, i);
                    break;
                }
                else if (i == 5)
                {
                    ringFull = true;
                }
            }

            if (ringFull)
            {
                if (player.carry == null)
                {
                    player.carry = _item;
                }
                else
                {
                    player.dropCarry(true);
                    player.carry = _item;
                }
            }
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