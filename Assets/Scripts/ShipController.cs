using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipController : MonoBehaviour
{
    // References
    private HUDUtils HUDUtils;
    private ParticleSystem fire, smoke, shrapnel;

    // Logic
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float regenTimer = 10f;
    [SerializeField] private float regenRate = 1f;
    private float hitTime;
    private float health;
    private int exploding;

    // Audio
    [Header("Sounds")]
    [Range(0.05f, 1f)]
    [SerializeField] private float Volume = 0.3f;
    [SerializeField] private AudioClip alarm;
    [SerializeField] private AudioClip explosion;
    private AudioSource audioSource;
    private bool deathSounded = false;

    // Start is called before the first frame update
    void Start()
    {   
        HUDUtils = GameObject.Find("UI/HUD").GetComponent<HUDUtils>();
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
        if(health <= 0.1f * maxHealth && !deathSounded){
            deathSounded = true;
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

    public float GetMaxHealth() {
        return maxHealth;
    }

    public void IncrementHitTime(float increment) {
        hitTime += increment;
    }

    public void Hit(float damage) {
        health -= damage * Mathf.Log10(10 + HUDUtils.GetScore() / 100);
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
        PlayerPrefs.SetInt("score", (int)HUDUtils.GetScore());
        SceneManager.LoadScene("GameOver");
    }
}
