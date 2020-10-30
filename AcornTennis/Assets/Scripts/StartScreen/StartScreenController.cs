using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenController : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject playMenuPanel;
    public GameObject settingsPanel;
    public GameObject aboutPanel;

    GameObject currentMenu;
    GameObject previousMenu;
    // Start is called before the first frame update
    void Start()
    {
        currentMenu = previousMenu = startPanel;   
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
