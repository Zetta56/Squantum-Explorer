using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public int spawnRadius;
    public float spawnDelay;
    public float asteroidSpeed;
    public GameObject asteroidPrefab;
    public Transform ship;
    public float nonozone = 60;

    private InterfaceUtils interfaceUtils;
    private float angle = 0;
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

            AsteroidController asteroid = Object.Instantiate(asteroidPrefab, pos, Quaternion.identity, transform).GetComponent<AsteroidController>();
            asteroid.target = ship;
            asteroid.speed = asteroidSpeed * Mathf.Log10(10f + interfaceUtils.GetScore() / 100);
            yield return new WaitForSeconds(spawnDelay * (1 / Mathf.Log10(10f + interfaceUtils.GetScore() / 1000)));
        }
    }
}
