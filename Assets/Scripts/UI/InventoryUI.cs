

using UnityEngine;

public class InventoryUI : MonoBehaviour
{

    private Inventory player;
    void Start()
    {
        player = FindAnyObjectByType<Inventory>();
    }
    public void Pause()
    {
        if (player == null) Start();

        // Place inventory UI on player
        Vector3 screenPos = Camera.main.WorldToScreenPoint(player.transform.position);
        screenPos.z = transform.position.z;
        transform.position = screenPos;

        setFlipped(player.transform.localScale.x == -1);
    }

    public void Unpause()
    {
        player.dropMouse();
    }


    // Flip layout, but not children (double the flip to cancel)
    private void setFlipped(bool flipped)
    {
        Vector3 scale = new Vector3(flipped ? -1 : 1, 1, 1);

        transform.localScale = scale;
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).transform.localScale = scale;

    }
}
