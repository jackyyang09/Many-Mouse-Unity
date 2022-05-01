using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Game : MonoBehaviour {
    public static Game Instance;
    public bool playing;
    public float remainingTime;
    public float initialGameLength = 30;
    public int multiplier = 1;
    public int score;
    public new Camera camera;
    public Canvas canvas;
    public TargetSpawner targetSpawner;
    public GameObject gameOverUI;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public GainScoreUI gainScoreUIPrefab;
    public GainTimeUI gainTimeUIPrefab;
    public Image screenFlash;
    void OnEnable () {
        Instance = this;
        // StartGame();
        EndGame();
    }
    
    void Update () {
        if(playing) {
            remainingTime -= Time.deltaTime;
            scoreText.text = score.ToString();
            if(remainingTime <= 0) {
                EndGame();
            }
        }
        var seconds = Mathf.FloorToInt(remainingTime);
        var milliseconds = Mathf.FloorToInt((remainingTime-seconds)*100);
        timeText.text = string.Format("<mspace=0.65em>{0:00}<size=60%><mspace=0.65em>{1:00}", seconds, milliseconds);
    }

    public void StartGame () {
        remainingTime = initialGameLength;
        score = 0;
        multiplier = 1;
        playing = true;
        targetSpawner.enabled = true;
        gameOverUI.SetActive(false);
    }
    public void EndGame () {
        remainingTime = 0;
        playing = false;
        targetSpawner.enabled = false;
        gameOverUI.SetActive(true);
    }
}
