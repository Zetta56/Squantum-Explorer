using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverUtils : MonoBehaviour
{
    private TextMeshProUGUI score;
    private TextMeshProUGUI bestScore;
    private int scoreValue;
    private int bestScoreValue;

    // Start is called before the first frame update
    void Start()
    {
        score = transform.Find("Score").GetComponent<TextMeshProUGUI>();
        bestScore = transform.Find("BestScore").GetComponent<TextMeshProUGUI>();
        scoreValue = PlayerPrefs.GetInt("score");
        bestScoreValue = PlayerPrefs.GetInt("bestScore");

        if(scoreValue > bestScoreValue) {
            PlayerPrefs.SetInt("bestScore", scoreValue);
            bestScoreValue = scoreValue;
        }
        score.text = "Score: " + scoreValue.ToString();
        bestScore.text = "Best Score: " + bestScoreValue.ToString();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
