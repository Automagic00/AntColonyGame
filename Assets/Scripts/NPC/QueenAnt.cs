using System;
using System.Collections.Generic;
using UnityEngine;

public class QueenAnt : Interactable, Dialoguer
{
    private Inventory player;
    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Inventory>();
    }

    public override void interact()
    {
        this.canInteract = false;
        Dialogue.OpenDialogue(this);
    }

    public List<DialogueItem> getDialogue()
    {
        switch (Globals.gameProgression)
        {
            case 0: // TUTORIAL part 0: Intro
                return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=GetComponent<SpriteRenderer>().sprite},
                new DialogueItem(){text="Oh loyal follower, I have most unfortunate news..."},
                new DialogueItem(){text="The cursed wasps have destroyed everything!"},
                new DialogueItem(){text="Our nest, our empire... all gone."},
                new DialogueItem(){text="...", action=()=>{GetComponent<SpriteRenderer>().flipX=false;}},
                new DialogueItem(){text="It seems we've no choice but to build it all back from scratch.",
                                    action=()=>{GetComponent<SpriteRenderer>().flipX=true;}},
                new DialogueItem(){text="This land seems suitable.\nI am in need of materials."},
                new DialogueItem(){text="Go east and find me a <b>leaf</b>. We may make something of this yet."},
                new DialogueItem(){action=()=>{
                    Globals.gameProgression++;
                    canInteract = true;
                }},
            };
            case 1: // TUTORIAL part 1: Bring a leaf
                if (!player.holding("leaf"))
                    return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=GetComponent<SpriteRenderer>().sprite},
                new DialogueItem(){text="Go henceforth, and bring me a <b>leaf</b>!"},
                new DialogueItem(){action=()=>{
                    canInteract = true;
                }}};

                return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=GetComponent<SpriteRenderer>().sprite},
                new DialogueItem(){action=()=> {player.carry = null;}},
                new DialogueItem(){action=()=>{Globals.gameProgression++;}},
                new DialogueItem(){text="Excellent... Our nest's restoration is under way."},
                new DialogueItem(){text="Though this new home is lacking,\nsome of my followers may return."},
                new DialogueItem(){text="Next, I require a <b>twig</b>!"},
                new DialogueItem(){action=()=>{canInteract = true;}},
            };
            case 2: // TUTORIAL part 2: Bring a twig (traded)
                if (!player.holding("twig"))
                    return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=GetComponent<SpriteRenderer>().sprite},
                new DialogueItem(){text="To continue construction, I require a <b>twig</b>!"},
                new DialogueItem(){text="If you seem stuck, find some of my loyal followers. They may be willing to trade with you."},
                new DialogueItem(){action=()=>{
                    canInteract = true;
                }}};

                return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=GetComponent<SpriteRenderer>().sprite},
                new DialogueItem(){action=()=> {player.carry = null;}},
                new DialogueItem(){action=()=>{Globals.gameProgression++;}},
                new DialogueItem(){text="TODO keep going!"},
                new DialogueItem(){action=()=>{canInteract = true;}},
            };

        }
        Debug.LogError("Game progression missing dialogue: " + Globals.gameProgression);
        return new List<DialogueItem>();
    }


}