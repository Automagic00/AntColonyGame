using System.Collections.Generic;
using UnityEngine;

public class Trader : Interactable
{
    private Inventory player;
    private Interactive interactive;

    public Item want, give;

    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Inventory>();
        interactive = GetComponent<Interactive>();
    }

    public Color noTradeHighlight, tradeHighlight;

    bool canTrade() => player.holding(want.name);

    void Update()
    {
        if (canTrade())
            interactive.SetOutlineColor(tradeHighlight);
        else
            interactive.SetOutlineColor(noTradeHighlight);

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