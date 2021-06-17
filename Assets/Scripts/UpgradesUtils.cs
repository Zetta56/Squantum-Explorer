using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradesUtils : MonoBehaviour
{
    public string[] buttonNames = {"Shields", "Grapple", "FuelCap",/* "FuelRegen",*/"MagStrength", "ScrapCap", "ScrapLuck", "Tether", "Heal", "Clear"};
    public string[] upgradeDescriptions = {
        "Increase the Shield capacity for your ship.", "Increase the length and range of you grapple gun.",
        "Increase the Fuel Capacity of your Jetpack.",/* "Increase the speed at which your Jetpack replenishes fuel.",*/"Increase the rate at which your magnet grows while you are returning to your ship",
        "Increase the amount of Scrap you can hold.", "Increase the amount of Scrap you pick up from Asteroids and Floating Pieces.",
        "Increase the length of your tether to your ship",
        "Heal your ship to 100% health.", "Clear all asteroids."
        };
    public int[] prices = {20, 5, 5, /*5,*/ 5, 10, 20, 20, 50, 25};
    public int[] priceIncrement = {10, 5, 5, /*5,*/ 5, 5, 5, 10, 10, 15};
    public AudioClip upgrade;
    public AudioClip reject;

    private PlayerController player;
    private ShipController ship;
    private AsteroidSpawner asteroidSpawner;
    private GameObject breakdown;
    private TextMeshProUGUI scraps;
    private InterfaceUtils interfaceUtils;

    private int currentIndex;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        ship = GameObject.Find("Spaceship").GetComponent<ShipController>();
        asteroidSpawner = GameObject.Find("Asteroid Spawner").GetComponent<AsteroidSpawner>();
        breakdown = GameObject.Find("UI/Upgrades/Breakdown");
        scraps = GameObject.Find("Scraps").GetComponent<TextMeshProUGUI>();
        interfaceUtils = GameObject.Find("UI/Interface").GetComponent<InterfaceUtils>();
        gameObject.SetActive(false);
        ShowDescription("Shields");
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
            BuyCurrent();
        }

        Time.timeScale = 0; // Just in case you pause in upgrades menu
        string name = buttonNames[currentIndex];
        string desc = upgradeDescriptions[currentIndex];
        string price = prices[currentIndex].ToString();
        (string, string) stats = GetStats(name);

        breakdown.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = desc;
        breakdown.transform.Find("Price").GetComponent<TextMeshProUGUI>().text = price;
        breakdown.transform.Find("Status").GetComponent<TextMeshProUGUI>().text = GetLongName(name);
        breakdown.transform.Find("Stats").GetComponent<TextMeshProUGUI>().text = stats.Item1 + ": " + stats.Item2;
        scraps.text = player.scrap.ToString();
    }

    public void ShowDescription(string buttonName) {
        currentIndex = Array.IndexOf(buttonNames, buttonName);
    }

    public void BuyCurrent() {
        if (player.scrap >= prices[currentIndex]) {
            player.PlaySound(upgrade);
            player.scrap -= prices[currentIndex];
            prices[currentIndex] += priceIncrement[currentIndex];
            Invoke(buttonNames[currentIndex], 0f);
        } else {
            player.PlaySound(reject);
        }
    }

    

    string GetLongName(string shortName) {
        switch(shortName) {
            case "FuelCap":
                return "Fuel Capacity";
            /* case "FuelRegen": */
            /*     return "Fuel Regeneration"; */
            case "ScrapCap":
                return "Scrap Capacity";
            case "ScrapLuck":
                return "Scrap Fortune";
            case "Clear":
                return "Clear Asteroids";
            case "MagStrength":
                return "Magnet Strength";
            default:
                return shortName;
        }
    }

    (string, string) GetStats(string name) {
        switch(name) {
            case "Shields":
                return ("Asteroid Damage", $"{(ship.damageRate + Mathf.Log10(1 + interfaceUtils.GetScore() / 20)).ToString(".00")} -10%");
            case "Grapple":
                return ("Length", $"{player.grapplingLength} +10");
            case "FuelCap":
                return ("Capacity", $"{player.maxFuel} +25");
            /* case "FuelRegen": */
            /*     return ("Rate", $"{player.fuelRegen} +1"); */
            case "MagStrength":
                return ("Strength", $"{player.magnetGrowth * 1000} + 5");
            case "ScrapCap":
                return ("Capacity", $"{player.maxScrap} +10");
            case "ScrapLuck":
                return ("Bonus", $"{player.scrapBonus} +1");
            case "Tether":
                return ("Length", $"{player.tetherDistance} +25");
            case "Heal":
                return ("Health", $"{ship.health.ToString("F2")} +{(ship.maxHealth - ship.health).ToString("F2")}");
            case "Clear":
                int asteroids = GameObject.Find("Asteroid Spawner").transform.childCount;
                return ("Num Asteroids", $"{asteroids} -{asteroids}");
            default:
                return ("", "");
        }
    }

    void Shields() {
        ship.damageRate *= .9f;
    }

    void Grapple() {
        player.grapplingLength += 10;
    }

    void FuelCap() {
        player.maxFuel += 25;
    }

    /* void FuelRegen() { */
    /*     player.fuelRegen += 1; */
    /* } */

    void MagStrength() {
        player.magnetGrowth += 0.005f;
    }

    void ScrapCap() {
        player.maxScrap += 10;
    }

    void ScrapLuck() {
        player.scrapBonus += 1;
    }

    void Tether() {
        player.tetherDistance += 25;
    }

    void Heal() {
        ship.health = ship.maxHealth;
    }

    void Clear() {
        foreach(Transform asteroid in GameObject.Find("Asteroid Spawner").transform) {
            GameObject.Destroy(asteroid.gameObject);
        }
    }
}
