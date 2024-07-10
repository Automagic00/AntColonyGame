using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Entity player;
    //private Slider hpSlider;
    private float healthPerPiece = 10;
    private float pieceWidth = 30;
    public GameObject borderFolder;
    public GameObject healthbarFolder;
    public GameObject borderEnd;
    public GameObject healthbarEnd;
    public RectMask2D mask;
    public Sprite[] borderSprites;
    public Sprite[] healthbarSprites;
    private int lastUsedSpriteIndex = -1;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Entity>();
        //hpSlider = GetComponent<Slider>();
        mask = healthbarFolder.GetComponent<RectMask2D>();
    }

    void Update()
    {
        int pieces = Mathf.RoundToInt(player.maxHP / healthPerPiece);

        if (pieces <= 2)
        {
            pieces = 2;

            //Remove old midsections if they exist
            if (borderFolder.transform.childCount != pieces)
            {
                foreach (Transform child in borderFolder.transform)
                {
                    if (child.gameObject.name != "Border_Start" && child.gameObject.name != "Border_End")
                    {
                        Destroy(child.gameObject);
                    }
                }
                foreach (Transform child in healthbarFolder.transform)
                {
                    if (child.gameObject.name != "Health_Start" && child.gameObject.name != "Health_End")
                    {
                        Destroy(child.gameObject);
                    }
                }

                //Adjust End Piece
                borderEnd.transform.localPosition = new Vector3(-15 + (pieceWidth * (pieces - 1)), 0, 0);
                healthbarEnd.transform.localPosition = new Vector3(-15 + (pieceWidth * (pieces - 1)), 0, 0);
            }
        }

        if (pieces > 2)
        {
            if (borderFolder.transform.childCount != pieces)
            {
                //Remove old midsections
                foreach (Transform child in borderFolder.transform)
                {
                    if (child.gameObject.name != "Border_Start" && child.gameObject.name != "Border_End")
                    {
                        Destroy(child.gameObject);
                    }
                }
                foreach (Transform child in healthbarFolder.transform)
                {
                    if (child.gameObject.name != "Health_Start" && child.gameObject.name != "Health_End")
                    {
                        Destroy(child.gameObject);
                    }
                }

                //Add new midsections
                for (int i = 1; i < pieces - 1; i++)
                {
                    //Set Piece Transforms
                    GameObject borderPiece = new GameObject("BorderPiece_" + i);
                    borderPiece.transform.SetParent(borderFolder.transform);
                    borderPiece.transform.localPosition = new Vector3(-15 + (pieceWidth * i), 0, 0);

                    GameObject healthPiece = new GameObject("HealthPiece_" + i);
                    healthPiece.transform.SetParent(healthbarFolder.transform);
                    healthPiece.transform.localPosition = new Vector3(-15 + (pieceWidth * i), 0, 0);


                    //Set Image
                    Image bpImage = borderPiece.AddComponent<Image>();
                    Image hpImage = healthPiece.AddComponent<Image>();
                    if (lastUsedSpriteIndex >= borderSprites.Length - 1)
                    {
                        lastUsedSpriteIndex = 0;
                    }
                    else
                    {
                        lastUsedSpriteIndex++;
                    }
                    bpImage.sprite = borderSprites[lastUsedSpriteIndex];
                    hpImage.sprite = healthbarSprites[lastUsedSpriteIndex];

                    //fix scale
                    float xScalar = Screen.width / 1920f;
                    float yScalar = Screen.height / 1080f;
                    borderPiece.GetComponent<RectTransform>().sizeDelta = new Vector2(30 * xScalar, 70 * yScalar);
                    healthPiece.GetComponent<RectTransform>().sizeDelta = new Vector2(30 * xScalar, 70 * yScalar);

                }
                //Adjust End Piece
                borderEnd.transform.localPosition = new Vector3(-15 + (pieceWidth * (pieces - 1)), 0, 0);
                healthbarEnd.transform.localPosition = new Vector3(-15 + (pieceWidth * (pieces - 1)), 0, 0);
            }
        }

        float hpPercent = player.currentHealth / player.maxHP;

        //mask.GetComponent<RectTransform>().sizeDelta = new Vector2(pieces * pieceWidth,70);
        mask.GetComponent<RectTransform>().offsetMin = new Vector2((pieces - 2) * -pieceWidth, 0);
        mask.GetComponent<RectTransform>().offsetMax = new Vector2((pieces - 2) * pieceWidth, 0);
        mask.padding = new Vector4(0, 0, (1 - hpPercent) * (pieces * pieceWidth), 0);
        //hpSlider.value = player.currentHealth / player.maxHP;
    }
}
