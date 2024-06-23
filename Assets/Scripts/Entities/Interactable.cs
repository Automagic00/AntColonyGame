using UnityEngine;

public abstract class Interactable : MonoBehaviour
{

    public bool canInteract = true;
    public abstract void interact();

    public void enableInteraction() { }
    public void disableInteraction() { }
}