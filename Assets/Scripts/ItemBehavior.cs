
using UnityEngine;

public class ItemBehavior : Interactable
{
    public Item item;
    public Inventory player;


    public virtual void Start()
    {
        GetComponent<SpriteRenderer>().sprite = item.sprite;
        player = GameObject.Find("Player").GetComponent<Inventory>();
    }


    // Pick up on interact
    public override void interact()
    {
        if (item.GetType() == typeof(Weapon))
        {
            if (player.weapon != null)
                player.dropWeapon(true);

            player.weapon = (Weapon)item;
            Destroy(this.gameObject);
        }
        else
        {
            if (player.carry != null)
                player.dropCarry(true);

            player.carry = item;
            Destroy(this.gameObject);
        }
    }
}