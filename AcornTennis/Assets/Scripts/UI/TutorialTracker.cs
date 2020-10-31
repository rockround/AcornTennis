using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTracker : MonoBehaviour
{
    bool moveOn = false;
    internal const int MOVEUPTODOWN = 0, SHIFT = 1, TARGET = 2, HIT = 3, UPDRAFT = 4, CLICK= 5, MOVEUPTOUP = 6;
    internal int need = CLICK;
    public GameObject intro, moveUpToHanging, hitShift, targetAcorn, hitAcorn, moveUpToFallen, useUpdraft, conclusion;
    public GameObject panel;
    // Start is called before the first frame update
    void Start()
    {
        if (StaticInfoContainer.showTutorial)
            StartCoroutine(tutorialController());
        else
        {
            Destroy(panel);
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && need == CLICK)
            moveOn = true;
    }

    internal void continueTutorial(int step)
    {
        if (need == step)
            moveOn = true;
    }
    IEnumerator tutorialController()
    {

        //At beginning wait for player to confirm
        panel.SetActive(true);

        Time.timeScale = 0;
        intro.SetActive(true);
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        intro.SetActive(false);


        //Wait for menu confirmation
        moveUpToHanging.SetActive(true);
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        moveUpToHanging.SetActive(false);
        Time.timeScale = 1;
        panel.SetActive(false);


        //First wait until player moves up to hanging acorn
        panel.SetActive(false);
        need = MOVEUPTOUP;

        yield return new WaitUntil(() => moveOn);
        moveOn = false;

        need = CLICK;
        panel.SetActive(true);

        //Wait for menu confirmation
        Time.timeScale = 0;
        hitShift.SetActive(true);
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        hitShift.SetActive(false);
        Time.timeScale = 1;
        panel.SetActive(false);

        need = SHIFT;

        //Next wait until player hits shift
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        need = CLICK;
        panel.SetActive(true);


        //Wait for menu confirmation
        Time.timeScale = 0;
        targetAcorn.SetActive(true);
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        targetAcorn.SetActive(false);
        Time.timeScale = 1;
        panel.SetActive(false);

        need = TARGET;

        //Next wait until acorn targeted successfully
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        need = CLICK;
        panel.SetActive(true);

        //Wait for menu confirmation
        Time.timeScale = 0;
        hitAcorn.SetActive(true);
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        hitAcorn.SetActive(false);
        Time.timeScale = 1;
        panel.SetActive(false);

        need = HIT;

        //Next wait until player hits acorn
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        need = CLICK;
        panel.SetActive(true);

        //Wait for menu confirmation
        Time.timeScale = 0;
        moveUpToFallen.SetActive(true);
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        moveUpToFallen.SetActive(false);
        Time.timeScale = 1;
        panel.SetActive(false);

        need = MOVEUPTODOWN;

        //Next wait until player goes up to a fallen acorn
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        need = CLICK;
        panel.SetActive(true);

        //Wait for menu confirmation
        Time.timeScale = 0;
        useUpdraft.SetActive(true);
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        useUpdraft.SetActive(false);
        Time.timeScale = 1;
        panel.SetActive(false);

        need = UPDRAFT;

        //Next wait until player hits E
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        need = CLICK;
        panel.SetActive(true);

        //Wait for menu confirmation
        Time.timeScale = 0;
        conclusion.SetActive(true);
        yield return new WaitUntil(() => moveOn);
        moveOn = false;
        conclusion.SetActive(false);
        Time.timeScale = 1;
        panel.SetActive(false);

        //Tutorial Complete
        StaticInfoContainer.showTutorial = false;
    }
}
