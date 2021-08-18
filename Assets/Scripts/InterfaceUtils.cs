using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterfaceUtils : MonoBehaviour
{
    // References
    public AudioClip pickup;
    public AudioClip reject;
    private PlayerController player;
    private Camera cam;
    private GameObject ship;
    private ShipController shipController;
    private Transform nearestAsteroid;
    private Color purple = new Color(0.4f, 0f, 1f);
    private Coroutine comboTimer;

    // UI Elements
    private GameObject pause;
    private Slider healthBar;
    private Slider scrapBar;
    private TextMeshProUGUI crosshair;
    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI comboText;
    private Image comboImage;
    private Image arrow;
    private RectTransform arrowRect;

    // Logic
    public float maxScrap = 20f;
    public float comboDuration = 4f;
    [HideInInspector] public int combo = 1;
    [HideInInspector] public float score = 0f;
    [HideInInspector] public float scrap = 0;
    private float arrowMargin = 50f;
    private bool maxScrapAlarmed = false;
    private float previousCombo = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        // Getting references
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        cam = GameObject.Find("Player/Main Camera").GetComponent<Camera>();
        ship = GameObject.Find("Spaceship");
        shipController = ship.GetComponent<ShipController>();
        pause = GameObject.Find("UI/Pause");
        healthBar = transform.Find("Health Bar").GetComponent<Slider>();
        scrapBar = transform.Find("Scrap Bar").GetComponent<Slider>();
        scoreText = transform.Find("Score").GetComponent<TextMeshProUGUI>();
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
        healthBar.value = shipController.GetHealth() / shipController.maxHealth;
        scrapBar.value = scrap / maxScrap;

        // Crosshair
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, player.sliceDistance, 1 << 6)) {
            crosshair.color = purple;
        } else if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward)) {
            crosshair.color = Color.red;
        } else {
            crosshair.color = Color.white;
        }

        // Others
        scoreText.text = "Score: " + score.ToString();
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

    public bool IsPaused() {
        return pause.activeSelf;
    }

    public void IncrementScore(float amount) {
        score += amount * combo;
    }

    public void IncrementScrap(int amount) {
        if(scrap < maxScrap && !GameManager.Instance.frozen){
            float newScrap = scrap + amount;
            scrap = newScrap > maxScrap ? maxScrap : newScrap;
            maxScrapAlarmed = false;
            player.PlaySound(pickup);
        } else if(!maxScrapAlarmed) {
            maxScrapAlarmed = true;
            player.PlaySound(reject);
        }
    }

    public void IncrementCombo(){
        if(Time.time - previousCombo <= comboDuration){
            combo++;
            comboText.text = "x" + combo;
            comboText.enabled = true;
            comboImage.enabled = true;
            TickCombo();
        }
        previousCombo = Time.time;
    }

    // Separated from IncrementCombo() for more usability in other scripts
    public void TickCombo() {
        if (comboTimer != null) {
            StopCoroutine(comboTimer);
        }
        comboTimer = StartCoroutine(HideCombo());
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

    public IEnumerator HideCombo() {
        yield return new WaitForSeconds(comboDuration);
        if(!GameManager.Instance.frozen) {
            comboText.enabled = false;
            comboImage.enabled = false;
            combo = 1;
        }
    }
}
