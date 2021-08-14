using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapController : MonoBehaviour
{

    public Transform target;
    public float speed = 10f;
    public float moveSpeed = 100f;
    public bool grappling;
    private GameObject player;

    void Start()
    {
        player = GameObject.Find("Player");
        grappling = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(!grappling){
            transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);
        }else{
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, moveSpeed * Time.deltaTime);
        }
    }

    void OnCollisionEnter(Collision collision) {
        if(collision.collider.name == "Player") {
            Destroy(gameObject);
            player.GetComponent<PlayerController>().IncrementScrap(1, true);
        }
    }
}
