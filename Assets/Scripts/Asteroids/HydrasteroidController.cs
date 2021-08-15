using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydrasteroidController : AsteroidController
{
    public GameObject asteroid;

    void Start(){
        transform.Find("Break").GetComponent<Renderer>().material = GetComponent<Renderer>().material;
    }

    public override void DestroyAsteroid() {
        if(gameObject != null && !isDestroying){
            if(!touchingShip) {
                for(int i = 0; i < 2; i++){
                    // Random position within 6 units of Hydrasteroid
                    Vector3 pos = Random.onUnitSphere * 6 + transform.position;
                    AsteroidController newAsteroidObject = Object.Instantiate(asteroid, pos, 
                        Quaternion.identity, transform.parent).GetComponent<AsteroidController>();
                    newAsteroidObject.speed = speed;
                }
            }
            base.DestroyAsteroid();
        }
    }
}
