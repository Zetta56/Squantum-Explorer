using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapSpawner : MonoBehaviour
{

    public GameObject scrapPrefab;
    public float rate = 2f;

    public float yMax = 5f;
    public float yMin = -5f;
    public float radiusMin = 12f;
    public float radiusMax = 30f;


    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("Spawn", 0f, rate);
    }

    void Spawn(){
        float yVal = Random.Range(yMin, yMax);
        float rotVal = Random.Range(0, 360);
        float radVal = Random.Range(radiusMin, radiusMax);

        GameObject newScrap = Instantiate(scrapPrefab, new Vector3(0, yVal, 0), Quaternion.identity, transform);
        newScrap.GetComponent<ScrapController>().target = transform;

        newScrap.transform.RotateAround(transform.position, Vector3.up, rotVal);
        newScrap.transform.Translate(-newScrap.transform.forward * radVal);
    }
}
