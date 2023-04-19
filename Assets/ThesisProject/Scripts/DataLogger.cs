using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class DataLogger : MonoBehaviour
{
    private string gameGUID;

    //Player vars
    int p_woodInteraction;
    int p_ironInteraction;
    int p_goldInteraction;
    int p_baseInteraction;

    //Enemy vars
    int e_woodInteraction;
    int e_ironInteraction;
    int e_goldInteraction;
    int e_baseInteraction;

    StringBuilder summary;
    StringBuilder playerLog;
    StringBuilder enemyLog;

    // Start is called before the first frame update
    void Start()
    {
        gameGUID = Guid.NewGuid().ToString();

        string difficulty = GameObject.Find("DifficultySetting").GetComponent<DifficultySetting>().chosenDifficulty.ToString();

        summary = new StringBuilder(",Difficulty,"+difficulty+" \n " +
            ",Player,Enemy");

    }

    public void LogResourceInteraction(bool isPlayer, Resource.Type resource)
    {
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

        TimeLog(GetName(isPlayer) + " mining " + resource + " resource.",isPlayer);
    }

    public void LogBaseDeposit(bool isPlayer)
    {
        if (isPlayer) p_baseInteraction++;
        else e_baseInteraction++;

        TimeLog(GetName(isPlayer) + " depositing at base",isPlayer);
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

    public void SaveLogs(string playerScore, string enemyScore)
    {
        var path = Application.streamingAssetsPath+"/"+"Data_"+gameGUID; //Rework for build

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

        summary.Append('\n') //Write all values to summary file
            .Append("Wood resource interactions,").Append(p_woodInteraction + ",").Append(e_woodInteraction + ",").Append('\n')
            .Append("Iron resource interactions,").Append(p_ironInteraction + ",").Append(e_ironInteraction + ",").Append('\n')
            .Append("Gold resource interactions,").Append(p_goldInteraction + ",").Append(e_goldInteraction + ",").Append('\n')
            .Append("Base deposit interactions,").Append(p_baseInteraction + ",").Append(e_baseInteraction + ",").Append('\n')
            .Append("Final score,").Append(playerScore+",").Append(enemyScore);

        using (var writer = new StreamWriter(path+"/gameSummary.csv", false)) //Save summary
        {
            writer.Write(summary);
        }

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
    }
}
