using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameRule : MonoBehaviour
{
    public float totalTime = 120;
    public Slider slider;
    Vector3 centerField1 = new Vector3(-3.92f,.1f,-8.21f);
    Vector3 extentField1 = new Vector3(7,.1f,8);
    Vector3 centerField2 = new Vector3(-3.92f,.1f,8.23f);
    Vector3 extentField2 = new Vector3(7,.1f,8);

    //Modified publicly by acorns as they spawn and die
    public List<GameObject> acorns;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(timer(totalTime));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator timer(float timePeriod)
    {
        float startTime = Time.realtimeSinceStartup;
        float endTime = startTime + timePeriod;
        while (Time.realtimeSinceStartup < endTime)
        {
            float progress = (Time.realtimeSinceStartup - startTime) / timePeriod;
            slider.value = progress;
            yield return new WaitForSecondsRealtime(.05f);
        }
        Collider[] team1Colliders = Physics.OverlapBox(centerField1, extentField1, Quaternion.identity,1<<8);
        Collider[] team2Colliders = Physics.OverlapBox(centerField2, extentField2, Quaternion.identity, 1 << 8);
        print(team1Colliders.Length + " " + team2Colliders.Length);
        if(team1Colliders.Length > team2Colliders.Length)
        {
            print("Team 2 wins");
        }
        else
        {
            print("Team 1 wins");
        }
    }
}
