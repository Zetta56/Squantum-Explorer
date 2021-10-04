using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
	[SerializeField] private AudioClip ButtonPress;
	[SerializeField] private float Volume = 1f;
	private AudioSource audioSource;
  

	void Start() {
		audioSource = GameObject.Find("GameManager").GetComponent<AudioSource>();
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
				audioSource.Stop();
				audioSource.Play();
			}
			SceneManager.LoadScene("Main");
			Time.timeScale = 1;
		}
	}

	public void Menu(){
		PlaySound();
		Time.timeScale = 1;
		SceneManager.LoadScene("MainMenu");
	}

	public void Unpause() {
		PlaySound();
		GameManager.Instance.paused = false;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		Time.timeScale = 1;
		
	}

	public void PlaySound() {
		audioSource.PlayOneShot(ButtonPress, Volume);
	}
}
