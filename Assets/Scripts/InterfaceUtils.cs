using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterfaceUtils : MonoBehaviour
{
    // References
    [SerializeField] private AudioClip pickup;
    [SerializeField] private AudioClip reject;
    private PlayerController player;
    private Camera cam;
    private GameObject ship;
    private ShipController shipController;
    private Transform nearestAsteroid;
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
    [SerializeField] private float maxScrap = 30f;
    [SerializeField] private float minScrap = 20f;
    [SerializeField] private float comboDuration = 4f;
    private int combo = 1;
    private float score;
    private float scrap;
    private float scrapCap;  // Scrap limit that changes during runtime
    private float arrowMargin = 50f;
    private bool scrapAlarmed = false;
    private float previousCombo = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        // References
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
        scrapCap = maxScrap;
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
        healthBar.value = shipController.GetHealth() / shipController.GetMaxHealth();
        scrapBar.value = scrap / scrapCap;

        // Crosshair
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, player.GetSliceDistance(), 1 << 6)) {
            crosshair.color = GameManager.Instance.purple;
        } else if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward) ||
            Physics.SphereCast(Camera.main.transform.position, player.GetAimAssistSize(),
            Camera.main.transform.forward, out hit, Mathf.Infinity, 1 << 6)
        ) {
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

    public float GetScore() {
        return score;
    }

    public float GetScrapCap() {
        return scrapCap;
    }

    public float GetScrap() {
        return scrap;
    }

    public void SetScrap(float scrap) {
        this.scrap = scrap;
    }

    public void IncrementScore(float amount) {
        score += amount * combo;
        // Lower the current scrap cap if it is above minScrap
        if(scrapCap > minScrap) {
            scrapCap = Mathf.Round(maxScrap - score / 1500);
            scrapCap = scrapCap < minScrap ? minScrap : scrapCap;
        }
    }

    public void IncrementScrap(int amount) {
        if(scrap < scrapCap && !GameManager.Instance.frozen){
            float newScrap = scrap + amount;
            scrap = newScrap > scrapCap ? scrapCap : newScrap;
            scrapAlarmed = false;
            player.PlaySound(pickup);
        } else if(!scrapAlarmed) {
            scrapAlarmed = true;
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
