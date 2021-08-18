using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    public GameObject scrapPrefab;
    public float speed = 5f;
    public int numScrap = 3;
    public bool isDestroying = false;
    public AudioClip boom;
    public float Volume = 0.3f;

    protected Transform target;
    protected ParticleSystem fragments;
    protected AudioSource audioSource;
    protected Rigidbody rb;
    protected InterfaceUtils interfaceUtils;
    protected PlayerController player;
    protected bool touchingShip; 

    protected void Start(){
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        interfaceUtils = GameObject.Find("UI/Interface").GetComponent<InterfaceUtils>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        target = GameObject.Find("Spaceship").transform;
        fragments = transform.Find("Break").GetComponent<ParticleSystem>();
        fragments.Stop();
    }

    // Update is called once per frame
    protected void Update()
    {
        transform.LookAt(target);
        rb.velocity = transform.forward * speed;
    }

    // Virtual indicates that method should be overwritten
    public virtual void DestroyAsteroid() {
        if(gameObject != null && !isDestroying){
            // Effects
            audioSource.PlayOneShot(boom, Volume);
            fragments.Play();
            if(!touchingShip && !GameManager.Instance.frozen) {
                interfaceUtils.IncrementScore(100);
                for(int i = 0; i < numScrap; i++){
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
            ship.Hit(1);
            touchingShip = true;
            DestroyAsteroid();
        }
    }
}
