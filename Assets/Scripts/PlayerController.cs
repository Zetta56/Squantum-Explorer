using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Space(25)]
    [Header("Mechanics")]
    [Space(10)]
    public float maxFuel = 100;
    /* public float fuelRegen = 5; */
    public float fuelCost = 30;
    public float magnetGrowth = 0.1f;
    public float maxScrap = 10;
    public float comboTimeSubtractor = 4f;

    [Space(25)]
    [Header("Movement")]
    [Space(10)]
    public float speed = 1000;
    public float tetherDistance = 75f;
    public float tetherSpeed = 25f;
    public float grapplingSpeed = 100f;
    public float grapplingLength = 50f;
    public float gravity = 0.005f;
    public float sensitivity = 0.005f;

    [Space(25)]
    [Header("Other")]
    [Space(10)]
    public GameObject djWacky;
    public bool TestingMode = false;

    // Audio
    [Space(25)]
    [Header("Soundz")]
    [Space(10)]
    [Range(0.05f, 1f)]
    public float Volume = 0.3f;
    [Space(10)]
    public AudioClip hookShotSuccess;
    public AudioClip hookShotFail;
    public AudioClip reel;
    public AudioClip pickup;
    public AudioClip jetpack;
    public AudioClip grapplingSound;
    public AudioClip reject;
    private AudioClip currentClip;

    //Collected on start
    [Space(25)]
    [Header("Assigned on Start")]
    [Space(10)]
    public AudioSource audioSource;
    public float fuel;
    public float indicatorAlpha;
    public float scrap = 0;
    public float scrapBonus = 0;
    public int combo = 1;

    //private
    private Rigidbody rb;
    private GameObject ship;
    private SphereCollider magnet;
    private GameObject DJ;
    private LineRenderer hookShot, tether;
    private InterfaceUtils interfaceUtils;
    private GameObject upgrades;
    private GameObject pause;
    private Transform target;
    private Vector3 forwardForce;
    private Vector3 rightForce;
    private Vector3 upForce;
    private bool touchingShip = false;
    private bool returning = false;
    private bool grappling = false;
    private float mouseX = 0f;
    private float mouseY = 0f;
    private float LastBoomBoom = 0f;

    void Awake()
    {
        upgrades = GameObject.Find("UI/Upgrades"); // Done before Upgrades becomes disabled
        pause = GameObject.Find("UI/Pause");
        DJ = GameObject.Find("DJ Wacky");
        if(DJ == null) {
            GameObject newDJ = Instantiate(djWacky, new Vector3(0, 0, 0), Quaternion.identity);
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
        pause.SetActive(false);
        magnet = GameObject.Find("Magnet").GetComponent<SphereCollider>();
        ship = GameObject.Find("Spaceship");

        hookShot = transform.Find("Hookshot").GetComponent<LineRenderer>();
        hookShot.enabled = false;
        tether = transform.Find("Tether").GetComponent<LineRenderer>();
        tether.enabled = true;
        tether.SetPosition(1, ship.GetComponent<Renderer>().bounds.center);

        fuel = maxFuel;
        /* InvokeRepeating("RegenFuel", 1f, 0.1f); */

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        audioSource = GetComponent<AudioSource>();

        if(TestingMode){
            GameObject.Find("UI/Interface").SetActive(false);
            tether.enabled = false;
            ShipController shipController=ship.GetComponent<ShipController>();
            shipController.maxHealth = 2;
            shipController.health = 2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (touchingShip)
            fuel = maxFuel;

        if (grappling && target != null){
            hookShot.SetPosition(1, target.position);
        }

        float DistanceFromShip = Vector3.Distance(ship.transform.position, transform.position);

        // Move towards ship when too far
        if (DistanceFromShip >= tetherDistance) {
            transform.position = Vector3.MoveTowards(transform.position, ship.transform.position, 1f);
            rb.velocity /= 3;
        }

        indicatorAlpha = Map(DistanceFromShip, 0f, tetherDistance, 0f, 100f);

        if(Input.GetKeyDown(KeyCode.E) && touchingShip) {
            returning = false;
            ToggleMenu(upgrades);
            returning = false;
            interfaceUtils.crosshair.enabled = true;
            interfaceUtils.magnet.enabled = false;
        }
        if(!upgrades.activeSelf && !pause.activeSelf){
            UpdateCamera();
            UpdateJetpack();
            UpdateTethers();
        }

        if(Input.GetKeyDown(KeyCode.Escape)){
            if(upgrades.activeSelf){
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                returning = false;
                ToggleMenu(upgrades);
                returning = false;
            } else{
                ToggleMenu(pause);
            }
        }

        

    }

    void FixedUpdate()
    {
        if (target == null)
        {
            grappling = false;
            hookShot.enabled = false;
        }

        // Air resistance
        float resistance = rb.velocity.magnitude * 0.1f;
        rb.AddForce(resistance * -rb.velocity.normalized * 60 * Time.fixedDeltaTime);

        if(returning || grappling && target != null) {
            float magnitude = grappling ? grapplingSpeed : tetherSpeed;
            rb.velocity = magnitude * (target.position - transform.position).normalized;
        }
        if(returning && !touchingShip && !TestingMode) {
            interfaceUtils.magnet.transform.localScale += Vector3.one * (magnetGrowth/6);

            magnet.radius += magnetGrowth / 2;
        } else {
            rb.position -= new Vector3(0, gravity, 0);
            magnet.radius = 0.5f;
            interfaceUtils.magnet.transform.localScale = Vector3.one;
        }
        if(Time.time - LastBoomBoom > comboTimeSubtractor){
            combo = 1;
        }
    }

    void OnCollisionEnter(Collision collision) {
        if(collision.collider.name == "Spaceship") {
            touchingShip = true;
            rb.velocity = new Vector3();    // Stop player movement
        }
        if(collision.collider.transform.parent && collision.collider.transform.parent.name == "Asteroid Spawner" &&
           !collision.gameObject.GetComponent<AsteroidController>().isDestroying
        ) {
            if(Time.time - LastBoomBoom <= comboTimeSubtractor){
                combo++;
                interfaceUtils.ShowCombo();
            }
            LastBoomBoom = Time.time;
            collision.gameObject.GetComponent<AsteroidController>().DestroyAsteroid();
            rb.velocity /= 3;
            if(GameObject.ReferenceEquals(target, collision.collider.transform)) {
                grappling = false;
                target = null;
            }
        }
    }

    void OnCollisionExit(Collision collision) {
        if(collision.collider.name == "Spaceship") {
            touchingShip = false;
        }
    }

    public void IncrementScrap(int amount, bool playReject) {
        if(scrap < maxScrap){
            float newScrap = scrap + amount + scrapBonus + (combo - 1);
            scrap = newScrap > maxScrap ? maxScrap : newScrap;
            PlaySound(pickup);
        } else if(playReject) {
            PlaySound(reject);
        }
    }

    void ToggleMenu(GameObject menu) {
        menu.SetActive(!menu.activeSelf);
        audioSource.Stop();
        if (menu.activeSelf) {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
        }
    }

    void UpdateCamera()
    {
        mouseX += Input.GetAxis("Mouse X") * sensitivity;
        mouseY += Input.GetAxis("Mouse Y") * sensitivity;

        if (mouseY < -0.5f)
            mouseY = -0.5f;
        else if (mouseY > 0.5f)
            mouseY = 0.5f;

        if(!TestingMode){
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, grapplingLength, 1 << 6 | 1 << 7)) {
                interfaceUtils.ColorCrosshair(Color.red);
            } else {
                interfaceUtils.ColorCrosshair(Color.white);
            }
        }

        transform.rotation = Quaternion.Euler(new Vector4(-mouseY * 180f, mouseX * 360f, transform.rotation.z));
    }

    void UpdateJetpack()
    {
        // Validate fuel
        if (fuel < 0.0f) {
            fuel = 0.0f;
        }

        if (fuel > 0.0f && !grappling && !returning)
        {
            forwardForce = (Camera.main.transform.forward * Input.GetAxis("Vertical")).normalized;
            forwardForce.y = 0;
            rightForce = (Camera.main.transform.right * Input.GetAxis("Horizontal")).normalized;
            rightForce.y = 0;
            // Shift & Space keys to move up and down
            upForce = (Vector3.up * Input.GetAxis("UpDown")).normalized;

            if(Input.GetAxis("Vertical") + Input.GetAxis("Horizontal") + Input.GetAxis("UpDown") != 0f){
                //PlaySound(jetpack);
            }else{
                if(currentClip == jetpack && audioSource.isPlaying) {
                    StartCoroutine(WaitForStop());
                }
            }

            if (forwardForce != Vector3.zero)
            {
                fuel -= fuelCost * Time.deltaTime;
                if (fuel <= 0f) return;
                rb.AddForce(forwardForce * speed * Time.deltaTime);
            }

            if (rightForce != Vector3.zero)
            {
                fuel -= fuelCost * Time.deltaTime;
                if (fuel <= 0f) return;
                rb.AddForce(rightForce * speed * Time.deltaTime);
            }

            if (upForce != Vector3.zero)
            {
                fuel -= fuelCost * Time.deltaTime;
                if (fuel <= 0f) return;
                rb.AddForce(upForce * speed * Time.deltaTime);
            }
        }
    }

    void UpdateTethers()
    {

        // Hookshot
        if(Input.GetMouseButtonDown(0) && !returning) {
            RaycastHit hit;
            // out allows changes made to 'hit' to persist after leaving Raycast()'s scope
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, grapplingLength, 1 << 6)) {
                target = hit.transform;
                grappling = true;
                rb.velocity = new Vector3();

                hookShot.enabled = true;
                hookShot.SetPosition(1, hit.point);
                PlaySound(hookShotSuccess);
                PlaySound(grapplingSound);
            }
            else if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, grapplingLength, 1 << 7)) {
                target = hit.transform;
                target.gameObject.GetComponent<ScrapController>().grappling = true;

                hookShot.enabled = true;
                hookShot.SetPosition(1, hit.point);
                PlaySound(hookShotSuccess);
                PlaySound(grapplingSound);
            }else{
                PlaySound(hookShotFail);
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            audioSource.Stop();
            grappling = false;
            hookShot.enabled = false;
            if(target != null && target.gameObject.GetComponent<ScrapController>()){
                target.gameObject.GetComponent<ScrapController>().grappling = false;
            }
        }

        // Ship Tether when right click
        if(Input.GetMouseButtonDown(1) && !grappling) {
            PlaySound(reel);
            if(!touchingShip) {
                target = ship.transform;
                returning = true;
                rb.velocity = new Vector3();
                interfaceUtils.crosshair.enabled = false;
                interfaceUtils.magnet.enabled = true;
            }
        }

        if (Input.GetMouseButtonUp(1)) {
            returning = false;
            audioSource.Stop();
            interfaceUtils.crosshair.enabled = true;
            interfaceUtils.magnet.enabled = false;
        }

        if(target != null && !target.GetComponent<MeshRenderer>().enabled){
            grappling = false;
            hookShot.enabled = false;
        }

        tether.SetPosition(0, transform.Find("Tether").position);
        hookShot.SetPosition(0, transform.Find("Hookshot").position);
    }

    /* void RegenFuel() */
    /* { */
    /*     if ((Input.GetAxis("Vertical") == 0f && Input.GetAxis("Horizontal") == 0f && Input.GetAxis("UpDown") == 0f) || (grappling || returning)) */
    /*     { */
    /*         fuel += fuelRegen; */
    /*         if (fuel > maxFuel) */
    /*             fuel = maxFuel; */
    /*     } */
    /* } */

    public void PlaySound(AudioClip clip){
        float vol = clip == reject ? Volume*0.3f : Volume;
        audioSource.PlayOneShot(clip, vol);//StateController.Get<float>("SFX", 0.5f)*0.01f);
        currentClip = clip;
    }

    IEnumerator WaitForStop() {
        yield return new WaitForSeconds(0.4f);
        if(currentClip == jetpack && audioSource.isPlaying) {
            audioSource.Stop();
        }
    }

    IEnumerator WaitForAsteroid(AudioClip clip){
        yield return new WaitForSeconds(0.2f);
        PlaySound(clip);
    }

    public float Map(float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
