using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private DifficultySetting difficultySetting;

    [SerializeField] private GameObject difficultySection;
    [SerializeField] private Button PlayButton;

    public void OpenDifficulties()
    {
        PlayButton.interactable = false;
        difficultySection.SetActive(true);
    }

    public void SetDifficultyAndPlay(int difficulty)
    {
        difficultySetting = GameObject.Find("DifficultySetting").GetComponent<DifficultySetting>();
        difficultySetting.chosenDifficulty = (DifficultySetting.difficultySetting) difficulty; 
        SceneManager.LoadScene("PlayerLevel");
    }

    public void GoToTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
