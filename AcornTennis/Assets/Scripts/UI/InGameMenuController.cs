using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuController : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject inGameOverlay;
    public GameObject gameEndPanel;
    public GameObject controlsPanel;
    public int playerTeamID = 1;
    public MonoBehaviour[] toToggleOnMenu;
    public GameObject[] toToggleOnMenuGO;

    GameObject currentPanel;
    // Start is called before the first frame update
    void Start()
    {
        currentPanel = menuPanel;
    }

    bool currentlyOpened = false;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!currentlyOpened)
            {
                //load menu
                currentlyOpened = true;
                foreach (MonoBehaviour thing in toToggleOnMenu)
                {
                    thing.enabled = true;
                }
                foreach (GameObject thing in toToggleOnMenuGO)
                {
                    thing.SetActive(true);
                }
                Time.timeScale = 0;
            }
            else
            {
                //close menu
                currentlyOpened = false;
                foreach(MonoBehaviour thing in toToggleOnMenu)
                {
                    thing.enabled = false;
                }
                foreach(GameObject thing in toToggleOnMenuGO)
                {
                    thing.SetActive(false);
                }
                Time.timeScale = 1;
            }
        }
    }
    internal void GameEnded(int winningTeam)
    {
        inGameOverlay.SetActive(false);
        gameEndPanel.SetActive(true);
        bool won = playerTeamID == winningTeam;
        if (won)
        {
            gameEndPanel.GetComponentInChildren<TMP_Text>().text = "Victory";
            int currentStage = StaticInfoContainer.currentStage;
            if(currentStage == 0)
            {
                if (StaticInfoContainer.currentDifficulty > StaticInfoContainer.difficultyProgressUrban)
                {
                    //StaticInfoContainer.difficultyProgressUrban = StaticInfoContainer.currentDifficulty;
                    PlayerPrefs.SetInt("StageProgressUrban", StaticInfoContainer.currentDifficulty);
                }
            }
            else if(currentStage == 1)
            {
                if (StaticInfoContainer.currentDifficulty > StaticInfoContainer.difficultyProgressSuburban)
                {
                   // StaticInfoContainer.difficultyProgressSuburban = StaticInfoContainer.currentDifficulty;
                    PlayerPrefs.SetInt("StageProgressSuburban", StaticInfoContainer.currentDifficulty);
                }
            }
            else
            {
                if (StaticInfoContainer.currentDifficulty > StaticInfoContainer.difficultyProgressRural)
                {
                    //StaticInfoContainer.difficultyProgressRural = StaticInfoContainer.currentDifficulty;
                    PlayerPrefs.SetInt("StageProgressRural", StaticInfoContainer.currentDifficulty);
                }
            }

        }
        else
        {
            gameEndPanel.GetComponentInChildren<TMP_Text>().text = "Defeat";
        }
    }
    public void OnReturnButton()
    {
        SceneManager.LoadScene(0);
    }
    public void OnExitButton()
    {
        Application.Quit();
    }
    public void OnRestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void OnControlsButton()
    {
        currentPanel.SetActive(false);
        currentPanel = controlsPanel;
        currentPanel.SetActive(true);
    }
    public void OnBackButton()
    {
        currentPanel.SetActive(false);
        currentPanel = menuPanel;
        currentPanel.SetActive(true);
    }
}
