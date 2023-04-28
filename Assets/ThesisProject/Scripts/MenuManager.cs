using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private DifficultySetting difficultySetting;

    [SerializeField] private GameObject difficultySection;
    [SerializeField] private Button PlayButton;
    [SerializeField] private TMP_InputField filePathInput;

    private void Start()
    {
        difficultySetting = DifficultySetting._instance;
        if(difficultySetting.resultsPath.Length > 1)
        {
            Debug.Log("Results path: " + difficultySetting.resultsPath);
            filePathInput.text = difficultySetting.resultsPath;
        }
    }

    public void OpenDifficulties()
    {
        PlayButton.interactable = false;
        difficultySection.SetActive(true);
    }

    public void SetDifficultyAndPlay(int difficulty)
    {
        difficultySetting = DifficultySetting._instance;
        difficultySetting.chosenDifficulty = (DifficultySetting.difficultySetting) difficulty;
        difficultySetting.resultsPath = filePathInput.text;
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
