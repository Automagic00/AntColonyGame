using UnityEngine;

public abstract class Interactable : MonoBehaviour
{

    public bool canInteract = true;
    public abstract void interact();

    public virtual void enterInteractionRange() { }
    public virtual void exitInteractionRange() { }
}