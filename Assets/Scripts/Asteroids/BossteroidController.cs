using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossteroidController : AsteroidController
{
    public float bossScale = 0f;
    public float health = 2f;
    public float push = 100f;
    private Rigidbody playerRB;

    void Start(){
        base.Start();
        playerRB = player.gameObject.GetComponent<Rigidbody>();
        transform.Find("Break").GetComponent<Renderer>().material = GetComponent<Renderer>().material;
        transform.localScale *= bossScale + 1;
        health *= bossScale + 1;
    }

    public override void DestroyAsteroid() {
        if(gameObject != null && !isDestroying){
            health--;
            audioSource.PlayOneShot(boom, Volume);//StateController.Get<float>("SFX", 0.5f)*0.01f);
            fragments.Play();
            
            if(health <= 0 || touchingShip){
                base.DestroyAsteroid();
            } else{
                playerRB.velocity = push * -playerRB.velocity.normalized;
                StartCoroutine(TurnOffBreak());
            }
            
        }
    }

    IEnumerator TurnOffBreak()
    {
        yield return new WaitForSeconds(1f);
        fragments.Stop();
    }
}
