using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnthillCourier : Interactable, Dialoguer
{

    private Inventory player;
    public Sprite dialogueIcon;
    public Item[] questItems;
    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Inventory>();
    }
    void emptyBag()
    {
        // TODO stagger toss
        foreach (Item toss in Courier.courierBag)
        {
            GameObject giveItem = Instantiate(player.itemPrefab, transform.position, Quaternion.identity);
            giveItem.GetComponent<ItemBehavior>().item = toss;

            float direction = player.transform.position.x > transform.position.x ? 1 : -1;
            giveItem.GetComponent<Rigidbody2D>().velocity = new Vector3(Random.Range(1f, 3f) * direction, Random.Range(1f, 3f), 0);
        }

        Courier.courierBag.Clear();
    }
    public List<DialogueItem> getDialogue()
    {
        if (Globals.courierEnabled)
        {
            int itemCount = Courier.courierBag.Count;
            if (itemCount > 0)
                return new List<DialogueItem>(){
                new DialogueItem(){name="Courier", picture=dialogueIcon},
                new DialogueItem(){action=emptyBag},
                new DialogueItem(){text="I've got " + itemCount + "item"+(itemCount==1?"":"s") + " for you today."},
                // TODO storage
                new DialogueItem(){text="Somewhere to store that sure would be nice."},
            };

            return new List<DialogueItem>(){
                new DialogueItem(){name="Courier", picture=dialogueIcon},
                new DialogueItem(){text="I don't have any mail for you today."},
            };
        }

        if (questItems.Contains(player.carry))
        {
            return new List<DialogueItem>(){
                new DialogueItem(){name="Courier", picture=dialogueIcon},
                new DialogueItem(){action=()=>{
                    player.carry = null;
                    Globals.courierEnabled = true;
                    }},
                new DialogueItem(){text="Ah yes... this will do nicely!"},
                new DialogueItem(){text="To be completely honest, I just expected a bottlecap."},
                new DialogueItem(){text="I'll make it worth your while, though.\nAny item you find in the fields, I can ferry back."},
            };
        }
        else
            return new List<DialogueItem>(){
                new DialogueItem(){name="Courier", picture=dialogueIcon},
                new DialogueItem(){text="This habitat seems suitable for my business."},
                new DialogueItem(){text="Oh, me? I've been moving ant mail for over 50 minutes now."},
                new DialogueItem(){text="If you want to hire my services,\nyou'll have to pay in advance."},
                new DialogueItem(){text="Something... valuable. I'll know it when I see it."},
            };
    }

    public override void interact()
    {
        Dialogue.OpenDialogue(this);
    }

}
