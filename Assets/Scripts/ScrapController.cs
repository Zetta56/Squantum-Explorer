using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapController : MonoBehaviour
{
    [SerializeField] private float acceleration = 20f;
    private float speed = 0f;
    private Transform player;
    private InterfaceUtils interfaceUtils;
    private Rigidbody rb;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        interfaceUtils = GameObject.Find("UI/Interface").GetComponent<InterfaceUtils>();
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
            interfaceUtils.IncrementScrap(1);
        }
    }
}
