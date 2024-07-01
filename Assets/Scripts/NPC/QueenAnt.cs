using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QueenAnt : Interactable, Dialoguer
{
    private Inventory player;

    // Tutorial
    public Item want1, want2;
    // Post-tutorial
    public Item want3, want4, want5, want6;

    public static List<Item> queenWants;

    public Sprite dialogueIcon;

    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<Inventory>();

        queenWants = new List<Item>();
        queenWants.AddRange(new Item[] { want1, want2, want3, want4, want5, want6 });
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
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){action=()=>{Globals.gameProgression++;}},
                new DialogueItem(){text="Oh loyal follower, I have most unfortunate news..."},
                new DialogueItem(){text="The cursed wasps have destroyed everything!"},
                new DialogueItem(){text="Our nest, our empire... all gone."},
                new DialogueItem(){text="...", action=()=>{GetComponent<SpriteRenderer>().flipX=false;}},
                new DialogueItem(){text="It seems we've no choice but to build it all back from scratch.",
                                    action=()=>{GetComponent<SpriteRenderer>().flipX=true;}},
                new DialogueItem(){text="This land seems suitable.\nI am in need of materials."},
                new DialogueItem(){text="Go east and find me a "+want1.name+". We may make something of this yet."},
                new DialogueItem(){action=()=>{canInteract = true;}}};

            case 1: // TUTORIAL part 1: Bring a leaf
                if (!player.holding(want1))
                    return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){text="Go henceforth, and bring me a "+want1.name+"!"},
                new DialogueItem(){action=()=>{canInteract = true;}}};

                return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){action=()=> {player.remove(want1);}},
                new DialogueItem(){action=()=>{Globals.gameProgression++;}},
                new DialogueItem(){text="Excellent... Our nest's restoration is under way."},
                new DialogueItem(){text="Though this new home is lacking,\nsome of my followers may return."},
                new DialogueItem(){text="Next, I require a "+want2.name+"!"},
                new DialogueItem(){action=()=>{canInteract = true;}}};

            case 2: // TUTORIAL part 2: Bring a twig (traded)
                if (!player.holding(want2))
                    return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){text="To continue construction, I require a "+want2.name+"!"},
                new DialogueItem(){text="If you seem stuck, find some of my loyal followers. They may be willing to trade with you."},
                new DialogueItem(){action=()=>{canInteract = true;}}};

                return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){action=()=> {player.remove(want2);}},
                new DialogueItem(){action=()=>{Globals.gameProgression++;}},
                new DialogueItem(){text="Excellent work!"},
                new DialogueItem(){text="I shall signal this success to the followers."},
                new DialogueItem(){text="Meanwhile, search the Grassy Fields for more supplies!"},
                new DialogueItem(){text="Be warned though- the bugs there may be hostile to us.\n(press Z to attack)"},
                new DialogueItem(){action=()=>{canInteract = true;}}};
            case 3:
                // TUTORIAL part 3: Go to grassy fields now
                if (SceneManager.GetActiveScene().name == "Tutorial")
                    return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){text="Search the Grassy Fields for more supplies!"},
                new DialogueItem(){text="Be warned though- the bugs there may be hostile to us.\n(press Z to attack)"},
                new DialogueItem(){action=()=>{canInteract = true;}}};

                // Returned from grassy fields
                if (!player.holding(want3))
                    return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){text="Next, I require a "+want3.name+"."},
                new DialogueItem(){action=()=>{canInteract = true;}}};

                return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){action=()=>{Globals.gameProgression++;}},
                new DialogueItem(){action=()=> {player.remove(want3);}},
                new DialogueItem(){text="Thank you for the "+want3.name+"!"},
                new DialogueItem(){text="Next, go fetch me a "+want4.name+"."},
                new DialogueItem(){action=()=>{canInteract = true;}}};
            case 4:
                if (!player.holding(want4))
                    return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){text="Next, I require a "+want4.name+"."},
                new DialogueItem(){action=()=>{canInteract = true;}}};

                return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){action=()=>{Globals.gameProgression++;}},
                new DialogueItem(){action=()=> {player.remove(want4);}},
                new DialogueItem(){text="Thank you for the "+want4.name+"!"},
                new DialogueItem(){text="Next, go fetch me a "+want5.name+"."},
                new DialogueItem(){action=()=>{canInteract = true;}}};
            case 5:
                if (!player.holding(want5))
                    return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){text="Next, I require a "+want5.name+"."},
                new DialogueItem(){action=()=>{canInteract = true;}}};

                return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){action=()=>{Globals.gameProgression++;}},
                new DialogueItem(){action=()=> {player.remove(want5);}},
                new DialogueItem(){text="Thank you for the "+want5.name+"!"},
                new DialogueItem(){text="Next, go fetch me a "+want6.name+"."},
                new DialogueItem(){action=()=>{canInteract = true;}}};
            case 6:
                if (!player.holding(want6))
                    return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){text="Next, I require a "+want6.name+"."},
                new DialogueItem(){action=()=>{canInteract = true;}}};

                return new List<DialogueItem>(){
                new DialogueItem(){name="Queen Tyr", picture=dialogueIcon},
                new DialogueItem(){action=()=>{Globals.gameProgression++;}},
                new DialogueItem(){action=()=> {player.remove(want6);}},
                new DialogueItem(){text="Thank you for the "+want6.name+"!"},
                new DialogueItem(){text="I'm all done now."},
                new DialogueItem(){action=()=>{canInteract = true;}}};
        }

        Debug.LogError("Game progression missing dialogue: " + Globals.gameProgression);
        return new List<DialogueItem>();
    }


}