using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultySetting : MonoBehaviour
{

    public static DifficultySetting _instance;


    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != null)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
        //myData = GetComponent<SaveLoadData>();
    }

    //public int difficultySetting = 0;

    public enum difficultySetting
    {
        Easy,
        Medium,
        Hard,
        Auto
    }

    public difficultySetting chosenDifficulty = difficultySetting.Easy; //Easy by default

}
