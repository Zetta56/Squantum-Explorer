using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// E
// F
using UnityEngine.UI;
using System;
using TMPro;

public class ShipController : MonoBehaviour
{
    public float health;
    public float damageRate = 5f;
    public float maxHealth = 100f;
    public float Volume = 0.3f;
    public AudioClip alarm;
    public AudioClip BoomBoom;

    private InterfaceUtils interfaceUtils;
    private ParticleSystem fire, smoke, shrapnel;
    private Transform DJ;
    private int exploding;
    private AudioSource audioSource;

    private Vector3 EOffScreen;
    private Vector3 EOnScreen;
    private IEnumerator lerpE;
    private float Etime = 0.5f;
    private float EcurrentTime = 0;
    private float EnormalizedValue;
    private RectTransform ERectangleTransform; //I am using all of my willpower to not leave it as ERect
    

    // Start is called before the first frame update
    void Start()
    {
        ERectangleTransform = GameObject.Find("UI/Interface/E").GetComponent<RectTransform>();
        EOffScreen = ERectangleTransform.anchoredPosition;
        EOnScreen = EOffScreen;
        EOnScreen.x += 200;
        
        interfaceUtils = GameObject.Find("UI/Interface").GetComponent<InterfaceUtils>();
        fire = transform.Find("Fire").GetComponent<ParticleSystem>();
        fire.Stop();
        smoke = transform.Find("Smoke").GetComponent<ParticleSystem>();
        smoke.Stop();
        shrapnel = transform.Find("Shrapnel").GetComponent<ParticleSystem>();
        shrapnel.Stop();

        health = maxHealth;
        audioSource = GetComponent<AudioSource>();
        DJ = GameObject.Find("Player/DJ Wacky").transform;
        exploding = 0;
    }

    void FixedUpdate()
    {
        if(exploding>0) {
            if(exploding%50 == 0){
                audioSource.PlayOneShot(alarm, Volume*1.5f);
            }
            if(exploding%5 == 0 && exploding < 25){
                audioSource.PlayOneShot(BoomBoom, Volume*1.5f);
            }
            
            if(exploding == 1) {
                fire.Play();
                smoke.Play();
                shrapnel.Play();
            }
            exploding++;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "Player") {
            if (lerpE != null)
                StopCoroutine(lerpE);
            lerpE = LerpObject(EOffScreen, EOnScreen);
            EcurrentTime = 0;
            StartCoroutine(lerpE);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.collider.name == "Player") {
            if (lerpE != null)
                StopCoroutine(lerpE);
            lerpE = LerpObject(EOnScreen, EOffScreen);
            EcurrentTime = 0;
            StartCoroutine(lerpE);
        }
    }

    public void Hit(){
        health -= damageRate * Mathf.Log10(10 + interfaceUtils.GetScore() / 100);
        audioSource.PlayOneShot(alarm, Volume); // StateController.Get<float>("SFX", 0.5f)*0.01f);
        if(health <= 0f) {
            exploding = 1;
            StartCoroutine(Die());
        }
    }

    IEnumerator LerpObject(Vector3 start, Vector3 end)
    { 
        while (EcurrentTime <= Etime) { 
            EcurrentTime += Time.deltaTime; 
            EnormalizedValue=EcurrentTime/Etime;    
            //ERectangleTransform.position += Vector2.Lerp(start, end, EnormalizedValue); 
            ERectangleTransform.anchoredPosition = Vector3.Lerp(start, end, EnormalizedValue); 
            yield return null; 
    }}

    IEnumerator Die()
    {
        yield return new WaitForSeconds(3);
        PlayerPrefs.SetInt("score", (int)interfaceUtils.GetScore());
        DJ.transform.parent = null;
        DontDestroyOnLoad(DJ);
        SceneManager.LoadScene("GameOver");
    }
}