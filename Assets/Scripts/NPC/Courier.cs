using System.Collections.Generic;
using Unity.Loading;
using Unity.VisualScripting;
using UnityEngine;

public class Courier : Interactable
{
    private Inventory player;

    private GameObject interactionBox;
    private Transform contents;

    public Item want;

    public static List<Item> courierBag = new List<Item>();


    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Inventory>();

        interactionBox = transform.Find("TradeBox").gameObject;
        contents = interactionBox.transform.Find("contents");
    }


    bool canGive() => player.carry != null;

    void Update()
    {
        canInteract = canGive();
        // Face player

        Vector3 scale = transform.localScale;
        int direction = player.transform.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(direction * Mathf.Abs(scale.x), scale.y, scale.z);
        contents.localScale = new Vector3(direction * Mathf.Abs(contents.localScale.x), contents.localScale.y, contents.localScale.z);


    }

    public override void enterInteractionRange()
    {
        // contents.transform.Find("want").GetComponent<SpriteRenderer>().sprite = want.sprite;
        // interactionBox.SetActive(true);
    }
    public override void exitInteractionRange()
    {
        interactionBox.SetActive(false);
    }

    public override void interact()
    {
        if (!canGive()) return;

        courierBag.Add(player.carry);
        player.carry = null;

        exitInteractionRange();
    }
}