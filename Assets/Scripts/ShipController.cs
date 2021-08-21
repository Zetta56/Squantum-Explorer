using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipController : MonoBehaviour
{
    // References
    private InterfaceUtils interfaceUtils;
    private ParticleSystem fire, smoke, shrapnel;

    // Audio
    public AudioClip alarm;
    public AudioClip explosion;
    public bool DeathSounded = false;
    public float Volume = 0.3f;
    private AudioSource audioSource;

    // Logic
    public float maxHealth = 100f;
    public float regenTimer = 10f;
    public float regenRate = 1f;
    private float hitTime;
    private float health;
    private int exploding;

    // Start is called before the first frame update
    void Start()
    {   
        interfaceUtils = GameObject.Find("UI/Interface").GetComponent<InterfaceUtils>();
        audioSource = GetComponent<AudioSource>();
        health = maxHealth;

        // Particle Initialization
        fire = transform.Find("Fire").GetComponent<ParticleSystem>();
        fire.Stop();
        smoke = transform.Find("Smoke").GetComponent<ParticleSystem>();
        smoke.Stop();
        shrapnel = transform.Find("Shrapnel").GetComponent<ParticleSystem>();
        shrapnel.Stop();
        exploding = 0;
        
    }

    void FixedUpdate()
    {
        if(hitTime + regenTimer <= Time.time && !GameManager.Instance.frozen) {
            health += regenRate * Time.fixedDeltaTime;
        }

        // Low-health warning
        if(health <= 0.1f * maxHealth && !DeathSounded){
            DeathSounded = true;
            for(int i = 0; i < 3; i++){
                StartCoroutine(Warn(i));
            }
        }

        // Death Animation
        if(exploding > 0) {
            if(exploding % 50 == 0){
                audioSource.PlayOneShot(alarm, Volume * 1.5f);
            }
            if(exploding % 5 == 0 && exploding < 25){
                audioSource.PlayOneShot(explosion, Volume * 1.5f);
            }
            if(exploding == 1) {
                fire.Play();
                smoke.Play();
                shrapnel.Play();
            }
            exploding++;
        }
    }

    public float GetHealth() {
        return health;
    }

    public void IncrementHitTime(float increment) {
        hitTime += increment;
    }

    public void Hit(float damage) {
        health -= damage * Mathf.Log10(10 + interfaceUtils.score / 100);
        audioSource.PlayOneShot(alarm, Volume); // StateController.Get<float>("SFX", 0.5f)*0.01f);
        if(health <= 0f) {
            exploding = 1;
            StartCoroutine(Die());
        }
        hitTime = Time.time;
    }

    IEnumerator Warn(int i) {
        yield return new WaitForSeconds(0.627f * i);
        audioSource.PlayOneShot(alarm, Volume * 1.5f);
    }

    IEnumerator Die() {
        yield return new WaitForSeconds(3);
        PlayerPrefs.SetInt("score", (int)interfaceUtils.score);
        SceneManager.LoadScene("GameOver");
    }
}
