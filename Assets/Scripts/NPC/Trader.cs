using UnityEngine;

public class Trader : Interactable
{
    private Inventory player;

    private GameObject interactionBox;
    private Transform contents;

    public Item want, give;

    public RuntimeAnimatorController[] animators;

    void Awake()
    {
        Animator anim = transform.Find("Sprite").GetComponent<Animator>();
        anim.runtimeAnimatorController = animators[Random.Range(0, animators.Length)];
        player = GameObject.Find("Player").GetComponent<Inventory>();

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
        contents.transform.Find("want").GetComponent<SpriteRenderer>().sprite = want.sprite;
        contents.transform.Find("give").GetComponent<SpriteRenderer>().sprite = give.sprite;
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

        GameObject giveItem = Instantiate(player.itemPrefab, transform.position, Quaternion.identity);
        giveItem.GetComponent<ItemBehavior>().item = give;

        float direction = player.transform.position.x > transform.position.x ? 1 : -1;
        giveItem.GetComponent<Rigidbody2D>().velocity = new Vector3(2f * direction, 2, 0);

    }
}