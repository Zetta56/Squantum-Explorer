using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Movement
    [Space(25)]
    [Header("Movement")]
    [Space(10)]
    public float sensitivity = 0.005f;
    public float gravity = 0.005f;
    public float airResistance = 6f;
    public float maxSpeed = 50f;
    public float jetpackSpeed = 700f;
    public float swingSpeed = 45f;
    public float sliceSpeed = 100f;
    public float sliceDistance = 10f;
    private bool swinging = false;
    private bool slicing = false;
    private Transform target;
    private Vector3 targetOffset;
    private Vector3 targetDirection;
    private float targetDistance;
    private float swingLength;
    private float mouseX = 0f;
    private float mouseY = 0f;
    private bool touchingShip = false;

    // Audio
    [Space(25)]
    [Header("Sounds")]
    [Space(10)]
    [Range(0.05f, 1f)]
    public float Volume = 0.3f;
    [Space(10)]
    public AudioClip swingSuccess;
    public AudioClip swingFail;
    public AudioClip reel;
    public AudioClip pickup;
    public AudioClip swingSound;
    public AudioClip reject;
    public GameObject DJPrefab;
    private GameObject DJ;
    private AudioSource audioSource;
    private AudioClip currentClip;

    // Scrap
    [Space(25)]
    [Header("Scrap")]
    [Space(10)]
    public float maxScrap = 10;
    public float scrap = 0;
    public float scrapBonus = 0;

    // Others
    private Rigidbody rb;
    private GameObject ship;
    private InterfaceUtils interfaceUtils;
    private LineRenderer swingTether;

    void Awake()
    {
        DJ = GameObject.Find("DJ Wacky");
        if(DJ == null) {
            GameObject newDJ = Instantiate(DJPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            newDJ.transform.parent = transform;
            newDJ.name = "DJ Wacky";
            DJ = newDJ;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        interfaceUtils = GameObject.Find("UI/Interface").GetComponent<InterfaceUtils>();
        ship = GameObject.Find("Spaceship");
        audioSource = GetComponent<AudioSource>();
        swingTether = transform.Find("SwingTether").GetComponent<LineRenderer>();
        swingTether.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Controls
        if(!interfaceUtils.GetPauseEnabled()){
            UpdateCamera();
            UpdateTethers();
        }
        if(Input.GetKeyDown(KeyCode.Escape)){
            audioSource.Stop();
            interfaceUtils.TogglePause();
        }
    }

    void FixedUpdate()
    {
        // Air resistance
        rb.AddForce(airResistance * -rb.velocity * Time.fixedDeltaTime);

        // Gravity
        if(!touchingShip) {
            rb.position -= new Vector3(0, gravity, 0) * Time.fixedDeltaTime;
        }

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
            if(inwardSpeed < 0 && targetDistance > swingLength) {
                // Removing negative inward speed (a.k.a. outward speed) from velocity
                rb.velocity -= inwardSpeed * targetDirection;
            }
            // Move player towards target at speed scaling with distance
            rb.velocity += ((Mathf.Clamp(targetDistance, 15, 75)  * swingSpeed - inwardSpeed) * targetDirection * Time.fixedDeltaTime);
            // Shrink swingLength as player gets closer
            if(swingLength > targetDistance) {
                swingLength = targetDistance;
            }
        }

        // Slicing
        if(slicing && target) {
            swinging = false;
            rb.velocity = sliceSpeed * Vector3.Normalize(target.position - transform.position);
        }
    }

    void OnCollisionEnter(Collision collision) {
        // Spaceship
        if(collision.collider.name == "Spaceship") {
            touchingShip = true;
            rb.velocity = new Vector3();    // Stop player movement
        }

        // Asteroids
        if(collision.collider.transform.parent && collision.collider.transform.parent.name == "Asteroid Spawner" &&
           !collision.gameObject.GetComponent<AsteroidController>().isDestroying
        ) {
            if(slicing) {
                rb.velocity *= 0.4f;
            }
            if(GameObject.ReferenceEquals(target, collision.collider.transform)) {
                swinging = false;
                slicing = false;
                target = null;
                swingTether.enabled = false;
            }
            interfaceUtils.IncrementCombo();
            audioSource.Stop();
            collision.gameObject.GetComponent<AsteroidController>().DestroyAsteroid();
        }
    }

    void OnCollisionExit(Collision collision) {
        if(collision.collider.name == "Spaceship") {
            touchingShip = false;
        }
    }

    void UpdateCamera()
    {
        mouseX += Input.GetAxis("Mouse X") * sensitivity;
        mouseY += Input.GetAxis("Mouse Y") * sensitivity;
        // Limiting vertical camera movement to 180 degrees
        mouseY = Mathf.Clamp(mouseY, -0.5f, 0.5f);
        transform.rotation = Quaternion.Euler(new Vector4(-mouseY * 180f, mouseX * 360f, transform.rotation.z));
    }

    void UpdateTethers()
    {
        // Swing Action
        if(Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            // out allows changes made to 'hit' to persist after leaving Raycast() function's scope
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit)) {
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

    public void IncrementScrap(int amount, bool playReject) {
        if(scrap < maxScrap){
            float newScrap = scrap + amount + scrapBonus + (interfaceUtils.GetCombo() - 1);
            scrap = newScrap > maxScrap ? maxScrap : newScrap;
            PlaySound(pickup);
        } else if(playReject) {
            PlaySound(reject);
        }
    }

    public void PlaySound(AudioClip clip){
        float vol = clip == reject ? Volume*0.3f : Volume;
        audioSource.PlayOneShot(clip, vol);
        currentClip = clip;
    }

    IEnumerator WaitForAsteroid(AudioClip clip){
        yield return new WaitForSeconds(0.2f);
        PlaySound(clip);
    }

    public float Map(float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
