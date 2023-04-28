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
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public EnemyAgent enemyAgent; //EnemyAgent script, assignned by ItemSpawner

    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI enemyScoreText;

    [SerializeField] private TextMeshProUGUI timerText;
    public int timerTotalSeconds;
    public int timerSecondsLeft;

    //public bool agentTrainingLevel;
    //public bool agentEvaluationLevel;
    [Space(10)]
    [Header("Agent Model Evaluation")]
    [SerializeField] private NNModel[] models;
    [SerializeField] private string modelPath;

    private int currentModel = 0;
    private int modelRound = 0;
    public int maxEvalRounds = 10;
    private int[] roundScores;
    StringBuilder sb;

    public ItemSpawner itemSpawner;

    private DifficultySetting difficultySetting;

    private DataLogger dataLogger;

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
        if(levelType == LevelType.AgentEvaluation || levelType == LevelType.PlayerLevel)
        {
            models = Resources.LoadAll(modelPath, typeof(NNModel)).Cast<NNModel>().ToArray();
        }

        if (levelType == LevelType.AgentEvaluation)
        {
            roundScores = new int[maxEvalRounds];

            sb = new StringBuilder("ModelNumber,ModelName,");

            for (int i = 0; i < maxEvalRounds; i++)
            {
                sb.Append("Round" + (i + 1) + "Score,");
            }
            sb.Append("AvgScore");
        }

        else if(levelType == LevelType.PlayerLevel)
        {
            //difficultySetting = GameObject.Find("DifficultySetting").GetComponent<DifficultySetting>();
            difficultySetting = DifficultySetting._instance;
            sb = new StringBuilder();

            dataLogger = GameObject.Find("DataLogger").GetComponent<DataLogger>();
        }


        StartCoroutine(Timer());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
        }
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

        timerSecondsLeft = timerTotalSeconds;
        var ts = TimeSpan.FromSeconds(timerSecondsLeft);
        timerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);


        if (levelType == LevelType.AgentEvaluation) //If is evaluation level, set agent brain to correct one to evaluate
        {

            while (!itemSpawner) //Fixes error in Agent Evaluation where Item Spawner hasn't finished so enemyAgent = null
            {
                yield return null;
            }
            enemyAgent.SetModel("ResourceAgent", models[currentModel]);
        }

        else if(levelType == LevelType.PlayerLevel)
        {
            if((int)difficultySetting.chosenDifficulty < 3) //3 refers to Auto difficulty
            {
                enemyAgent.SetModel("ResourceAgent", models[(int)difficultySetting.chosenDifficulty]);
            }
        }

        while (timerSecondsLeft > 0)
        {
            /*if (levelType == LevelType.AgentTraining)
                yield return new WaitForSecondsRealtime(1); //If Agent training use unscaled time
            else*/
                yield return new WaitForSeconds(1); //Else use regular scaled time that allows pauses

            timerSecondsLeft--;

            ts = TimeSpan.FromSeconds(timerSecondsLeft);
            timerText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);

            if(levelType == LevelType.PlayerLevel) //if player level, log info
            {
                dataLogger.LogPerformance(timerTotalSeconds-timerSecondsLeft); //Log performance
            }
        }

        //After Timer End
        if (levelType == LevelType.AgentTraining)
        {
            enemyAgent.SetTensorScore(); //Update episode-end score on TensorBoard
            enemyAgent.EndEpisode();
        }

        else if (levelType == LevelType.AgentEvaluation)
        {
            roundScores[modelRound] = enemyAgent.GetScore(); //Save enemy score of this round

            Debug.Log("Agent Evaluation - ModelNum: " + currentModel + " RoundNum: " + (modelRound + 1) + " Score: " + roundScores[modelRound]);

            modelRound++;
            if (modelRound >= maxEvalRounds) //Play x amount of rounds on a single model
            {
                double average = roundScores.Average(); //Calculate avg of all rounds

                modelRound = 0;

                Debug.Log("Writing Evaluation");

                sb.Append('\n')
                    .Append(currentModel.ToString()).Append(',')
                    .Append(models[currentModel].name).Append(',');

                for (int i = 0; i < maxEvalRounds; i++)
                {
                    sb.Append(roundScores[i]).Append(',');
                }

                sb.Append(average).Append(',');

                currentModel++;
            }
            //currentModel++; //set next model to evaluate


            if (currentModel >= models.Length) //If reached end of list save file and exit playmode
            {
                SaveToFile(sb.ToString(),true);
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#endif
            }
            else
            {
                //enemyAgent.ResetLevelAndAgent();
                itemSpawner.ResetLevel(enemyAgent.gameObject, true);
            }
        }

        else //Player Level
        {
            sb.Append(playerScoreText.text).Append(',')
                .Append(enemyScoreText.text);
            isGameFinished = true;
            StartCoroutine(FinishGame());

            SaveToFile(sb.ToString(), false);

            dataLogger.SaveLogs(playerScoreText.text,enemyScoreText.text);
        }
    }

    #region Log methods
    // These methods link to ones in DataLogger.cs. They are accessed from here since levelType needs to be checked
    public void LogResourceInteraction(bool isPlayer, Resource.Type resource)
    {
        if (levelType == LevelType.PlayerLevel)
        {
            dataLogger.LogResourceInteraction(isPlayer, resource);
        }
    }

    public void LogBaseInteraction(bool isPlayer)
    {
        if (levelType == LevelType.PlayerLevel)
        {
            dataLogger.LogBaseDeposit(isPlayer);
        }
    }

    public void LogMovement(bool isPlayer)
    {
        if (levelType == LevelType.PlayerLevel)
        {
            dataLogger.LogMovement(isPlayer);
        }
    }
    #endregion


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

    public void QuitGame()
    {
        Debug.Log("Quitting to Main Menu");
        SceneManager.LoadScene("MainMenu");
    }

    public void SaveToFile(string toSave, bool evaluation)
    {
#if UNITY_EDITOR
        var folder = Application.streamingAssetsPath;
#else
    var folder = difficultySetting.resultsPath;
#endif
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string filePath;
        if(evaluation) filePath = Path.Combine(folder, "modelEvaluation.csv");
        else filePath = Path.Combine(folder, difficultySetting.chosenDifficulty+".csv");

        if (evaluation) //if agent evaluation, just write file
        {
            using (var writer = new StreamWriter(filePath, false))
            {
                writer.Write(toSave);
            }
        }
        else //if player level, check if file exists and create or append
        {
            if (!File.Exists(filePath)) //Create header if creating new file
            {
                string header = "Game ID, Player Score, Enemy Score";
                File.WriteAllText(filePath, header);
            }
            var lines = File.ReadAllLines(filePath);
            //var count = lines.Length; //count lines to get participant number

            string playerEntry = Environment.NewLine + dataLogger.gameGUID+","+toSave; //Save game Id + scores received

            File.AppendAllText(filePath, playerEntry); //Append results
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
