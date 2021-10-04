using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    // References
    [SerializeField] protected GameObject scrapPrefab;
    [SerializeField] protected AudioClip boom;
    protected Transform target;
    protected ParticleSystem fragments;
    protected AudioSource audioSource;
    protected Rigidbody rb;
    protected HUDUtils HUDUtils;
    protected PlayerController player;

    // Logic
    [SerializeField] protected int numScrap = 3;
    [SerializeField] protected float Volume = 0.3f;
    [SerializeField] protected float damage = 4f;
    protected float speed = 5f;
    protected bool touchingShip = false;
    protected bool isDestroying = false;

    protected void Start(){
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        HUDUtils = GameObject.Find("UI/HUD").GetComponent<HUDUtils>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        target = GameObject.Find("Spaceship").transform;
        fragments = transform.Find("Fragments").GetComponent<ParticleSystem>();
        fragments.Stop();
    }

    // Update is called once per frame
    protected void Update()
    {
        if(!GameManager.Instance.frozen) {
            transform.LookAt(target);
            rb.velocity = transform.forward * speed;
        } else {
            rb.velocity = Vector3.zero;
        }
    }

    public bool IsDestroying() {
        return isDestroying;
    }

    public void SetSpeed(float speed) {
        this.speed = speed;
    }

    // Virtual indicates that method should be overwritten
    public virtual void Break() {
        if(gameObject != null && !isDestroying){
            // Effects
            audioSource.PlayOneShot(boom, Volume);
            fragments.Play();

            // Counters
            if(!touchingShip) {
                float actualScrap = GameManager.Instance.frozen ? 1 : numScrap;
                HUDUtils.IncrementScore(100);
                for(int i = 0; i < actualScrap; i++){
                    Vector3 pos = Random.onUnitSphere * 10 + transform.position;
                    Object.Instantiate(scrapPrefab, pos, Quaternion.identity, transform.parent);
                }
            }

            // Deletion
            Destroy(gameObject, 2f);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            isDestroying = true;
        }
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (!isDestroying && collision.collider.name == "Spaceship") {
            ShipController ship = collision.gameObject.GetComponent<ShipController>();
            touchingShip = true;
            ship.Hit(damage);
            Break();
        }
    }
}
