

using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{

    private Inventory player;
    public GameObject statWindow;
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

    public void CloseStatWindow()
    {
        statWindow.SetActive(false);
    }

    public void UpdateStatWindow(InventorySlot inventorySlot)
    {
        if (inventorySlot == null)
        {
            statWindow.SetActive(false);
            return;
        }

        Item item = inventorySlot.GetItem();
        if (item == null)
        {
            statWindow.SetActive(false);
            return;
        }

        if (item.GetType() == typeof(Equipment) || item.GetType() == typeof(Weapon) || item.GetType() == typeof(Ring))
        {
            statWindow.SetActive(true);
            Equipment itemData = (Equipment)item;

            //Equipment itemData = (Equipment)GetComponent<ItemBehavior>().item;
            TMP_Text name = statWindow.transform.Find("Name").GetComponent<TMP_Text>();
            TMP_Text rarity = statWindow.transform.Find("Rarity").GetComponent<TMP_Text>();
            TMP_Text modifiers = statWindow.transform.Find("Modifiers").GetComponent<TMP_Text>();

            name.text = itemData.name;

            switch (itemData.rarity)
            {
                case Equipment.Rarity.Common: rarity.text = "Common"; rarity.color = Color.gray; break;
                case Equipment.Rarity.Uncommon: rarity.text = "Uncommon"; rarity.color = Color.green; break;
                case Equipment.Rarity.Rare: rarity.text = "Rare"; rarity.color = Color.blue; break;
                case Equipment.Rarity.Mythic: rarity.text = "Mythic"; rarity.color = Color.magenta; break;
                case Equipment.Rarity.Legendary: rarity.text = "Legendary"; rarity.color = new Color(245f / 255f, 135f / 255f, 39f / 255f, 1); break;
                default: break;
            }


            modifiers.text = "";
            modifiers.color = rarity.color;
            foreach (Equipment.Modifiers itemMods in itemData.modifiers)
            {
                switch (itemMods)
                {
                    case Equipment.Modifiers.Blunt: modifiers.text += "Blunt:\n    +Knockback, -Damage \n\n"; break;
                    case Equipment.Modifiers.FeatherWeight: modifiers.text += "Feather Weight:\n    +Jump Height, -Defense \n\n"; break;
                    case Equipment.Modifiers.GlassCanon: modifiers.text += "Glass Cannon:\n    +Damage, -Max Health \n\n"; break;
                    case Equipment.Modifiers.Multishot: modifiers.text += "Multishot:\n    +Projectiles, -Damage \n\n"; break;
                    case Equipment.Modifiers.Piercing: modifiers.text += "Piercing:\n    +Projectile Pierce, -Knockback \n\n"; break;
                    case Equipment.Modifiers.RolyPolySoul: modifiers.text += "Roly Poly Soul:\n    +Roll Speed, +Defense, -Damage \n\n"; break;
                    case Equipment.Modifiers.ScarabBeetleSoul: modifiers.text += "Scarab Beetle Soul:\n    +Damage, +Knockback, -Speed \n\n"; break;
                    case Equipment.Modifiers.Sharp: modifiers.text += "Sharp:\n    +Damage, -Knockback \n\n"; break;
                    case Equipment.Modifiers.SprintersSpikes: modifiers.text += "Sprinters Spikes:\n    +Speed, -Jump Height \n\n"; break;
                    case Equipment.Modifiers.StoneSkin: modifiers.text += "Stone Skin:\n    +Defense, -Speed \n\n"; break;
                    case Equipment.Modifiers.WaspSoul: modifiers.text += "Wasp Soul:\n    +Speed, +Double Jump, -Defense \n\n"; break;
                }
            }
        }
        else
        {
            statWindow.SetActive(false);
            return;
        }
    }
}
