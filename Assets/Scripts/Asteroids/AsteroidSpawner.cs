using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    // References
    [SerializeField] private GameObject asteroid;
    [SerializeField] private GameObject bossteroid;
    [SerializeField] private GameObject hydrasteroid;
    private Transform ship;
    private HUDUtils HUDUtils;

    // Logic
    [SerializeField] private int spawnRadius = 200;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float asteroidSpeed = 5f;
    [SerializeField] private float minBottomAngle = 60f;
    private Vector3 pos;
    private float angle;

    // Spawn Rates
    [Range(0f, 1f)]
    [SerializeField] private float HydraChance = 0.02f;
    [Range(0f, 1f)]
    [SerializeField] private float BossChance = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        ship = GameObject.Find("Spaceship").transform;
        HUDUtils = GameObject.Find("UI/HUD").GetComponent<HUDUtils>();
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
                newAsteroid.GetComponent<AsteroidController>().SetSpeed(asteroidSpeed * Mathf.Log10(10f + HUDUtils.GetScore() / 100));
            }
            yield return new WaitForSeconds(spawnInterval * (1 / Mathf.Log10(10f + HUDUtils.GetScore() / 1000)));
        }
    }
}
