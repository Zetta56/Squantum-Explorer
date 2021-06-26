using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    public float speed;
    public bool isDestroying = false;
    public AudioClip boom;
    public float Volume = 0.3f;

    protected Transform target;
    protected ParticleSystem fragments;
    protected AudioSource audioSource;
    protected ParticleSystem.EmissionModule em;
    protected Rigidbody rb;
    protected InterfaceUtils interfaceUtils;
    protected PlayerController player;
    protected bool touchingShip; 

    protected void Awake(){
        em = transform.Find("Break").GetComponent<ParticleSystem>().emission;
        em.enabled = false;

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        interfaceUtils = GameObject.Find("UI/Interface").GetComponent<InterfaceUtils>();
        player = GameObject.Find("Player").GetComponent<PlayerController>();
        target = GameObject.Find("Spaceship").transform;
        fragments = transform.Find("Break").GetComponent<ParticleSystem>();
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
            audioSource.PlayOneShot(boom, Volume);//StateController.Get<float>("SFX", 0.5f)*0.01f);
            em.enabled = true;
            fragments.Play();
            Destroy(gameObject, 2f);
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            isDestroying = true;
            if(!touchingShip) {
                interfaceUtils.IncrementScore(100);
                player.IncrementScrap(3, false);
            }
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
