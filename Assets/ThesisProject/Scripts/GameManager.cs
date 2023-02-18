using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Barracuda;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public EnemyAgent enemyAgent; //EnemyAgent script, assignned by ItemSpawner

    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI enemyScoreText;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private int timerTotalSeconds;

    //public bool agentTrainingLevel;
    //public bool agentEvaluationLevel;
    [Space(10)]
    [Header("Agent Model Evaluation")]
    [SerializeField] private NNModel[] evaluationBrains;
    [SerializeField] private string modelPath;

    public ItemSpawner itemSpawner;

    public enum LevelType
    {
        PlayerLevel,
        AgentTraining,
        AgentEvaluation
    }

    public LevelType levelType;

    // Start is called before the first frame update
    void Start()
    {
        if(levelType == LevelType.AgentEvaluation)
        {
            evaluationBrains = Resources.LoadAll(modelPath, typeof(NNModel)).Cast<NNModel>().ToArray();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateScoreText(string tag, int score)
    {
        if(tag == "Player")
        {
            playerScoreText.text = score.ToString();
        }
        else if (tag == "Enemy")
        {
            enemyScoreText.text = score.ToString();
        }
    }

    public void ResetScoreText()
    {
        playerScoreText.text = "0";
        enemyScoreText.text = "0";
    }

    public IEnumerator Timer()
    {
        int currentSeconds = timerTotalSeconds;
        var ts = TimeSpan.FromSeconds(currentSeconds);
        timerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);

        while (currentSeconds > 0)
        {
            if (levelType == LevelType.AgentTraining)
                yield return new WaitForSecondsRealtime(1); //If Agent training use unscaled time
            else
                yield return new WaitForSeconds(1); //Else use regular scaled time that allows pauses

            currentSeconds--;

            ts = TimeSpan.FromSeconds(currentSeconds);
            timerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        }

        if (levelType == LevelType.AgentTraining)
        {
            enemyAgent.EndEpisode();
        }

        else if (levelType == LevelType.AgentEvaluation)
        {
            Debug.Log("Writing Evaluation");
        }
    }

    /*
    private void UpdateTimer(double secondsElapsed)
    {
        double secondsLeft = timerTotalSeconds - secondsElapsed;
        var ts = TimeSpan.FromSeconds(secondsLeft);
        timerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
    }
    */
}
