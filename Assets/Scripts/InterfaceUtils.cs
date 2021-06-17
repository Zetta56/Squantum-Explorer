using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class InterfaceUtils : MonoBehaviour
{
    private PlayerController player;
    private ShipController ship;
    private Slider fuel, durability, scrap;
    public TextMeshProUGUI crosshair;
    public Image magnet;
    private TextMeshProUGUI score;
    private Image Indicator;
    private float scoreValue = 0f;

    // Start is called before the first frame update
    void Start()
    {
        fuel = transform.Find("Fuel").GetComponent<Slider>();
        durability = transform.Find("Ship Health").GetComponent<Slider>();
        scrap = transform.Find("Scrap").GetComponent<Slider>();
        score = transform.Find("Score").GetComponent<TextMeshProUGUI>();
        crosshair = transform.Find("Crosshair").GetComponent<TextMeshProUGUI>();
        Indicator = transform.Find("Indicator").GetComponent<Image>();
        magnet = transform.Find("Magnet").GetComponent<Image>();
        magnet.enabled = false;

        player = GameObject.Find("Player").GetComponent<PlayerController>();
        ship = GameObject.Find("Spaceship").GetComponent<ShipController>();
    }

    // Update is called once per frame
    void Update()
    {
        fuel.value = player.fuel/player.maxFuel;
        durability.value = ship.health/ship.maxHealth;
        scrap.value = player.scrap/player.maxScrap;

        score.text = "Score: " + scoreValue.ToString();

        Color temp = Indicator.color;
        temp.a = (player.IndicatorAlpha/100)*0.4f;
        Indicator.color = temp;
    }

    public float GetScore() {
        return scoreValue;
    }

    public void IncrementScore(float increment) {
        scoreValue += increment;
    }

    public void ColorCrosshair(Color color) {
        crosshair.color = color;
    }
}
