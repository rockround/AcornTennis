using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameRule : MonoBehaviour
{
    public float totalTime = 120;
    public Slider slider;
    Vector3 centerField1 = new Vector3(-3.92f,.25f,-8.21f);
    Vector3 extentField1 = new Vector3(6,.5f,8);
    Vector3 centerField2 = new Vector3(-3.92f,.25f,8.23f);
    Vector3 extentField2 = new Vector3(6,.5f,8);
    public Collider field1, field2;
    //Modified publicly by acorns as they spawn and die
    public List<GameObject> acorns;

    internal int field1Acorns;
    internal int field2Acorns;

    public TMP_Text team1Acorns, team2Acorns;
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
    internal void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name.Contains("Tree"))
            return;
        if((other.transform.position - centerField1).magnitude < (other.transform.position - centerField2).magnitude )
        {
            field1Acorns++;
            team1Acorns.text = field1Acorns+ "";
        }
        else
        {
            field2Acorns++;
            team2Acorns.text = field2Acorns + "";
        }
    }
    internal void OnTriggerExit(Collider other)
    {
        if ((other.transform.position - centerField1).magnitude < (other.transform.position - centerField2).magnitude)
        {
            field1Acorns--;
            team1Acorns.text = field1Acorns + "";
        }
        else
        {
            field2Acorns--;
            team2Acorns.text = field2Acorns + "";
        }
    }
}
