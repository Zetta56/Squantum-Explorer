using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AsteroidType {
    Hydra,
    Boss,
    Normal,
}

public class AsteroidSpawner : MonoBehaviour
{
    // References
    public GameObject asteroid;
    public GameObject bossteroid;
    public GameObject hydrasteroid;
    private Transform ship;
    private InterfaceUtils interfaceUtils;
    private Vector3 pos;

    // Logic
    public int spawnRadius = 200;
    public float spawnInterval = 3f;
    public float asteroidSpeed = 5f;
    public float minBottomAngle = 60f;
    public float freezeDuration = 10f;
    public bool frozen = false;
    private float angle;

    // Spawn Chances
    [Space(10)]
    [Header("Spawn Chance")]
    [Range(0f, 1f)]
    public float HydraChance = 0.02f;
    [Range(0f, 1f)]
    public float BossChance = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        ship = GameObject.Find("Spaceship").transform;
        interfaceUtils = GameObject.Find("UI/Interface").GetComponent<InterfaceUtils>();
        StartCoroutine(SpawnAsteroid());
    }

    public void Freeze()
    {
        frozen = true;
        AsteroidController[] asteroids = GetComponentsInChildren<AsteroidController>();
        foreach(AsteroidController asteroid in asteroids) {
            asteroid.speed = 0f;
        }
        StartCoroutine(Unfreeze());
    }

    IEnumerator SpawnAsteroid() {
        yield return new WaitForSeconds(1f);
        while (true) {
            if(!frozen) {
                do {
                    pos = Random.onUnitSphere * spawnRadius;
                    angle = Vector3.Angle(pos - ship.position, Vector3.down);
                } while(angle < minBottomAngle);

                GameObject newAsteroid;
                if(Random.Range(0f, 1f) < HydraChance){
                    newAsteroid = Object.Instantiate(hydrasteroid, pos, Quaternion.identity, transform);
                    newAsteroid.GetComponent<HydrasteroidController>().speed = asteroidSpeed * Mathf.Log10(10f + interfaceUtils.score / 100);
                } else if(Random.Range(0f, 1f) < BossChance){
                    newAsteroid = Object.Instantiate(bossteroid, pos, Quaternion.identity, transform);
                    newAsteroid.GetComponent<BossteroidController>().speed = asteroidSpeed * Mathf.Log10(10f + interfaceUtils.score / 100);
                } else {
                    newAsteroid = Object.Instantiate(asteroid, pos, Quaternion.identity, transform);
                    newAsteroid.GetComponent<AsteroidController>().speed = asteroidSpeed * Mathf.Log10(10f + interfaceUtils.score / 100);
                }
            }
            
            yield return new WaitForSeconds(spawnInterval * (1 / Mathf.Log10(10f + interfaceUtils.score / 1000)));
        }
    }

    IEnumerator Unfreeze() {
        yield return new WaitForSeconds(freezeDuration);
        frozen = false;
        AsteroidController[] asteroids = GetComponentsInChildren<AsteroidController>();
        foreach(AsteroidController asteroid in asteroids) {
            asteroid.speed = asteroidSpeed;
        }
        interfaceUtils.TickCombo();
    }
}
