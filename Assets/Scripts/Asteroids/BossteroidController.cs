using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossteroidController : AsteroidController
{
    [SerializeField] private float health = 2f;
    [SerializeField] private float push = 100f;
    private Rigidbody playerRB;

    void Start(){
        base.Start();
        playerRB = player.gameObject.GetComponent<Rigidbody>();
        transform.Find("Fragments").GetComponent<Renderer>().material = GetComponent<Renderer>().material;
    }

    public override void Break() {
        if(gameObject != null && !isDestroying){
            health--;
            audioSource.PlayOneShot(boom, Volume);//StateController.Get<float>("SFX", 0.5f)*0.01f);
            fragments.Play();
            
            if(health <= 0 || touchingShip){
                base.Break();
            } else{
                playerRB.velocity = push * -playerRB.velocity.normalized;
                StartCoroutine(TurnOffFragments());
            }
            
        }
    }

    IEnumerator TurnOffFragments()
    {
        yield return new WaitForSeconds(1f);
        fragments.Stop();
    }
}
