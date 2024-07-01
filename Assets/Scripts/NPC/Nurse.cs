using System.Collections.Generic;
using Unity.Loading;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Nurse : Interactable
{
    private Inventory player;
    private Entity playerEntity;
    private Interactive interactive;

    private GameObject interactionBox;
    private Transform contents;

    public Item want;

    public AnimatorController[] animators;

    void Awake()
    {
        Animator anim = transform.Find("Sprite").GetComponent<Animator>();
        anim.runtimeAnimatorController = animators[Random.Range(0, animators.Length)];
        player = GameObject.Find("Player").GetComponent<Inventory>();
        playerEntity = player.GetComponent<Entity>();

        interactive = GetComponent<Interactive>();
        interactionBox = transform.Find("TradeBox").gameObject;
        contents = interactionBox.transform.Find("contents");
    }


    bool canTrade() => player.holding(want);

    void Update()
    {
        canInteract = canTrade();
        // Face player

        Vector3 scale = transform.localScale;
        int direction = player.transform.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(direction * Mathf.Abs(scale.x), scale.y, scale.z);
        contents.localScale = new Vector3(direction * Mathf.Abs(contents.localScale.x), contents.localScale.y, contents.localScale.z);


    }

    public override void enterInteractionRange()
    {
        if (playerEntity.currentHealth == playerEntity.maxHP) return;

        contents.transform.Find("want").GetComponent<SpriteRenderer>().sprite = want.sprite;
        interactionBox.SetActive(true);
    }
    public override void exitInteractionRange()
    {
        interactionBox.SetActive(false);
    }

    public override void interact()
    {
        if (!canTrade()) return;

        player.carry = null;
        playerEntity.currentHealth = playerEntity.maxHP;

    }
}