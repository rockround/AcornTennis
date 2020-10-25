﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Class dedicated to single player control
/// Physics slowing is also controlled here. Make sure objects to slow physics on is in layer 8
/// </summary>
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
    public delegate void OnShiftDown();
    public delegate void OnShiftUp();

    public OnLeftDown onLeftDown;
    public OnLeftUp onLeftUp;
    public OnRightUp onRightUp;
    public OnRightDown onRightDown;
    public OnShiftUp onShiftUp;
    public OnShiftDown onShiftDown;


    public float slowRadius = 2;
    public float slowMultiplier = 0.5f;
    public bool screenlock;

    public void Start()
    {
        StartCoroutine(update());
        StartCoroutine(slowTimeController());
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
                onShiftDown?.Invoke();
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                screenlock = false;
                onShiftUp?.Invoke();
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

    IEnumerator slowTimeController()
    {
        while (true)
        {
            while (!screenlock)
            {
                yield return null;
            }

            //Capture all local rigidbodies, slow down time on them

            List<Rigidbody> bodies = new List<Rigidbody>();
            List<Vector3> velocities = new List<Vector3>();
            List<Vector3> positions = new List<Vector3>();

            Collider[] results = Physics.OverlapSphere(transform.position, slowRadius);

            if (results.Length > 0)
            {
                foreach (Collider hit in results)
                {
                    print(hit.name);
                    Rigidbody rb = hit.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        print(rb.name);
                        bodies.Add(rb);
                    }
                }

                //Capture time sequence
                foreach (Rigidbody rb in bodies)
                {
                    velocities.Add(rb.velocity);
                    positions.Add(rb.position);
                    rb.useGravity = false;
                    rb.velocity = Vector3.zero;
                }

                //Iterate through time, slow motion
                while (screenlock)
                {
                    float delta = Time.fixedDeltaTime * slowMultiplier;
                    for (int i = 0; i < bodies.Count; i++)
                    {
                        positions[i] = positions[i] + velocities[i] * delta;
                        bodies[i].position = positions[i];

                        velocities[i] = velocities[i] + Physics.gravity * delta;
                        bodies[i].velocity = velocities[i];
                    }
                    yield return new WaitForFixedUpdate();
                }

                //Reset time sequence
                for (int i = 0; i < bodies.Count; i++)
                {
                    bodies[i].useGravity = true;
                    bodies[i].velocity = velocities[i];
                    bodies[i].position = positions[i];
                }

                bodies.Clear();
                velocities.Clear();
                positions.Clear();
            }
            else
            {
                while (screenlock)
                {
                    yield return null;
                }
            }

        }
    }

}
