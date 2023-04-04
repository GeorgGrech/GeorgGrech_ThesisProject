using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private DifficultySetting difficultySetting;

    [SerializeField] private GameObject difficultySection;
    [SerializeField] private Button PlayButton;
    // Start is called before the first frame update
    void Start()
    {
        difficultySetting = GameObject.Find("DifficultySetting").GetComponent<DifficultySetting>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenDifficulties()
    {
        PlayButton.interactable = false;
        difficultySection.SetActive(true);
    }

    public void SetDifficultyAndPlay(int setting)
    {
        difficultySetting.difficultySetting = setting;
        SceneManager.LoadScene("PlayerLevel");
    }
}
