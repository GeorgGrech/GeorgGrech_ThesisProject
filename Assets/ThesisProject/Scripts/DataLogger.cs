using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Unity.Profiling;
using UnityEngine.Profiling;
using System.Threading;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Linq;

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
    ProfilerRecorder memoryUsageRecorder; //Memory Usage in MB
    public float CpuUsage; //Percentage

    private Thread _cpuThread;
    private float _lasCpuUsage;
    public int processorCount;

    //Resulting strings
    StringBuilder gameSummary;
    StringBuilder playerLog;
    StringBuilder enemyLog;
    StringBuilder performanceSummary;

    private DifficultySetting difficultySetting;

    // Start is called before the first frame update
    void Start()
    {
        difficultySetting = DifficultySetting._instance;

        gameGUID = Guid.NewGuid().ToString();

        memoryUsageRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory");

        gameSummary = new StringBuilder(",Player,Enemy");

        performanceSummary = new StringBuilder("Time (seconds), Cpu Usage (%), Memory Usage (MB)");

        playerLog = new StringBuilder();
        enemyLog = new StringBuilder();

        Application.runInBackground = true;
        processorCount = SystemInfo.processorCount;
        _cpuThread = new Thread(UpdateCPUUsage)
        {
            IsBackground = true,
            // we don't want that our measurement thread
            // steals performance
            Priority = System.Threading.ThreadPriority.BelowNormal
        };

        // start the cpu usage thread
        _cpuThread.Start();
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
            .Append(time + ",").Append(GetCPUUsage() + ",").Append(GetMemoryUsage());

        Debug.Log("CPU Usage: " + GetCPUUsage() + " Memory Usage :" + GetMemoryUsage());
    }

    private string GetCPUUsage()
    {
        return Math.Round(CpuUsage,2).ToString();
    }

    private string GetMemoryUsage()
    {
        return (memoryUsageRecorder.LastValue / (1024 * 1024)).ToString();
    }

    private void UpdateCPUUsage()
    {
        var lastCpuTime = new TimeSpan(0);

        // This is ok since this is executed in a background thread
        while (true)
        {
            var cpuTime = new TimeSpan(0);

            // Get a list of all running processes in this PC
            var AllProcesses = Process.GetProcesses();

            // Sum up the total processor time of all running processes
            cpuTime = AllProcesses.Aggregate(cpuTime, (current, process) => current + process.TotalProcessorTime);

            // get the difference between the total sum of processor times
            // and the last time we called this
            var newCPUTime = cpuTime - lastCpuTime;

            // update the value of _lastCpuTime
            lastCpuTime = cpuTime;

            // The value we look for is the difference, so the processor time all processes together used
            // since the last time we called this divided by the time we waited
            // Then since the performance was optionally spread equally over all physical CPUs
            // we also divide by the physical CPU count
            CpuUsage = 100f * (float)newCPUTime.TotalSeconds / 1 / processorCount;

            // Wait for UpdateInterval
            Thread.Sleep(Mathf.RoundToInt(1 * 1000));
        }
    }

    public void SaveLogs(string playerScore, string enemyScore)
    {
        Debug.Log("Saving logs with id: " + gameGUID);

        string path;

#if UNITY_EDITOR
        path = Application.streamingAssetsPath+"/"+"Data_"+gameGUID; //Rework for build
#else
        path = difficultySetting.resultsPath+"/"+"Data_"+gameGUID;
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

        string difficulty = difficultySetting.chosenDifficulty.ToString();

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
