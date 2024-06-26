
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
        if (player.carry != null)
            player.dropCarry(true);

        player.carry = item;
        Destroy(this.gameObject);
    }
}