﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreenController : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject playMenuPanel;
    public GameObject settingsPanel;
    public GameObject aboutPanel;

    GameObject currentMenu;
    GameObject previousMenu;

    public GameObject urbanStages;
    public GameObject suburbanStages;
    public GameObject ruralStages;

    // Start is called before the first frame update
    void Start()
    {
        currentMenu = previousMenu = startPanel;
        StaticInfoContainer.difficultyProgressUrban = PlayerPrefs.GetInt("StageProgressUrban", -1);
        StaticInfoContainer.difficultyProgressSuburban = PlayerPrefs.GetInt("StageProgressSuburban", -1);
        StaticInfoContainer.difficultyProgressRural = PlayerPrefs.GetInt("StageProgressRural", -1);

        StaticInfoContainer.useDiscrete = PlayerPrefs.GetInt("UseDiscrete", 1)==1;
        StaticInfoContainer.hideMouseDefault = PlayerPrefs.GetInt("HideMouseDefault", 1)==1;

        StaticInfoContainer.showTutorial = PlayerPrefs.GetInt("ShowTutorial", 1)==1;

        if (StaticInfoContainer.showTutorial)
        {
            suburbanStages.transform.GetChild(0).GetComponent<Button>().interactable = false;
            ruralStages.transform.GetChild(0).GetComponent<Button>().interactable = false;
        }
        else
        {
           //if (StaticInfoContainer.difficultyProgressUrban >= 0)
            {
                for (int i = 1; i < 3; i++)
                {
                    urbanStages.transform.GetChild(i).GetComponent<Button>().interactable = true;
                }
            }
            //if (StaticInfoContainer.difficultyProgressSuburban >= 0)
            {
                for (int i = 1; i < 3; i++)
                {
                    suburbanStages.transform.GetChild(i).GetComponent<Button>().interactable = true;
                }
            }
            //if (StaticInfoContainer.difficultyProgressRural >= 0)
            {
                for (int i = 1; i < 3; i++)
                {
                    ruralStages.transform.GetChild(i).GetComponent<Button>().interactable = true;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnToggleConstrainHit(bool state)
    {
        StaticInfoContainer.useDiscrete = state;
        PlayerPrefs.SetInt("UseDiscrete", state ? 1 : 0);
    }
    public void OnToggleAutoHide(bool state)
    {
        StaticInfoContainer.hideMouseDefault = state;
        PlayerPrefs.SetInt("HideMouseDefault", state ? 1 : 0);
    }
    public void OnPlay()
    {
        previousMenu = currentMenu;
        currentMenu = playMenuPanel;
        currentMenu.SetActive(true);
        previousMenu.SetActive(false);
    }
    public void OnPlaySelectLevel(int level)
    {
        int scene = level / 3;//Goes from 0-2
        int difficulty = level % 3;//Goes from 0-2
        StaticInfoContainer.currentDifficulty = difficulty;
        StaticInfoContainer.currentStage = scene;
        SceneManager.LoadScene(scene + 1);
    }
    public void OnBack()
    {
        currentMenu.SetActive(false);
        previousMenu.SetActive(true);
        currentMenu = startPanel;
        previousMenu =currentMenu;
    }
    public void OnSettings()
    {
        previousMenu = currentMenu;
        currentMenu = settingsPanel;
        currentMenu.SetActive(true);
        previousMenu.SetActive(false);
    }
    public void OnAbout()
    {
        previousMenu = currentMenu;
        currentMenu = aboutPanel;
        currentMenu.SetActive(true);
        previousMenu.SetActive(false);
    }
    public void OnReplayTutorial()
    {
        StaticInfoContainer.showTutorial = true;
        int scene = 0;
        int difficulty = 0;
        StaticInfoContainer.currentDifficulty = difficulty;
        StaticInfoContainer.currentStage = scene;
        SceneManager.LoadScene(scene + 1);
    }
    public void OnExitButton()
    {
        Time.timeScale = 1;
        Application.Quit();
    }
}
