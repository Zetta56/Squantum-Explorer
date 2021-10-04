using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseController : MonoBehaviour
{
    // References
    private Rigidbody rb;
    private LineRenderer tether;
    private ParticleSystem speedLines;

    // Logic
    [Header("Controls")]
    [SerializeField] private float sensitivity = 0.005f;
    [SerializeField] private float aimAssistSize = 15f;
    [SerializeField] private float swingSpeed = 1.25f;
    [SerializeField] private float sliceSpeed = 200f;
    [SerializeField] private float sliceDistance = 50f;
    private Color tetherBaseColor;
    private Transform target;
    private Vector3 targetOffset;
    private Vector3 targetDirection;
    private float targetDistance;
    private bool swinging = false;
    private bool slicing = false;
    private float mouseX = 0f;
    private float mouseY = 0f;

    // Audio
    [Header("Sounds")]
    [Range(0.05f, 1f)]
    [SerializeField] private float volume = 0.3f;
    [SerializeField] private AudioClip swingSuccess;
    [SerializeField] private AudioClip swingFail;
    [SerializeField] private AudioClip swingSound;
    private AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        tether = transform.Find("Tether").GetComponent<LineRenderer>();
        tether.enabled = false;
        tetherBaseColor = tether.material.color;
        speedLines = transform.Find("Speed Lines").GetComponent<ParticleSystem>();
        speedLines.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        // Run this if the game is unpaused
        if(!GameManager.Instance.paused){
            // Mouse & Click Controls
            mouseX += Input.GetAxis("Mouse X") * sensitivity;
            mouseY += Input.GetAxis("Mouse Y") * sensitivity;
            // Limit vertical camera movement to 180 degrees
            mouseY = Mathf.Clamp(mouseY, -0.5f, 0.5f);
            transform.rotation = Quaternion.Euler(new Vector4(-mouseY * 180f, mouseX * 360f, transform.rotation.z));
            HandleClick();

            // Make tether's endpoints follow the player and target
            tether.SetPosition(0, tether.transform.position);
            if (target != null && swinging){
                tether.SetPosition(1, target.position + targetOffset);
            }

            // Untrack asteroid if it was destroyed by external source (ex. spaceship collision)
            if(target != null && !target.GetComponent<MeshRenderer>().enabled){
                UntrackTarget();
            }
        }
    }

    void FixedUpdate()
    {
        if(target == null) {
            slicing = false;
            swinging = false;
        }
        // Swinging
        if(swinging) {
            targetDirection = Vector3.Normalize(target.position + targetOffset - transform.position);
            // Dot product finds current velocity projected on the target direction
            float inwardSpeed = Vector3.Dot(rb.velocity, targetDirection);
            if(inwardSpeed < 0) {
                // Removing negative inward speed (a.k.a. outward speed) from velocity
                rb.velocity -= inwardSpeed * targetDirection;
            }
            // Move player towards target at speed scaling with distance
            rb.velocity += ((Mathf.Clamp(GetTargetDistance(), 30, 120) *
                swingSpeed - inwardSpeed) * targetDirection * Time.fixedDeltaTime);
            // Recolor tether color based on distance
            if (GetTargetDistance() < sliceDistance && target.gameObject.layer == 6) {
                tether.material.color = GameManager.Instance.purple;
            } else {
                tether.material.color = tetherBaseColor;
            }
        }

        // Slicing
        if(slicing) {
            // if(target) {
            swinging = false;
            rb.velocity = sliceSpeed * Vector3.Normalize(target.position - transform.position);
            // This normally triggers when slicing an asteroid that isn't the current target
            // } 
            // else {
            //     slicing = false;
            //     rb.velocity *= 0.4f;
            //     speedLines.Stop();
            // }
            
        }
    }

    void HandleClick() {
        // Left-click Swinging
        if(Input.GetMouseButtonDown(0)) {
            // Slow down player when swinging
            rb.velocity *= 0.75f;
            RaycastHit hit;
            // Check if player is looking at a collider (with aim-assist for layer 6 asteroids)
            // Note: out allows changes made to 'hit' to persist after leaving function scope
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit) ||
            Physics.SphereCast(Camera.main.transform.position, aimAssistSize, Camera.main.transform.forward, out hit, Mathf.Infinity, 1 << 6)) {
                target = hit.transform;
                targetOffset = hit.point - target.position;
                swinging = true;
                tether.enabled = true;
                tether.SetPosition(1, hit.point);
                PlaySound(swingSuccess);
                PlaySound(swingSound);
            } else {
                PlaySound(swingFail);
            }
        }
        // Checking if !slicing because UntrackTarget() sets target to null, messing up collision logic when slicing
        if (Input.GetMouseButtonUp(0) && !slicing) {
            UntrackTarget();
        }

        // Right-click Slicing
        if(Input.GetMouseButtonDown(1)) {
            RaycastHit hit;
            // Change target to the asteroid straight ahead (if one exists)
            if(Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, sliceDistance, 1 << 6)) {
                target = hit.transform;
            }
            if(target && target.gameObject.layer == 6 && GetTargetDistance() < sliceDistance) {
                slicing = true;
                speedLines.Play();
            }
        }
    }

    public bool IsSlicing() {
        return slicing;
    }

    public float GetAimAssistSize() {
        return aimAssistSize;
    }

    public float GetSliceDistance() {
        return sliceDistance;
    }

    public Transform GetTarget() {
        return target;
    }

    public float GetTargetDistance() {
        return Vector3.Distance(target.position + targetOffset, transform.position);
    }

    public void PlaySound(AudioClip clip){
        audioSource.PlayOneShot(clip, volume);
    }

    public void UntrackTarget() {
        audioSource.Stop();
        speedLines.Stop();
        target = null;
        slicing = false;
        swinging = false;
        tether.enabled = false;
    }
}
