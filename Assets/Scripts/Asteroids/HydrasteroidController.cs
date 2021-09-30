using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydrasteroidController : AsteroidController
{
    [SerializeField] private GameObject asteroid;

    void Start(){
        base.Start();
        transform.Find("Fragments").GetComponent<Renderer>().material = GetComponent<Renderer>().material;
    }

    public override void Break() {
        if(gameObject != null && !isDestroying){
            if(!touchingShip) {
                for(int i = 0; i < 2; i++){
                    // Random position within 6 units of Hydrasteroid
                    Vector3 pos = Random.onUnitSphere * 6 + transform.position;
                    AsteroidController newAsteroidObject = Object.Instantiate(asteroid, pos, 
                        Quaternion.identity, transform.parent).GetComponent<AsteroidController>();
                    newAsteroidObject.SetSpeed(speed);
                }
            }
            base.Break();
        }
    }
}
