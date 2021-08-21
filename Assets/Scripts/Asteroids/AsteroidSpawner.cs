using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public enum AsteroidType {
//     Hydra,
//     Boss,
//     Normal,
// }

public class AsteroidSpawner : MonoBehaviour
{
    // References
    public GameObject asteroid;
    public GameObject bossteroid;
    public GameObject hydrasteroid;
    private Transform ship;
    private InterfaceUtils interfaceUtils;

    // Logic
    public int spawnRadius = 200;
    public float spawnInterval = 3f;
    public float asteroidSpeed = 5f;
    public float minBottomAngle = 60f;
    private Vector3 pos;
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

    GameObject PickAsteroid() {
        if(Random.Range(0f, 1f) < HydraChance){
            return hydrasteroid;
        } else if(Random.Range(0f, 1f) < BossChance){
            return bossteroid;
        } else {
            return asteroid;
        }
    }

    IEnumerator SpawnAsteroid() {
        yield return new WaitForSeconds(1f);
        while (true) {
            if(!GameManager.Instance.frozen) {
                // Get random location to spawn asteroid
                do {
                    pos = Random.onUnitSphere * spawnRadius;
                    angle = Vector3.Angle(pos - ship.position, Vector3.down);
                } while(angle < minBottomAngle);
                // Instantiate asteroid and set its speed
                GameObject newAsteroid = Object.Instantiate(PickAsteroid(), pos, Quaternion.identity, transform);
                newAsteroid.GetComponent<AsteroidController>().speed = asteroidSpeed * Mathf.Log10(10f + interfaceUtils.score / 100);
            }
            yield return new WaitForSeconds(spawnInterval * (1 / Mathf.Log10(10f + interfaceUtils.score / 1000)));
        }
    }
}
