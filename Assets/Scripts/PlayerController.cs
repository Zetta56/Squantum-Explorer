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

    [Space(25)]
    [Header("Movement")]
    [Space(10)]
    public float speed = 500;
    public float tetherDistance = 50f;
    public float tetherSpeed = 100f;
    public float grapplingSpeed = 50f;
    public float grapplingLength = 1000f;
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
    public float IndicatorAlpha;
    public float scrap = 0;
    public float scrapBonus = 0;

    //private
    private Rigidbody rb;
    private GameObject ship;
    private GameObject magnet;
    private SphereCollider magnetCollider;
    private GameObject DJ;
    private LineRenderer hookShot, tether;
    private InterfaceUtils interfaceUtils;
    private GameObject upgrades;
    private GameObject pause;
    private Transform targetObj;
    private Vector3 forwardForce;
    private Vector3 rightForce;
    private Vector3 upForce;
    private bool touchingShip = false;
    private bool returning = false;
    private bool grappling = false;
    private float mouseX = 0f;
    private float mouseY = 0f;

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
        magnet = GameObject.Find("Magnet");
        magnet.SetActive(false);
        magnetCollider = magnet.GetComponent<SphereCollider>();
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

        if (grappling && targetObj != null){
            hookShot.SetPosition(1, targetObj.position);
        }

        float DistanceFromShip = (ship.transform.position-transform.position).magnitude;

        // Move towards ship when too far
        if (DistanceFromShip >= tetherDistance) {
            transform.position = Vector3.MoveTowards(transform.position, ship.transform.position, .1f);
        }

        //if(DistanceFromShip > tetherDistance/2f){
        IndicatorAlpha = Map(DistanceFromShip, 0f, tetherDistance, 0f, 100f);
        //}

        if(Input.GetKeyDown(KeyCode.E) && touchingShip) {
            returning = false;
            ToggleMenu(upgrades);
            returning = false;
        }

        if(Input.GetKeyDown(KeyCode.Escape) && !upgrades.activeSelf) {
            ToggleMenu(pause);
        }

        if(!upgrades.activeSelf && !pause.activeSelf){
            UpdateCamera();
            UpdateJetpack();
            UpdateTethers();
        }

    }

    void FixedUpdate()
    {
        if (targetObj == null)
        {
            grappling = false;
            hookShot.enabled = false;
        }

        // Air resistance
        float resistance = rb.velocity.magnitude * 0.1f;
        rb.AddForce(resistance * -rb.velocity.normalized * 60 * Time.fixedDeltaTime);

        if(returning || grappling && targetObj != null) {
            float magnitude = grappling ? grapplingSpeed : tetherSpeed;
            rb.velocity = magnitude * (targetObj.position - transform.position).normalized;
        }
        if(returning && !touchingShip && !TestingMode) {
            magnet.transform.localScale += new Vector3(magnetGrowth, magnetGrowth, magnetGrowth);
            magnetCollider.radius += magnetGrowth / 2;
            magnet.SetActive(true);
        } else {
            rb.position -= new Vector3(0, gravity, 0);
            magnet.transform.localScale = new Vector3(1, 1, 1);
            magnetCollider.radius = 0.5f;
            magnet.SetActive(false);
        }
    }

    void OnCollisionEnter(Collision collision) {
        if(collision.collider.name == "Spaceship") {
            touchingShip = true;
            rb.velocity = new Vector3();    // Stop player movement
        }
        if(collision.collider.name == "Asteroid(Clone)") {
            IncrementScrap(3, false);
            if(!TestingMode){
                interfaceUtils.IncrementScore(100);
            }
            collision.collider.gameObject.GetComponent<AsteroidController>().DestroyAsteroid();
            rb.velocity /= 3;
            if(GameObject.ReferenceEquals(targetObj, collision.collider.gameObject)) {
                grappling = false;
            }
        }
    }

    void OnTriggerEnter(Collider trigger){
        if(trigger.name == "Scrap(Clone)") {
            Destroy(trigger.gameObject);
            IncrementScrap(1, true);
        }
    }

    void OnCollisionExit(Collision collision) {
        if(collision.collider.name == "Spaceship") {
            touchingShip = false;
        }
    }

    public void IncrementScrap(int amount, bool playReject) {
        if(scrap < maxScrap){
            scrap = amount + scrapBonus > maxScrap ? maxScrap : amount + scrapBonus;
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
                targetObj = hit.transform;
                grappling = true;
                rb.velocity = new Vector3();

                hookShot.enabled = true;
                hookShot.SetPosition(1, hit.point);
                PlaySound(hookShotSuccess);
                PlaySound(grapplingSound);
            }
            else if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, grapplingLength, 1 << 7)) {
                targetObj = hit.transform;
                targetObj.gameObject.GetComponent<ScrapController>().player = transform;

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
            if(targetObj != null && targetObj.gameObject.GetComponent<ScrapController>()){
                targetObj.gameObject.GetComponent<ScrapController>().player = null;
            }
        }

        // Ship Tether when right click
        if(Input.GetMouseButtonDown(1) && !grappling) {
            PlaySound(reel);
            if(!touchingShip) {
                targetObj = ship.transform;
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

        if(targetObj !=null && !targetObj.GetComponent<MeshRenderer>().enabled){
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
