using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapController : MonoBehaviour
{
    [SerializeField] private float acceleration = 20f;
    private float speed = 0f;
    private Transform player;
    private HUDUtils HUDUtils;
    private Rigidbody rb;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        HUDUtils = GameObject.Find("UI/HUD").GetComponent<HUDUtils>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if(!GameManager.Instance.frozen) {
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            speed += acceleration * Time.fixedDeltaTime;
        }
    }

    void OnCollisionEnter(Collision collision) {
        if(collision.collider.name == "Player") {
            Destroy(gameObject);
            HUDUtils.IncrementScrap(1);
        }
    }
}
