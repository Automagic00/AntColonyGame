


using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    private Item _carry;

    private Weapon _weapon;
    private List<Ring> rings = new List<Ring>(MAX_RINGS);
    private const int MAX_RINGS = 6;


    private PlayerController player;
    public void Start()
    {
        player = GetComponent<PlayerController>();
    }

    public Item carry
    {
        get => _carry;
        set => _carry = value;
    }
    public Weapon weapon
    {
        get => _weapon;
        set
        {
            _weapon = value;
            updateEquipmentStats();
        }
    }

    public bool RingsFull() => rings.Count >= MAX_RINGS;
    public void SetRing(Ring ring, int index)
    {
        rings[index] = ring;
        updateEquipmentStats();
    }


    private void updateEquipmentStats()
    {
        float groundspeedMod = 1;
        float airspeedMod = 1;
        float jumpHeightMod = 1;
        float hpMod = 1;
        int doubleJumpMod = 0;

        // TODO modify hitbox properties
        float damageMod = 1;
        // TODO add other modifiers here as needed

        foreach (Equipment modifier in equipment)
        {
            groundspeedMod *= modifier.groundspeedMod;
            airspeedMod *= modifier.airspeedMod;
            damageMod *= modifier.damageMod;
            jumpHeightMod *= modifier.jumpHeightMod;
            hpMod *= modifier.hpMod;
            doubleJumpMod += modifier.doubleJumpMod;
        }

        player.ModifyStats(groundspeedMod, airspeedMod, jumpHeightMod, doubleJumpMod, hpMod);
    }

    // Generated whenever needed
    private List<Equipment> equipment
    {
        get
        {
            List<Equipment> equipmentList = new List<Equipment>();
            if (_weapon != null) equipmentList.Add(_weapon);
            equipmentList.AddRange(rings);
            return equipmentList;
        }
    }

}