using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMotion : MonoBehaviour
{
    bool windBack = false;
    Quaternion finalRight;
    Quaternion targetRotation;
    Quaternion neutralRotation;
    Quaternion neutralRacketRotation;
    Vector3 neutralRacketPosition;
    public Transform swingObject;
    public Rigidbody racketBody;
    public Transform handle;
    public float rotationScale;
    public float swingVelocity;
    public float windVelocity;
    float angularVelocity;
    bool swinging;
    int currentSwingSign;
    // Start is called before the first frame update
    void Start()
    {
        neutralRotation = transform.localRotation;
        neutralRacketRotation = swingObject.localRotation;
        neutralRacketPosition = swingObject.localPosition;
        StartCoroutine(swing());
    }
    private void FixedUpdate()
    {
        swingObject.localRotation = neutralRacketRotation;
        swingObject.localPosition = neutralRacketPosition;
    }
    internal void swing(Vector3 rotationEuler, Vector3 finalRightDirection)
    {
        if (!windBack)
        {
            currentSwingSign = rotationEuler.y > 0 ? -1 : 1;
            finalRight = Quaternion.FromToRotation(currentSwingSign * Vector3.right, finalRightDirection);// * neutralBatRotation;
            targetRotation = Quaternion.Euler(rotationEuler * rotationScale);
            windBack = true;
            swinging = true;
        }
    }
    internal void release()
    {
        finalRight = neutralRacketRotation;
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
            angularVelocity = windVelocity * Mathf.Deg2Rad * currentSwingSign;
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
            angularVelocity = swingVelocity * Mathf.Deg2Rad * -currentSwingSign;
            while (Time.fixedTime < endTime)
            {
                float progress = (Time.fixedTime - startTime) / timeTaken;
                transform.localRotation = Quaternion.Slerp(fromRotation, neutralRotation, progress * progress);
                //swingObject.localRotation = Quaternion.Slerp(fromBatRotation, neutralBatRotation,progress);
                yield return new WaitForFixedUpdate();
            }
            //swingObject.localRotation = neutralBatRotation;
            swinging = false;
        }
    }
    internal void OnCollisionEnter(Collision collision)
    {

        Vector3 normal = collision.contacts[0].normal;
        Vector3 position = collision.contacts[0].point;
        Rigidbody other = collision.rigidbody;
        float torqueArm = (position - handle.position).magnitude;
        Vector3 batMomentum = -torqueArm * angularVelocity * normal * racketBody.mass;
        Vector3 otherMomentum = other.velocity * other.mass;
        Vector3 finalVelocity = (batMomentum + otherMomentum) / (other.mass + racketBody.mass);
        other.velocity = finalVelocity;
    }
}
