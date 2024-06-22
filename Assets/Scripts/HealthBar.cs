using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Entity player;
    private Slider hpSlider;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Entity>();
        hpSlider = GetComponent<Slider>();
    }

    void Update()
    {
        hpSlider.value = player.currentHealth / player.maxHP;
    }
}
