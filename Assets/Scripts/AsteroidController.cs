using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidController : MonoBehaviour
{
    public float speed;
    public Transform target;

    public AudioClip boom;
    public float Volume = 0.3f;
    public bool isDestroying = false;
    AudioSource audioSource;

    ParticleSystem.EmissionModule em;
    public Transform Break;

    private Rigidbody rb;

    void Awake(){
        em = Break.GetComponent<ParticleSystem>().emission;
        em.enabled = false;

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target);
        rb.velocity = transform.forward * speed;
    }

    public void DestroyAsteroid() {
        if(gameObject != null && !isDestroying){
            audioSource.PlayOneShot(boom, Volume);//StateController.Get<float>("SFX", 0.5f)*0.01f);
            em.enabled = true;
            Break.GetComponent<ParticleSystem> ().Play();
            gameObject.GetComponent<MeshRenderer>().enabled = false;
            isDestroying = true;
            Destroy(gameObject, 1.5f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isDestroying && collision.collider.name == "Spaceship") {
            ShipController ship = collision.gameObject.GetComponent<ShipController>();
            ship.Hit();
            DestroyAsteroid();
        }
    }

}
