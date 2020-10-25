using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMotion : MonoBehaviour
{
    bool windBack = false;
    Vector3 rotationIncrementEuler;
    Quaternion neutralRotation;
    public float swingDuration;
    // Start is called before the first frame update
    void Start()
    {
        neutralRotation = transform.localRotation;
        StartCoroutine(swing());
    }

    internal void swing(Vector3 rotationEuler)
    {
        if (!windBack)
        {
            rotationIncrementEuler = rotationEuler;
            windBack = true;
        }
    }
    internal void release()
    {
        rotationIncrementEuler = Vector3.zero;
        windBack = false;
    }
    IEnumerator swing()
    {
        while (true)
        {
            //Wait for next swing
            while (!windBack)
            {
                yield return null;
            }


            while (windBack)
            {
                transform.localRotation *= Quaternion.Euler(rotationIncrementEuler);
                yield return null;
            }

            float startTime = Time.fixedTime;
            float endTime = startTime + swingDuration;
            Quaternion fromRotation = transform.localRotation;

            while (Time.fixedTime < endTime)
            {
                float progress = (Time.fixedTime - startTime) / swingDuration;
                transform.localRotation = Quaternion.Slerp(fromRotation, neutralRotation, progress);
                yield return new WaitForFixedUpdate();
            }

        }
    }
}
