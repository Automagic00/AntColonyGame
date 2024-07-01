using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnthillNurse : Interactable, Dialoguer
{

    private Inventory player;
    public Sprite dialogueIcon;
    public Item[] questItems;
    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Inventory>();
    }
    public List<DialogueItem> getDialogue()
    {
        if (Globals.nurseEnabled)
            return new List<DialogueItem>(){
                new DialogueItem(){name="Nurse", picture=dialogueIcon},
                new DialogueItem(){text="If we meet again in the meadows, I'll be happy to assist!"},
                new DialogueItem(){text="You might need to provide your own bandages, though."},
            };

        if (questItems.Contains(player.carry))
        {
            bool likesWeapon = player.carry.name.ToLower() == "needle";
            return new List<DialogueItem>(){
                new DialogueItem(){name="Nurse", picture=dialogueIcon},
                new DialogueItem(){action=()=>{
                    player.carry = null;
                    Globals.nurseEnabled = true;
                    }},
                new DialogueItem(){text=
                    likesWeapon? "Excellent choice! I'll be able to\ndefend myself perfectly with this."
                    : "Not my first choice of weapon, but\nI suppose this will make do..."},
                new DialogueItem(){text="If we meet again in the meadows, I'll be happy to assist!"},
                new DialogueItem(){text="You might need to provide your own bandages, though."},
            };
        }
        else
            return new List<DialogueItem>(){
                new DialogueItem(){name="Nurse", picture=dialogueIcon},
                new DialogueItem(){text="Thank the queen for this lovely shelter!"},
                new DialogueItem(){text="I would assist her more by exploring, but\nmy bite isn't what it used to be..."},
                new DialogueItem(){text="Say, could you find me a weapon?"},
                new DialogueItem(){text="It'd be nice to be able to see you in the field!"},
            };
    }

    public override void interact()
    {
        Dialogue.OpenDialogue(this);
    }

}
