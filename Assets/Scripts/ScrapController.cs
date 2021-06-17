using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapController : MonoBehaviour
{

    public Transform target;
    public float speed = 10f;
    public float moveSpeed = 100f;

    public Transform player;

    // Update is called once per frame
    void Update()
    {
        if(player == null){
            transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);
        }else{
            transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }
}
