


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{

    [SerializeField]
    private Item _carry;
    public Item carry
    {
        get => _carry;
        set
        {
            _carry = value;
            if (_carry == null) carrySprite.sprite = null;
            else carrySprite.sprite = _carry.sprite;
        }
    }
    [SerializeField]
    private Item _mouseHold;
    public Item mouseHold
    {
        get => _mouseHold;
        set
        {
            _mouseHold = value;
            // if (_mouseHold == null) mouseHoldSprite.sprite = null;
            // else mouseHoldSprite.sprite = _carry.sprite;
        }
    }

    [SerializeField]
    private Weapon _weapon;
    private Ring[] rings = new Ring[MAX_RINGS];
    private const int MAX_RINGS = 6;


    private PlayerController player;
    private SpriteRenderer carrySprite;
    private GameObject weaponPosition;
    private SpriteRenderer weaponSprite;
    public void Start()
    {
        player = GetComponent<PlayerController>();
        carrySprite = transform.Find("Hold").GetComponent<SpriteRenderer>();
        weaponPosition = transform.Find("Sugar_Ant_Sprite").Find("Hands").Find("Weapon").gameObject;
        weaponSprite = weaponPosition.GetComponent<SpriteRenderer>();
        // Refresh sprites
        carry = _carry;
        weapon = _weapon;

    }

    public bool holding(Item item) => carry == item || weapon == item;
    public void remove(Item item)
    {
        if (carry == item) carry = null;
        else if (weapon == item) weapon = null;
    }

    public GameObject itemPrefab;

    public void dropMouse()
    {
        if (mouseHold == null) return;

        GameObject throwItem = Instantiate(itemPrefab, transform.Find("Hold").position, Quaternion.identity);
        throwItem.GetComponent<ItemBehavior>().item = mouseHold;

        throwItem.GetComponent<Rigidbody2D>().velocity = new Vector3(-2.5f * transform.localScale.x, 4, 0);

        mouseHold = null;
    }

    public void dropCarry(bool backwards)
    {
        if (carry == null) return;

        GameObject throwItem = Instantiate(itemPrefab, transform.Find("Hold").position, Quaternion.identity);
        throwItem.GetComponent<ItemBehavior>().item = carry;

        throwItem.GetComponent<Rigidbody2D>().velocity = new Vector3(2.5f * transform.localScale.x * (backwards ? -1 : 1), 4, 0);

        carry = null;
    }
    public void throwCarry(Vector2 direction)
    {
        if (carry == null) return;

        GameObject throwItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        throwItem.GetComponent<ItemBehavior>().item = carry;
        throwItem.GetComponent<Rigidbody2D>().velocity = new Vector3(12f * direction.x, 5f * direction.y, 0);

        carry = null;
    }
    public void dropWeapon(bool backwards)
    {
        if (weapon == null) return;

        GameObject throwItem = Instantiate(itemPrefab, transform.Find("Hold").position, Quaternion.identity);
        throwItem.GetComponent<ItemBehavior>().item = weapon;

        throwItem.GetComponent<Rigidbody2D>().velocity = new Vector3(2.5f * transform.localScale.x * (backwards ? -1 : 1), 4, 0);

        weapon = null;
    }

    public Weapon weapon
    {
        get => _weapon;
        set
        {
            _weapon = value;
            if (_weapon == null) weaponSprite.sprite = null;
            else weaponSprite.sprite = _weapon.sprite;
            updateWeaponStats();
        }
    }

    public bool RingsFull() => rings.Length >= MAX_RINGS;
    public void SetRing(Ring ring, int index)
    {
        rings[index] = ring;
        updateEquipmentStats();
    }
    public Ring GetRing(int index) => rings[index];


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

    private void updateWeaponStats()
    {
        updateEquipmentStats();
        if (weapon == null) return;

        weaponPosition.transform.localPosition = weapon.holdPosition;

        if (weapon.weaponType == Weapon.WeaponType.Melee)
        {
            player.attacks[1].damage = weapon.baseDamage;
            player.attacks[1].splash = weapon.splash;
        }
        else if (weapon.weaponType == Weapon.WeaponType.Ranged)
        {
            player.projectiles[0] = weapon.Projectile;
        }
    }

    // Generated whenever needed
    private List<Equipment> equipment
    {
        get
        {
            List<Equipment> equipmentList = new List<Equipment>();
            if (_weapon != null) equipmentList.Add(_weapon);
            equipmentList.AddRange(rings);
            return equipmentList.Where(i => i != null).ToList();
        }
    }

}