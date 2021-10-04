using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Controls
    [Header("Controls")]
    [SerializeField] private float airResistance = 7f;
    [SerializeField] private float moveSpeed = 700f;
    [SerializeField] private float freezeDuration = 10f;
    [SerializeField] private Material frozenSky;
    private Material defaultSky;

    // Audio
    [Header("Sounds")]
    [Range(0.05f, 1f)]
    [SerializeField] private float volume = 0.3f;
    [SerializeField] private AudioClip freeze;
    [SerializeField] private AudioClip actionDenied;
    [SerializeField] private AudioClip forceFieldHit;
    private AudioSource audioSource;

    // References
    private Rigidbody rb;
    private GameObject ship;
    private ShipController shipController;
    private HUDUtils HUDUtils;
    private MouseController mouse;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mouse = GetComponent<MouseController>();
        HUDUtils = GameObject.Find("UI/HUD").GetComponent<HUDUtils>();
        ship = GameObject.Find("Spaceship");
        shipController = ship.GetComponent<ShipController>();
        audioSource = GetComponent<AudioSource>();
        defaultSky = RenderSettings.skybox;
    }

    // Update is called once per frame
    void Update()
    {
        // Game Controls (only active when unpaused)
        if(!GameManager.Instance.paused){
            // Space: freeze asteroids
            if(Input.GetKeyDown(KeyCode.Space)) {
                if(HUDUtils.GetScrap() >= HUDUtils.GetScrapCap()) {
                    HUDUtils.SetScrap(0f);
                    GameManager.Instance.frozen = true;
                    RenderSettings.skybox = frozenSky;
                    StartCoroutine(Unfreeze());
                    PlaySound(freeze);
                } else {
                    PlaySound(actionDenied);
                }
            }
        }

        // Escape: pause & unpause
        if(Input.GetKeyDown(KeyCode.Escape)){
            audioSource.Stop();
            HUDUtils.TogglePause();
        }
    }

    void FixedUpdate()
    {
        // Air resistance
        rb.AddForce(airResistance * -rb.velocity * Time.fixedDeltaTime);

        // WASD Movement
        Vector3 forwardInput = (Camera.main.transform.forward * Input.GetAxis("Vertical")).normalized;
        Vector3 rightInput = (Camera.main.transform.right * Input.GetAxis("Horizontal")).normalized;
        rb.AddForce(forwardInput * moveSpeed * Time.deltaTime);
        rb.AddForce(rightInput * moveSpeed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision) {
        // Spaceship
        if(collision.collider.name == "Spaceship") {
            rb.velocity = new Vector3();    // Stop player movement
        }

        // Asteroids
        AsteroidController asteroid = collision.gameObject.GetComponent<AsteroidController>();
        if(asteroid != null && !asteroid.IsDestroying()) {
            if(GameObject.ReferenceEquals(mouse.GetTarget(), collision.collider.transform)) {
                mouse.UntrackTarget();
                rb.velocity *= 0.4f;
            }
            HUDUtils.IncrementCombo();
            audioSource.Stop();
            asteroid.Break();
        }

        // Force Field
        if(collision.collider.name == "Force Field") {
            audioSource.PlayOneShot(forceFieldHit, volume);
        }
    }

    public void PlaySound(AudioClip clip){
        audioSource.PlayOneShot(clip, volume);
    }

    IEnumerator Unfreeze() {
        yield return new WaitForSeconds(freezeDuration);
        GameManager.Instance.frozen = false;
        RenderSettings.skybox = defaultSky;
        shipController.IncrementHitTime(freezeDuration);
        HUDUtils.TickCombo();
    }
}
