using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class InterfaceUtils : MonoBehaviour
{
    public TextMeshProUGUI crosshair;
    public Image magnet;
    private PlayerController player;
    private Camera cam;
    private GameObject ship;
    private ShipController shipController;
    private Transform shipTransform;
    private Slider fuel, durability, scrap;
    private TextMeshProUGUI score;
    private Image indicator;
    private Image comboImage;
    private TextMeshProUGUI combo;
    private Transform target;
    private Image arrow;
    private RectTransform arrowRect;
    private Coroutine comboCoroutine;
    private float arrowMargin = 50f;
    private float scoreValue = 0f;

    // Start is called before the first frame update
    void Start()
    {
        fuel = transform.Find("Fuel").GetComponent<Slider>();
        durability = transform.Find("Ship Health").GetComponent<Slider>();
        scrap = transform.Find("Scrap").GetComponent<Slider>();
        score = transform.Find("Score").GetComponent<TextMeshProUGUI>();
        crosshair = transform.Find("Crosshair").GetComponent<TextMeshProUGUI>();
        indicator = transform.Find("Indicator").GetComponent<Image>();
        magnet = transform.Find("Magnet").GetComponent<Image>();
        comboImage = transform.Find("Combo").GetComponent<Image>();
        combo = transform.Find("Combo/ComboText").GetComponent<TextMeshProUGUI>();
        arrow = transform.Find("Arrow").GetComponent<Image>();
        arrowRect = transform.Find("Arrow").GetComponent<RectTransform>();

        magnet.enabled = false;
        comboImage.enabled = false;
        combo.enabled = false;
        arrow.enabled = false;

        player = GameObject.Find("Player").GetComponent<PlayerController>();
        cam = GameObject.Find("Player/Main Camera").GetComponent<Camera>();
        ship = GameObject.Find("Spaceship");
        shipController = ship.GetComponent<ShipController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Bars
        fuel.value = player.fuel/player.maxFuel;
        durability.value = shipController.health/shipController.maxHealth;
        scrap.value = player.scrap/player.maxScrap;

        // Red Outline
        Color temp = indicator.color;
        temp.a = (player.indicatorAlpha/100)*0.4f;
        indicator.color = temp;

        // Others
        score.text = "Score: " + scoreValue.ToString();
        UpdateArrow();
    }

    void UpdateArrow() {
        target = null;
        float closest = Mathf.Infinity;
        foreach(Transform asteroid in GameObject.Find("Asteroid Spawner").transform) {
            // Taking square magnitude to avoid square rooting (an expensive operation)
            float distance = (asteroid.position - ship.transform.position).sqrMagnitude;
            if(distance < closest) {
                closest = distance;
                target = asteroid;
            }
        }
        if(target) {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(target.position);
            // If sceenPos is onscreen, disable arrow
            if(screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height && screenPos.z >= 0) {
                arrow.enabled = false;
                return;
            }
            // If screenPos is offscreen and behind camera, undo its reflections (caused by being behind camera)
            if(screenPos.z < 0) {
                // Lock x to opposite side of screen
                if(screenPos.x <= Screen.width / 2) {
                    screenPos.x = Screen.width - arrowMargin;
                } else {
                    screenPos.x = arrowMargin;
                }
                // Reflect y over the line: y = screenCenter
                screenPos.y += (Screen.height / 2 - screenPos.y) * 2;
            }
            // If screenPos is offscreen, limit screenPos to screen edges
            if(screenPos.x < arrowMargin) {
                screenPos.x = arrowMargin;
            }
            if(screenPos.x > Screen.width - arrowMargin) {
                screenPos.x = Screen.width - arrowMargin;
            }
            if(screenPos.y < arrowMargin) {
                screenPos.y = arrowMargin;
            }
            if(screenPos.y > Screen.height - arrowMargin) {
                screenPos.y = Screen.height - arrowMargin;
            }
            float angle = Mathf.Atan2(screenPos.y - Screen.height / 2, screenPos.x - Screen.width / 2) * 180 / Mathf.PI;
            arrow.enabled = true;
            arrowRect.position = screenPos;
            arrowRect.eulerAngles = new Vector3(0, 0, angle - 90);
        } else {
            arrow.enabled = false;
        }
    }

    public float GetScore() {
        return scoreValue;
    }

    public void IncrementScore(float increment) {
        scoreValue += increment*player.combo;
    }

    public void ColorCrosshair(Color color) {
        crosshair.color = color;
    }

    public void ShowCombo(){
        comboImage.enabled = true;
        combo.text = "x" + player.combo;
        combo.enabled = true;
        if (comboCoroutine != null) {
            StopCoroutine(comboCoroutine);
        }
        comboCoroutine = StartCoroutine(HideCombo());
    }

    IEnumerator HideCombo() {
        yield return new WaitForSeconds(4f);
        comboImage.enabled = false;
        combo.enabled = false;
    }
}
