using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SwingController : MonoBehaviour
{
    public PlayerController controller;

    public Volume volume;
    Vignette vignette;
    public float unfocusedVignette;
    public float focusedVignette;
    public float focusTime = 0.5f;
    public SwingMotion motion;
    public Transform swingObject;
    bool lockSequence = false;

    public SwingPath swingPath;

    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGet(out vignette);
        controller.onShiftDown += onLockOn;
        controller.onShiftUp += onLockOff;
        //controller.onLeftUp += onReleaseSwing;
        //controller.onLeftDown += onSwing;
        controller.onTargetAcquired += targetAcquired;
    }

    // Update is called once per frame
    void Update()
    {
    }
    void targetAcquired(Transform target, Vector3 targetPoint, Vector3 hitDirection)
    {
        Vector3 eulerRotationWind = getRotationEuler() * 200;
        Vector3 swingApex = Quaternion.Euler(eulerRotationWind) * (swingObject.position- swingObject.parent.position) + swingObject.parent.position;
        Vector3 windEndDir = Quaternion.Euler(eulerRotationWind) * swingObject.right;
        swingPath.UpdatePoints(swingObject, target.position + targetPoint,swingObject.right,hitDirection, swingApex,windEndDir);
    }
    Vector3 getRotationEuler()
    {
        Vector2 mouseScreenPos = Vector2.Scale(Input.mousePosition, new Vector3(1f / Screen.width, 1f / Screen.height)) - new Vector2(0.5f, 0.5f);
        //mouseScreenPos = new Vector2(-mouseScreenPos.y, mouseScreenPos.x);
        Vector3 swingRotation;

        //If want to use raw, must set swingRotation to mouseScreenPos
        ///Used to discretize motion
        if (mouseScreenPos.x > 0)
        {
            if (Mathf.Abs(mouseScreenPos.y) < 0.1f)
            {
                swingRotation = new Vector3(0, .5f, 0);
            }
            else if (mouseScreenPos.y > 0)
            {
                swingRotation = new Vector3(-.2f, .5f, 0);
            }
            else
            {
                swingRotation = new Vector3(.3f, .5f, 0);
            }
        }
        else
        {
            if (Mathf.Abs(mouseScreenPos.y) < 0.1f)
            {
                swingRotation = new Vector3(0, -.5f, 0);
            }
            else if (mouseScreenPos.y > 0)
            {
                swingRotation = new Vector3(-.2f, -.5f, 0);
            }
            else
            {
                swingRotation = new Vector3(.3f, -.5f, 0);
            }
        }
        return swingRotation;
    }

    IEnumerator focusAnimation(bool isTo)
    {
        while (lockSequence)
        {
            yield return null;
        }

        lockSequence = true;

        float startTime = Time.unscaledTime;
        float endTime = startTime + focusTime;
        float span;
        float currentVignette;

        if (isTo)
        {
            span = (focusedVignette - unfocusedVignette) / focusTime;
            currentVignette = unfocusedVignette;
        }
        else
        {
            span = (unfocusedVignette - focusedVignette) / focusTime;
            currentVignette = focusedVignette;
        }

        while (Time.unscaledTime < endTime)
        {
            currentVignette += span * Time.unscaledDeltaTime;
            vignette.intensity.value = currentVignette;
            yield return null;
        }

        lockSequence = false;
    }
    void onSwing()
    {
        //Vector3 startPoint = Camera.main.WorldToScreenPoint(swingObject.position);
        Vector2 mouseScreenPos = Vector2.Scale(Input.mousePosition, new Vector3(1f / Screen.width, 1f / Screen.height)) - new Vector2(0.5f, 0.5f);
        //mouseScreenPos = new Vector2(-mouseScreenPos.y, mouseScreenPos.x);
        Vector3 swingRotation;

        //If want to use raw, must set swingRotation to mouseScreenPos
        ///Used to discretize motion
        if (mouseScreenPos.x > 0)
        {
            if (Mathf.Abs(mouseScreenPos.y) < 0.2f)
            {
                swingRotation = new Vector3(0, .5f, 0);
            }
            else if (mouseScreenPos.y > 0)
            {
                swingRotation = new Vector3(-.2f, .5f, 0);
            }
            else
            {
                swingRotation = new Vector3(.3f, .5f, 0);
            }
        }
        else
        {
            if (Mathf.Abs(mouseScreenPos.y) < 0.2f)
            {
                swingRotation = new Vector3(0, -.5f, 0);
            }
            else if (mouseScreenPos.y > 0)
            {
                swingRotation = new Vector3(-.2f, -.5f, 0);
            }
            else
            {
                swingRotation = new Vector3(.3f, -.5f, 0);
            }
        }

        //float dist = mouseScreenPos.magnitude;
        //Vector2 dir = mouseScreenPos.normalized;

        motion.swing(swingRotation, new Vector3(0.5f - mouseScreenPos.x, 0.5f - mouseScreenPos.y, -.1f));
    }
    void onReleaseSwing()
    {
        motion.release();
    }
    void onLockOn()
    {
        StartCoroutine(focusAnimation(true));
    }
    void onLockOff()
    {
        StartCoroutine(focusAnimation(false));
    }
}
