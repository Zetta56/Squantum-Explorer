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
    public int spawnRadius;
    public float spawnDelay;
    public float asteroidSpeed;
    public GameObject asteroid;
    public GameObject bossteroid;
    public GameObject hydrasteroid;
    public Transform ship;
    public float nonozone = 60f;

    [Space(10)]
    [Header("Spawn Chance")]
    [Range(0f, 1f)]
    public float HydraChance = 0.02f;
    [Range(0f, 1f)]
    public float BossChance = 0.1f;

    private InterfaceUtils interfaceUtils;
    private float angle = 0f;
    private Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        interfaceUtils = GameObject.Find("UI/Interface").GetComponent<InterfaceUtils>();
        StartCoroutine(SpawnAsteroid(1f));
    }

    IEnumerator SpawnAsteroid(float time)
    {
        yield return new WaitForSeconds(time);
        while (true)
        {   
            do {
                pos = Random.onUnitSphere * spawnRadius;
                angle = Vector3.Angle(pos - ship.position, Vector3.down);
            } while(angle<nonozone);

            GameObject newAsteroid;
            if(Random.Range(0f, 1f) < HydraChance){
                newAsteroid = Object.Instantiate(hydrasteroid, pos, Quaternion.identity, transform);
                newAsteroid.GetComponent<HydrasteroidController>().speed = asteroidSpeed * Mathf.Log10(10f + interfaceUtils.GetScore() / 100);
            } else if(Random.Range(0f, 1f) < BossChance){
                newAsteroid = Object.Instantiate(bossteroid, pos, Quaternion.identity, transform);
                newAsteroid.GetComponent<BossteroidController>().speed = asteroidSpeed * Mathf.Log10(10f + interfaceUtils.GetScore() / 100);
            } else {
                newAsteroid = Object.Instantiate(asteroid, pos, Quaternion.identity, transform);
                newAsteroid.GetComponent<AsteroidController>().speed = asteroidSpeed * Mathf.Log10(10f + interfaceUtils.GetScore() / 100);
            }
            
            yield return new WaitForSeconds(spawnDelay * (1 / Mathf.Log10(10f + interfaceUtils.GetScore() / 1000)));
        }
    }

}
