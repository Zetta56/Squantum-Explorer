using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUtils : MonoBehaviour
{
    private GameObject screen1, screen2,screen3, screen4, screen5;

    // Start is called before the first frame update
    void Start()
    {
        screen1 = transform.Find("Screen1").gameObject;
        screen2 = transform.Find("Screen2").gameObject;
        screen3 = transform.Find("Screen3").gameObject;
        screen4 = transform.Find("Screen4").gameObject;
        screen5 = transform.Find("Screen5").gameObject;

        screen2.SetActive(false);
        screen2.transform.Find("Previous").GetComponent<Button>().interactable = false;
        screen3.SetActive(false);
        screen4.SetActive(false);
        screen5.SetActive(false);
        screen5.transform.Find("Next").GetComponent<Button>().interactable = false;
    }

    public void Quit(){
        Application.Quit();
    }
}
