using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Barracuda;
using UnityEditor;
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
    [SerializeField] private NNModel[] evaluationModels;
    [SerializeField] private string modelPath;

    private int currentModel = 0;
    private int modelRound = 0;
    private int[] roundScores;
    StringBuilder sb;

    public ItemSpawner itemSpawner;

    [Space(10)]
    [Header("Game End")]
    public bool isGameFinished;
    public GameObject endGamePanel;
    public TextMeshProUGUI endGameHeader;
    public TextMeshProUGUI endGamePlayerScore;
    public TextMeshProUGUI endGameEnemyScore;

    public GameObject[] endGameTextObjects;

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
            roundScores = new int[3];

            sb = new StringBuilder("ModelNumber,ModelName,Round1Score,Round2Score,Round3Score,AvgScore");
            evaluationModels = Resources.LoadAll(modelPath, typeof(NNModel)).Cast<NNModel>().ToArray();
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


        if (levelType == LevelType.AgentEvaluation) //If is evaluation level, set agent brain to correct one to evaluate
        {
            enemyAgent.SetModel("ResourceAgent", evaluationModels[currentModel]);
        }

        while (currentSeconds > 0)
        {
            /*if (levelType == LevelType.AgentTraining)
                yield return new WaitForSecondsRealtime(1); //If Agent training use unscaled time
            else*/
                yield return new WaitForSeconds(1); //Else use regular scaled time that allows pauses

            currentSeconds--;

            ts = TimeSpan.FromSeconds(currentSeconds);
            timerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);
        }

        //After Timer End
        if (levelType == LevelType.AgentTraining)
        {
            enemyAgent.EndEpisode();
        }

        else if (levelType == LevelType.AgentEvaluation)
        {
            roundScores[modelRound] = enemyAgent.GetScore(); //Save enemy score of this round

            Debug.Log("Agent Evaluation - ModelNum: " + currentModel + " RoundNum: " + (modelRound + 1) + " Score: " + roundScores[modelRound]);

            modelRound++;
            if (modelRound >= 3) //Play 3 rounds on a single model
            {
                double average = roundScores.Average(); //Calculate avg of 3 rounds

                modelRound = 0;

                Debug.Log("Writing Evaluation");

                sb.Append('\n')
                    .Append(currentModel.ToString()).Append(',')
                    .Append(evaluationModels[currentModel].name).Append(',')
                    .Append(roundScores[0]).Append(',')
                    .Append(roundScores[1]).Append(',')
                    .Append(roundScores[2]).Append(',')
                    .Append(average).Append(',');

                currentModel++;
            }
            //currentModel++; //set next model to evaluate


            if (currentModel >= evaluationModels.Length) //If reached end of list save file and exit playmode
            {
                SaveToFile(sb.ToString());

                EditorApplication.ExitPlaymode();
            }
            else
            {
                //enemyAgent.ResetLevelAndAgent();
                itemSpawner.ResetLevel(enemyAgent.gameObject, true);
            }
        }

        else //Player Level
        {
            isGameFinished = true;
            StartCoroutine(FinishGame());
        }
    }

    private IEnumerator FinishGame()
    {
        enemyAgent.enabled = false; //Disable enemy script to stop movement

        if (int.Parse(playerScoreText.text) > int.Parse(enemyScoreText.text)) //Win
            endGameHeader.text = "YOU WIN!";

        else if (int.Parse(playerScoreText.text) < int.Parse(enemyScoreText.text)) //Lose
            endGameHeader.text = "YOU LOSE!";

        else //Draw
            endGameHeader.text = "DRAW REACHED!";

        endGamePlayerScore.text = playerScoreText.text;
        endGameEnemyScore.text = enemyScoreText.text;

        endGamePanel.SetActive(true);
        foreach (GameObject textObject in endGameTextObjects)
        {
            yield return new WaitForSeconds(2);
            textObject.SetActive(true);
        }

    }

    public void SaveToFile(string toSave)
    {
#if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
#else
    var folder = Application.persistentDataPath;
#endif

        var filePath = Path.Combine(folder, "modelEvaluation.csv");

        using (var writer = new StreamWriter(filePath, false))
        {
            writer.Write(toSave);
        }

        // Or just
        //File.WriteAllText(content);

        Debug.Log($"CSV file written to \"{filePath}\"");

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
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
