using UnityEngine;

public class QueenAnt : Interactor, Dialoguer
{
    public override void interact()
    {
        this.canInteract = false;
        Dialogue.OpenDialogue(this);
    }

    public void dialogueFinished()
    {
        if (Globals.gameProgression < 2)
        {
            Globals.gameProgression++;
            this.canInteract = Globals.gameProgression < 2;
        }
    }

    public string getDialogueContent(int i)
    {
        switch (Globals.gameProgression)
        {
            case 0:
                switch (i)
                {
                    case 0: return "Hello!";
                    case 1: return "I make house now";
                }
                break;
            case 1:
                switch (i)
                {
                    case 0: return "U want more house?";
                    case 1: return "Here you go :)";
                }
                break;
        }
        return "not implemented";
    }
    public Sprite getPicture(int i)
    {
        return GetComponent<SpriteRenderer>().sprite;
    }

    public int getDialogueCount()
    {
        return 2;
    }

    public string getName() => "Queen Tyr";


}