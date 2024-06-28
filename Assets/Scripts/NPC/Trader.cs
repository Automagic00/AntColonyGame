using System.Collections.Generic;
using UnityEngine;

public class Trader : Interactable
{
    private Inventory player;
    private Interactive interactive;

    private GameObject interactionBox;

    public Item want, give;

    void Awake()
    {

        player = GameObject.Find("Player").GetComponent<Inventory>();
        interactive = GetComponent<Interactive>();
        interactionBox = transform.Find("TradeBox").gameObject;
    }


    bool canTrade() => player.holding(want);

    void Update()
    {
        canInteract = canTrade();
    }

    public override void enterInteractionRange()
    {
        interactionBox.transform.Find("want").GetComponent<SpriteRenderer>().sprite = want.sprite;
        interactionBox.transform.Find("give").GetComponent<SpriteRenderer>().sprite = give.sprite;
        interactionBox.SetActive(true);
    }
    public override void exitInteractionRange()
    {
        interactionBox.SetActive(false);
    }

    public override void interact()
    {
        if (!canTrade()) return;

        player.carry = null;

        GameObject giveItem = Instantiate(player.itemPrefab, transform.position, Quaternion.identity);
        giveItem.GetComponent<ItemBehavior>().item = give;

        float direction = player.transform.position.x > transform.position.x ? 1 : -1;
        giveItem.GetComponent<Rigidbody2D>().velocity = new Vector3(2f * direction, 2, 0);

    }
}