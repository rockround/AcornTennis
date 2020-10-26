using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMotion : MonoBehaviour
{
    bool windBack = false;
    Quaternion finalRight;
    Quaternion targetRotation;
    Quaternion neutralRotation;
    Quaternion neutralBatRotation;
    public Transform swingObject;
    public float rotationScale;
    public float swingVelocity;
    public float windVelocity;

    // Start is called before the first frame update
    void Start()
    {
        neutralRotation = transform.localRotation;
        neutralBatRotation = swingObject.localRotation;
        StartCoroutine(swing());
    }

    internal void swing(Vector3 rotationEuler, Vector3 finalRightDirection)
    {
        if (!windBack)
        {
            int sign =  rotationEuler.y > 0 ? -1 : 1;
            finalRight = Quaternion.FromToRotation(sign * Vector3.right, finalRightDirection);// * neutralBatRotation;
            targetRotation = Quaternion.Euler(rotationEuler * rotationScale);
            windBack = true;
        }
    }
    internal void release()
    {
        finalRight = neutralBatRotation;
        targetRotation = neutralRotation;
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

            float diff = Mathf.Abs(Quaternion.Angle(neutralRotation, targetRotation));
            float timeTaken = diff / windVelocity;
            float startTime = Time.fixedTime;
            float endTime = startTime + timeTaken;
            while (Time.fixedTime < endTime && windBack)
            {
                float progress = (Time.fixedTime - startTime) / timeTaken;
                transform.localRotation = Quaternion.Slerp(neutralRotation, targetRotation, progress);
                //swingObject.localRotation = Quaternion.Slerp(neutralBatRotation, finalRight,progress);
                yield return null;
            }
            //swingObject.localRotation = finalRight;
            while (windBack)
            {
                yield return null;
            }

            Quaternion fromRotation = transform.localRotation;
            //Quaternion fromBatRotation = swingObject.localRotation;

            diff = Mathf.Abs(Quaternion.Angle(fromRotation, neutralRotation));
            timeTaken = diff / swingVelocity;
            startTime = Time.fixedTime;
            endTime = startTime + timeTaken;

            while (Time.fixedTime < endTime)
            {
                float progress = (Time.fixedTime - startTime) / timeTaken;
                transform.localRotation = Quaternion.Slerp(fromRotation, neutralRotation, progress * progress);
                //swingObject.localRotation = Quaternion.Slerp(fromBatRotation, neutralBatRotation,progress);
                yield return new WaitForFixedUpdate();
            }
            //swingObject.localRotation = neutralBatRotation;

        }
    }
}
