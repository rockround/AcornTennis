using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SwingController : MonoBehaviour
{
    public PlayerController controller;
    public Transform swingObject;
    public GameObject swingScreen;

    public Volume volume;
    Vignette vignette;
    public float unfocusedVignette;
    public float focusedVignette;
    public float focusTime = 0.5f;

    bool lockSequence = false;
    // Start is called before the first frame update
    void Start()
    {
        volume.profile.TryGet(out vignette);
        controller.onShiftDown += onLockOn;
        controller.onShiftUp += onLockOff;
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
        if (isTo)
        {
            swingScreen.SetActive(true);
        }
        else
        {
            swingScreen.SetActive(false);
        }
        lockSequence = false;
    }
    void onLockOn()
    {
        StartCoroutine(focusAnimation(true));
    }
    void onLockOff()
    {
        StartCoroutine(focusAnimation(false));
    }

    void swingTL()
    {

    }
    void swingL()
    {

    }
    void swingBL()
    {

    }

    void swingTR()
    {

    }
    void swingR()
    {

    }
    void swingBR()
    {

    }
}
