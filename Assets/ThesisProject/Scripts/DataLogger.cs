using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Unity.Profiling;
using UnityEngine.Profiling;

public class DataLogger : MonoBehaviour
{
    public string gameGUID;

    //Player vars
    int p_woodInteraction;
    int p_ironInteraction;
    int p_goldInteraction;
    int p_baseInteraction;
    int p_travelTime;

    //Enemy vars
    int e_woodInteraction;
    int e_ironInteraction;
    int e_goldInteraction;
    int e_baseInteraction;
    int e_travelTime;

    //Program/Computer Performance
    Recorder frameTimeRecorder;
    ProfilerRecorder memoryUsageRecorder;

    //Resulting strings
    StringBuilder gameSummary;
    StringBuilder playerLog;
    StringBuilder enemyLog;
    StringBuilder performanceSummary;

    // Start is called before the first frame update
    void Start()
    {
        gameGUID = Guid.NewGuid().ToString();

        frameTimeRecorder = Recorder.Get("BehaviourUpdate"); frameTimeRecorder.enabled = true;
        memoryUsageRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");

        gameSummary = new StringBuilder(",Player,Enemy");

        performanceSummary = new StringBuilder("Time (seconds), Frame Time (ns), Memory Usage %");

        playerLog = new StringBuilder();
        enemyLog = new StringBuilder();
    }

    public void LogResourceInteraction(bool isPlayer, Resource.Type resource)
    {
        Debug.Log("Logging resource interaction");
        switch (resource)
        {
            case Resource.Type.Wood:
                if (isPlayer) p_woodInteraction++;
                else e_woodInteraction++;
                break;
            case Resource.Type.Iron:
                if (isPlayer) p_ironInteraction++;
                else e_ironInteraction++;
                break;
            case Resource.Type.Gold:
                if (isPlayer) p_goldInteraction++;
                else e_goldInteraction++;
                break;
            default:
                break;
        }

        TimeLog(GetName(isPlayer) + " mining " + resource + " resource",isPlayer);
    }

    public void LogBaseDeposit(bool isPlayer)
    {
        Debug.Log("Logging base deposit");
        if (isPlayer) p_baseInteraction++;
        else e_baseInteraction++;

        TimeLog(GetName(isPlayer) + " depositing at base",isPlayer);
    }

    public void LogMovement(bool isPlayer)
    {
        if (isPlayer)
            p_travelTime++;
        else
            e_travelTime++;
    }

    private void TimeLog(string message, bool isPlayer)
    {
        string logMessage = DateTime.Now + " - " + message;

        if (isPlayer)
            playerLog.Append(logMessage).Append('\n');
        else
            enemyLog.Append(logMessage).Append('\n');
    }

    private string GetName(bool isPlayer)
    {
        if (isPlayer) return "Player";
        else return "Enemy";
    }

    public void LogPerformance(int time)
    {
        performanceSummary.Append("\n")
            .Append(time + ",").Append(GetFrameTime() + ",").Append(GetMemoryUsage());

        Debug.Log("Frame count: "+Time.frameCount+" Total Frame Time: " + GetFrameTime() + " Memory Usage :" + GetMemoryUsage());
    }

    private string GetFrameTime()
    {
        return frameTimeRecorder.elapsedNanoseconds.ToString();
    }

    private string GetMemoryUsage()
    {
        return (memoryUsageRecorder.LastValue / (1024 * 1024)).ToString();
    }

    public void SaveLogs(string playerScore, string enemyScore)
    {
        Debug.Log("Saving logs with id: " + gameGUID);

        string path;

#if UNITY_EDITOR
        path = Application.streamingAssetsPath+"/"+"Data_"+gameGUID; //Rework for build
#else
        path = "E:\ThesisResults"+"/"+"Data_"+gameGUID;
#endif

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        using (var writer = new StreamWriter(path+"/playerLog.txt",false)) //Save player log
        {
            writer.Write(playerLog);
        }

        using (var writer = new StreamWriter(path + "/enemyLog.txt", false)) //Save enemy log
        {
            writer.Write(enemyLog);
        }

        string difficulty = GameObject.Find("DifficultySetting").GetComponent<DifficultySetting>().chosenDifficulty.ToString();

        gameSummary.Append('\n') //Write all values to summary file
            .Append("Wood resource interactions,").Append(p_woodInteraction + ",").Append(e_woodInteraction).Append('\n')
            .Append("Iron resource interactions,").Append(p_ironInteraction + ",").Append(e_ironInteraction).Append('\n')
            .Append("Gold resource interactions,").Append(p_goldInteraction + ",").Append(e_goldInteraction).Append('\n')
            .Append("Base deposit interactions,").Append(p_baseInteraction + ",").Append(e_baseInteraction).Append('\n')
            .Append("Time Spent Travelling (seconds),").Append(p_travelTime + ",").Append(e_travelTime).Append('\n')
            .Append("Final score,").Append(playerScore + ",").Append(enemyScore).Append('\n').Append('\n') //Skip 2 lines
            .Append("Difficulty,").Append(difficulty);

        using (var writer = new StreamWriter(path+"/gameSummary.csv", false)) //Save game summary
        {
            writer.Write(gameSummary);
        }


        using (var writer = new StreamWriter(path + "/performanceSummary.csv", false)) //Save performance summary
        {
            writer.Write(performanceSummary);
        }

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
