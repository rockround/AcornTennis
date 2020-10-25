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
    public Transform rotationCenter;

    bool lockSequence = false;
    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGet(out vignette);
        controller.onShiftDown += onLockOn;
        controller.onShiftUp += onLockOff;
        controller.onLeftUp += onReleaseSwing;
        controller.onLeftDown += onSwing;
    }

    // Update is called once per frame
    void Update()
    {

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
        Vector3 startPoint = Camera.main.WorldToScreenPoint(rotationCenter.position);
        Vector2 mouseScreenPos = Vector2.Scale(Input.mousePosition - startPoint, new Vector3(1f/Screen.width, 1f/Screen.height));
        mouseScreenPos = new Vector2(-mouseScreenPos.y, mouseScreenPos.x);
        //float dist = mouseScreenPos.magnitude;
        //Vector2 dir = mouseScreenPos.normalized;

       motion.swing(mouseScreenPos);
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
