using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameRule : MonoBehaviour
{
    public float totalTime = 120;
    public Slider slider;
    Vector3 centerField1 = new Vector3(-3.92f, .25f, -8.21f);
    Vector3 extentField1 = new Vector3(6, .5f, 8);
    Vector3 centerField2 = new Vector3(-3.92f, .25f, 8.23f);
    Vector3 extentField2 = new Vector3(6, .5f, 8);
    public Collider field1, field2;
    //Modified publicly by acorns as they spawn and die
    public List<GameObject> acorns;

    internal int field1Acorns;
    internal int field2Acorns;
    public InGameMenuController menuController;
    public TMP_Text team1Acorns, team2Acorns;

    public AudioSource soundOfTime;
    float startSoundVolume;
    // Start is called before the first frame update
    void Start()
    {
        startSoundVolume = soundOfTime.volume;
        StartCoroutine(timer(totalTime));
    }

    // Update is called once per frame
    void Update()
    {

    }
    IEnumerator timer(float timePeriod)
    {
        float startTime = Time.time;
        float endTime = startTime + timePeriod;
        while (Time.time < endTime)
        {
            float progress = (Time.time - startTime) / timePeriod;
            slider.value = progress;
            soundOfTime.volume = startSoundVolume * progress;
            yield return new WaitForSecondsRealtime(.05f);
        }
        soundOfTime.Stop();

        if (field1Acorns > field2Acorns)
        {
            menuController.GameEnded(2);
        }
        else
        {
            menuController.GameEnded(1);
        }
    }
    internal void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name.Contains("Tree"))
            return;
        if ((other.transform.position - centerField1).magnitude < (other.transform.position - centerField2).magnitude)
        {
            field1Acorns++;
            team1Acorns.text = field1Acorns + "";
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
