using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Controls
    [Space(25)]
    [Header("Controls")]
    [Space(10)]
    public float sensitivity = 0.005f;
    public float aimAssistSize = 25f;
    public float airResistance = 7f;
    public float jetpackSpeed = 700f;
    public float swingSpeed = 1f;
    public float sliceSpeed = 200f;
    public float sliceDistance = 50f;
    public float freezeDuration = 10f;

    // Audio
    [Space(25)]
    [Header("Sounds")]
    [Space(10)]
    [Range(0.05f, 1f)]
    public float volume = 0.3f;
    public AudioClip swingSuccess;
    public AudioClip swingFail;
    public AudioClip swingSound;
    public AudioClip reject;
    private AudioSource audioSource;

    // References
    private Rigidbody rb;
    private GameObject ship;
    private ShipController shipController;
    private InterfaceUtils interfaceUtils;
    private LineRenderer swingTether;
    private ParticleSystem speedLines;
    private AsteroidSpawner asteroidSpawner;
    private ForceFieldController forceField;

    // Logic
    private Transform target;
    private Vector3 targetOffset;
    private Vector3 targetDirection;
    private float targetDistance;
    private bool swinging = false;
    private bool slicing = false;
    private float mouseX = 0f;
    private float mouseY = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        interfaceUtils = GameObject.Find("UI/Interface").GetComponent<InterfaceUtils>();
        ship = GameObject.Find("Spaceship");
        shipController = ship.GetComponent<ShipController>();
        asteroidSpawner = GameObject.Find("Asteroid Spawner").GetComponent<AsteroidSpawner>();
        audioSource = GetComponent<AudioSource>();
        forceField = GameObject.Find("Force Field").GetComponent<ForceFieldController>();
        swingTether = transform.Find("Swing Tether").GetComponent<LineRenderer>();
        swingTether.enabled = false;
        speedLines = transform.Find("Speed Lines").GetComponent<ParticleSystem>();
        speedLines.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        // Controls
        if(!interfaceUtils.IsPaused()){
            UpdateCamera();
            UpdateTethers();
            if(Input.GetKeyDown(KeyCode.Space)) {
                if(interfaceUtils.scrap >= interfaceUtils.scrapCap) {
                    interfaceUtils.scrap = 0f;
                    GameManager.Instance.frozen = true;
                    forceField.ToggleFrozen(true);
                    StartCoroutine(Unfreeze());
                    PlaySound(swingSuccess);
                } else {
                    PlaySound(reject);
                }
            }
        }

        // Pause and unpause
        if(Input.GetKeyDown(KeyCode.Escape)){
            audioSource.Stop();
            interfaceUtils.TogglePause();
        }
    }

    void FixedUpdate()
    {
        // Air resistance
        rb.AddForce(airResistance * -rb.velocity * Time.fixedDeltaTime);

        // Jetpack
        Vector3 forwardInput = (Camera.main.transform.forward * Input.GetAxis("Vertical")).normalized;
        Vector3 rightInput = (Camera.main.transform.right * Input.GetAxis("Horizontal")).normalized;
        rb.AddForce(forwardInput * jetpackSpeed * Time.deltaTime);
        rb.AddForce(rightInput * jetpackSpeed * Time.deltaTime);

        // Swinging
        if(swinging && target) {
            targetDirection = Vector3.Normalize(target.position + targetOffset - transform.position);
            targetDistance = Vector3.Distance(target.position + targetOffset, transform.position);
            // Dot product finds current velocity projected on the target direction
            float inwardSpeed = Vector3.Dot(rb.velocity, targetDirection);
            if(inwardSpeed < 0) {
                // Removing negative inward speed (a.k.a. outward speed) from velocity
                rb.velocity -= inwardSpeed * targetDirection;
            }
            // Move player towards target at speed scaling with distance
            rb.velocity += ((Mathf.Clamp(targetDistance, 30, 120)  * swingSpeed - inwardSpeed) * targetDirection * Time.fixedDeltaTime);
        }

        // Slicing
        if(slicing) {
            if(target) {
                swinging = false;
                rb.velocity = sliceSpeed * Vector3.Normalize(target.position - transform.position);
            } else {
                slicing = false;
                rb.velocity *= 0.4f;
                speedLines.Stop();
            }
            
        }
    }

    void OnCollisionEnter(Collision collision) {
        // Spaceship
        if(collision.collider.name == "Spaceship") {
            rb.velocity = new Vector3();    // Stop player movement
        }

        // Asteroids
        AsteroidController asteroid = collision.gameObject.GetComponent<AsteroidController>();
        if(asteroid != null && !asteroid.IsDestroying()) {
            if(slicing) {
                rb.velocity *= 0.4f;
                speedLines.Stop();
            }
            if(GameObject.ReferenceEquals(target, collision.collider.transform)) {
                swinging = false;
                slicing = false;
                target = null;
                swingTether.enabled = false;
            }
            interfaceUtils.IncrementCombo();
            audioSource.Stop();
            asteroid.Break();
        }
    }

    public void PlaySound(AudioClip clip){
        audioSource.PlayOneShot(clip, volume);
    }

    public float GetAimAssistSize() {
        return aimAssistSize;
    }

    private void UpdateCamera()
    {
        mouseX += Input.GetAxis("Mouse X") * sensitivity;
        mouseY += Input.GetAxis("Mouse Y") * sensitivity;
        // Limiting vertical camera movement to 180 degrees
        mouseY = Mathf.Clamp(mouseY, -0.5f, 0.5f);
        transform.rotation = Quaternion.Euler(new Vector4(-mouseY * 180f, mouseX * 360f, transform.rotation.z));
    }

    private void UpdateTethers()
    {
        // Swing Action
        if(Input.GetMouseButtonDown(0)) {
            // Slow down player when grappling
            rb.velocity *= 0.75f;
            RaycastHit hit;
            // Check if player is looking at a collider (with aim-assist for layer 6 asteroids)
            // Note: out allows changes made to 'hit' to persist after leaving function scope
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit) ||
               Physics.SphereCast(Camera.main.transform.position, aimAssistSize, Camera.main.transform.forward, out hit, Mathf.Infinity, 1 << 6)) {
                target = hit.transform;
                targetOffset = hit.point - target.position;
                swinging = true;
                swingTether.enabled = true;
                swingTether.SetPosition(1, hit.point);
                PlaySound(swingSuccess);
                PlaySound(swingSound);
            } else {
                PlaySound(swingFail);
            }
        }
        if (Input.GetMouseButtonUp(0)) {
            audioSource.Stop();
            swinging = false;
            swingTether.enabled = false;
        }

        // Slice Action
        if(Input.GetMouseButtonDown(1)) {
            RaycastHit hit;
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, sliceDistance, 1 << 6)) {
                target = hit.transform;
            }
            if(target && target.gameObject.layer == 6 && Vector3.Distance(target.position + targetOffset, transform.position) < sliceDistance) {
                slicing = true;
                speedLines.Play();
            }
        }

        // Make swing tether follow player and target
        swingTether.SetPosition(0, swingTether.transform.position);
        if (target != null && swinging){
            swingTether.SetPosition(1, target.position + targetOffset);
        }

        // Stop swinging if asteroid was destroyed by external source (ex. spaceship)
        if(target != null && !target.GetComponent<MeshRenderer>().enabled){
            target = null;
            swinging = false;
            swingTether.enabled = false;
        }
    }

    IEnumerator Unfreeze() {
        yield return new WaitForSeconds(freezeDuration);
        GameManager.Instance.frozen = false;
        forceField.ToggleFrozen(false);
        shipController.IncrementHitTime(freezeDuration);
        interfaceUtils.TickCombo();
    }
}
