using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    internal float deltaF, deltaR;
    public float moveSpeed;
    public float jumpHeight;
    public float maxLookUpDownAngle = 60; // can't look up/down more than 60 dgs
    internal float upDownRotation = 0.0f;
    internal float rightLeftRotation = 0.0f;
    public float mouseSensitivity = 5;

    public const int LEFT_MOUSE_BUTTON = 0;
    public const int RIGHT_MOUSE_BUTTON = 1;
    public const int MIDDLE_MOUSE_BUTTON = 2;

    public Rigidbody bodyRB;//synced player rigidbody


    public delegate void OnLeftDown();
    public delegate void OnLeftUp();
    public delegate void OnRightDown();
    public delegate void OnRightUp();

    public OnLeftDown onLeftDown;
    public OnLeftUp onLeftUp;
    public OnRightUp onRightUp;
    public OnRightDown onRightDown;

    public bool screenlock;

    public void Start()
    {
        StartCoroutine(update());
    }

    public void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    // Update is called once per frame
    public IEnumerator update()
    {
        while (true)
        {
            if (Input.GetKey(KeyCode.W))
            {
                deltaF = moveSpeed;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                deltaF = -moveSpeed;
            }
            else
            {
                deltaF = 0;
            }
            if (Input.GetKey(KeyCode.A))
            {
                deltaR = -moveSpeed;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                deltaR = moveSpeed;
            }
            else
            {
                deltaR = 0;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                screenlock = true;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                screenlock = false;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                bodyRB.velocity += Vector3.up * jumpHeight;
            }

            if (Input.GetMouseButtonDown(RIGHT_MOUSE_BUTTON))
            {
                onRightDown?.Invoke();
            }
            else if (Input.GetMouseButtonUp(RIGHT_MOUSE_BUTTON))
            {
                onRightUp?.Invoke();
            }

            if (Input.GetMouseButtonDown(LEFT_MOUSE_BUTTON))
            {
                onLeftDown?.Invoke();
            }
            else if (Input.GetMouseButtonUp(LEFT_MOUSE_BUTTON))
            {
                onLeftUp?.Invoke();
            }


            if (!screenlock)
            {
                rightLeftRotation += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                upDownRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
                upDownRotation = Mathf.Clamp(upDownRotation, -maxLookUpDownAngle, maxLookUpDownAngle);
            }

            transform.rotation = Quaternion.Euler(new Vector3(0, rightLeftRotation, 0));
            Camera.main.transform.localEulerAngles = new Vector3(upDownRotation, 0, 0);
            bodyRB.velocity = transform.right * deltaR + Vector3.up * bodyRB.velocity.y + deltaF * transform.forward;
            yield return null;
        }
    }
}
