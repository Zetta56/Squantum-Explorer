using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{

	private AudioSource DJ;
	public AudioClip ButtonPress;
	[Range(0.5f, 2f)]
    public float Volume = 1f;

	void Start() {
		DJ = GameObject.Find("DJ Wacky").GetComponent<AudioSource>();
	}

	public void SetHasPlayed() {
		PlayerPrefs.SetInt("HasPlayed", 1);
	}

	public void Play() {
		if (PlayerPrefs.GetInt("HasPlayed", 0) == 0 && SceneManager.GetActiveScene().name == "MainMenu") {
			SetHasPlayed();
			Transform menu = GameObject.Find("Menu").transform;
			menu.Find("Screen1").gameObject.SetActive(false);
			menu.Find("Screen2").gameObject.SetActive(true);
		} else {
			if(SceneManager.GetActiveScene().name != "MainMenu"){
				DJ.Stop();
				DJ.Play();
			}
			SceneManager.LoadScene("Main");
			Time.timeScale = 1;
		}
	}

	public void Menu(){
		Time.timeScale = 1;
		SceneManager.LoadScene("MainMenu");
	}

	public void Unpause() {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Time.timeScale = 1;
		
	}

	public void ButtonNoise(){
		DJ.PlayOneShot(ButtonPress, 2f);
	}
}