
using UnityEngine;

public class ItemBehavior : Interactable
{
    public Item item;


    public virtual void Start()
    {
        GetComponent<SpriteRenderer>().sprite = item.sprite;
    }


    // Pick up on interact
    public override void interact()
    {
        Inventory player = GameObject.Find("Player").GetComponent<Inventory>();

        if (player.carry != null)
            player.dropCarry(true);

        player.carry = item;
        Destroy(this.gameObject);
    }
}