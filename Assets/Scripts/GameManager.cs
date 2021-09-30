using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Static variables are tied to the game, not the current scene
    // Since GameManager is a getter, 'get' is public and 'set' is private
    public static GameManager Instance { get; private set; }
    public Color purple = new Color(0.4f, 0f, 1f);
    public bool frozen;

    private void Awake()
    {
        // Set Instance to the first GameManager object in the game
        if(Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        // Destroy all GameManager duplicates spawned whenever a scene loads
        } else {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // Tell our 'OnLevelFinishedLoading' function to start listening for a scene change as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        // Tell our 'OnLevelFinishedLoading' function to stop listening for a scene change as soon as this script is disabled.
        // Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "Main"){
            frozen = false;
        }
    }
}
