using UnityEngine;

public abstract class Interactor : MonoBehaviour
{

    public bool canInteract = true;
    public abstract void interact();

    public void enableInteraction() { }
    public void disableInteraction() { }
}