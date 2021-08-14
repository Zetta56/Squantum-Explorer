using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterfaceUtils : MonoBehaviour
{
    private PlayerController player;
    private Camera cam;
    private GameObject ship;
    private ShipController shipController;
    private Transform nearestAsteroid;
    private GameObject pause;
    private Slider durability;
    private Slider scrap;
    private TextMeshProUGUI crosshair;
    private TextMeshProUGUI score;
    private TextMeshProUGUI comboText;
    private Image comboImage;
    private Image arrow;
    private RectTransform arrowRect;
    private Coroutine comboTimer;
    private Color orange = new Color(245f, 150f, 40f);
    public float comboDuration = 4f;
    private float previousCombo = 0f;
    private int combo = 0;
    private float arrowMargin = 50f;
    private float scoreValue = 0f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        cam = GameObject.Find("Player/Main Camera").GetComponent<Camera>();
        ship = GameObject.Find("Spaceship");
        shipController = ship.GetComponent<ShipController>();
        pause = GameObject.Find("UI/Pause");
        durability = transform.Find("Ship Health").GetComponent<Slider>();
        scrap = transform.Find("Scrap").GetComponent<Slider>();
        score = transform.Find("Score").GetComponent<TextMeshProUGUI>();
        crosshair = transform.Find("Crosshair").GetComponent<TextMeshProUGUI>();
        comboImage = transform.Find("Combo").GetComponent<Image>();
        comboText = transform.Find("Combo/ComboText").GetComponent<TextMeshProUGUI>();
        arrow = transform.Find("Arrow").GetComponent<Image>();
        arrowRect = transform.Find("Arrow").GetComponent<RectTransform>();

        // Turning off situational UI elements
        pause.SetActive(false);
        comboImage.enabled = false;
        comboText.enabled = false;
        arrow.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Bars
        durability.value = shipController.GetHealth() / shipController.maxHealth;
        scrap.value = player.scrap/player.maxScrap;

        // Crosshair
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, player.sliceDistance, 1 << 6)) {
            crosshair.color = orange;
            Debug.Log(crosshair.color);
        } else if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward)) {
            crosshair.color = Color.red;
        } else {
            crosshair.color = Color.white;
        }

        // Combo
        if(Time.time - previousCombo > comboDuration){
            combo = 1;
        }

        // Others
        score.text = "Score: " + scoreValue.ToString();
        UpdateArrow();
    }

    void UpdateArrow() {
        nearestAsteroid = null;
        float closest = Mathf.Infinity;
        foreach(Transform asteroid in GameObject.Find("Asteroid Spawner").transform) {
            // Taking square magnitude to avoid square rooting (an expensive operation)
            float distance = (asteroid.position - ship.transform.position).sqrMagnitude;
            if(distance < closest) {
                closest = distance;
                nearestAsteroid = asteroid;
            }
        }
        if(nearestAsteroid) {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(nearestAsteroid.position);
            // If sceenPos is onscreen, disable arrow
            if(screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height && screenPos.z >= 0) {
                arrow.enabled = false;
                return;
            }
            // If screenPos is offscreen and behind camera, undo its reflections caused by being behind the camera
            if(screenPos.z < 0) {
                // Fix x by locking it to opposite side of screen
                if(screenPos.x <= Screen.width / 2) {
                    screenPos.x = Screen.width - arrowMargin;
                } else {
                    screenPos.x = arrowMargin;
                }
                // Fix y by reflecting it over the line: y = screenCenter
                screenPos.y += (Screen.height / 2 - screenPos.y) * 2;
            }
            // If screenPos is offscreen, limit screenPos to screen edges
            screenPos.x = Mathf.Clamp(screenPos.x, arrowMargin, Screen.width - arrowMargin);
            screenPos.y = Mathf.Clamp(screenPos.y, arrowMargin, Screen.height - arrowMargin);
            arrowRect.position = screenPos;
            // Change arrow angle to match position on screen
            float angle = Mathf.Atan2(screenPos.y - Screen.height / 2, screenPos.x - Screen.width / 2) * 180 / Mathf.PI;
            arrowRect.eulerAngles = new Vector3(0, 0, angle - 90);
            arrow.enabled = true;
        } else {
            arrow.enabled = false;
        }
    }

    public float GetScore() {
        return scoreValue;
    }

    public bool GetPauseEnabled() {
        return pause.activeSelf;
    }

    public int GetCombo() {
        return combo;
    }

    public void IncrementScore(float increment) {
        scoreValue += increment * combo;
    }

    public void TogglePause() {
        pause.SetActive(!pause.activeSelf);
        if (pause.activeSelf) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
        }
    }



    public void IncrementCombo(){
        if(Time.time - previousCombo <= comboDuration){
            combo++;
            comboText.text = "x" + combo;
            comboText.enabled = true;
            comboImage.enabled = true;
            // Start combo timer
            if (comboTimer != null) {
                StopCoroutine(comboTimer);
            }
            comboTimer = StartCoroutine(HideCombo());
        }
        previousCombo = Time.time;
    }

    IEnumerator HideCombo() {
        yield return new WaitForSeconds(4f);
        comboText.enabled = false;
        comboImage.enabled = false;
    }
}
