using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Class dedicated to single player control
/// Physics slowing is also controlled here. Make sure objects to slow physics on is in layer 8
/// </summary>
public class PlayerController : MonoBehaviour
{

    internal float deltaF, deltaR;
    public float moveSpeed;
    public float maxLookUpDownAngle = 60; // can't look up/down more than 60 dgs
    internal float upDownRotation = 0.0f;
    internal float rightLeftRotation = 0.0f;
    public float mouseSensitivity = 5;

    public const int LEFT_MOUSE_BUTTON = 0;
    public const int RIGHT_MOUSE_BUTTON = 1;
    public const int MIDDLE_MOUSE_BUTTON = 2;

    public Rigidbody bodyRB;//synced player rigidbody

    public float slowRadius = 2;
    public float slowMultiplier = 0.5f;
    public bool screenlock;
    public float jumpAccelerationTime = 0.5f;
    public float jumpMaxAcceleration = 12;

    float currentSpeedMultiplier = 1;

    bool airborn = true;
    public float restHeight = .385f;
    public float legSpringConstant = 10;
    public Transform cameraTransform;
    public float jumpDipPercent = 0.5f;
    public MeshRenderer reticle;

    public Volume volume;
    Vignette vignette;
    bool lockSequence = false;
    bool jumping = false;

    public float unfocusedVignette;
    public float focusedVignette;

    public float focusTime = 0.5f;
    public float swingRadius = 1;

    public SwingPath swinger;

    public Collider groundCollider;

    public Vector3 updraftForce = new Vector3(0, 20, 0);

    public void Start()
    {
        StartCoroutine(update());
        StartCoroutine(slowTimeController());
        volume.profile.TryGet(out vignette);

    }

    public void OnDestroy()
    {
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
    }
    public IEnumerator strikeAction(float delay, float force, Rigidbody body, Vector3 hitDirection)
    {
        yield return new WaitForSecondsRealtime(delay);

        body.isKinematic = false;
        if (bodies.Contains(body) && screenlock)
        {
            body.velocity = slowMultiplier * (body.mass * body.velocity + 90 * hitDirection * force) / (body.mass + 90);
        }
        else
        {
            body.velocity = (body.mass * body.velocity + 90 * hitDirection * force) / (body.mass + 90);
        }
        //Include followthrough
    }
    public IEnumerator forceField()
    {
        float min_y = groundCollider.bounds.min.y;
        Collider[] results = Physics.OverlapSphere(new Vector3(transform.position.x, min_y, transform.position.z), 2, 1 << 8);
        foreach (Collider c in results)
        {
            if (c.transform.position.y - min_y > .5f)
                continue;
            Rigidbody body = c.GetComponent<Rigidbody>();
            if (body != null)
                body.velocity += updraftForce * currentSpeedMultiplier;
        }
        print(results.Length);
        yield break;
    }
    private void Update()
    {
        RaycastHit hit;
        bool didHit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, swingRadius, 1 << 8);
        if (didHit)
        {
            if (swinger.canHit(swinger.transform, hit.point, -hit.normal))
            {
                reticle.transform.position = hit.point;
                reticle.enabled = true;

                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.transform.root.name.Contains("Tree"))
                    {
                        print("Hit tree");
                    }
                    else
                    {
                        Transform target = hit.transform;
                        Vector3 targetPos = hit.point;
                        Vector2 timeForce = swinger.calculateTimeAndForce(swinger.transform, target.position, hit.point, swinger.transform.right, -hit.normal);//, swingApex,windEndDir);
                        print("After " + timeForce.x + " Velocity is " + timeForce.y);
                        StartCoroutine(strikeAction(timeForce.x, timeForce.y, target.GetComponent<Rigidbody>(), -hit.normal));
                    }
                }
            }
            else
            {
                //Do tree hitting here
            }


        }
        else
        {
            reticle.enabled = false;
        }

    }
    private void FixedUpdate()
    {
        if (!(airborn && jumping))
        {
            bodyRB.velocity += currentSpeedMultiplier * Physics.gravity * Time.deltaTime;
        }
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

            if (Input.GetKeyDown(KeyCode.E) && !airborn)
            {
                StartCoroutine(forceField());
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                //Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = true;
                screenlock = true;
                currentSpeedMultiplier = slowMultiplier;
                StartCoroutine(focusAnimation(true));
                if (airborn && jumping)
                {
                    bodyRB.velocity = new Vector3(bodyRB.velocity.x, bodyRB.velocity.y * currentSpeedMultiplier, bodyRB.velocity.z);
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                //Cursor.lockState = CursorLockMode.Confined;
                //Cursor.visible = false;
                screenlock = false;
                currentSpeedMultiplier = 1;
                StartCoroutine(focusAnimation(false));
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                //bodyRB.velocity += Vector3.up * jumpHeight;
                if (!airborn)
                    StartCoroutine(invokeJump());
            }

            if (!screenlock)
            {
                rightLeftRotation += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime * currentSpeedMultiplier;
                upDownRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * currentSpeedMultiplier;
                upDownRotation = Mathf.Clamp(upDownRotation, -maxLookUpDownAngle, maxLookUpDownAngle);
            }

            transform.rotation = Quaternion.Euler(new Vector3(0, rightLeftRotation, 0));
            Camera.main.transform.localEulerAngles = new Vector3(upDownRotation, 0, 0);
            //If in the air, simulate physics

            bodyRB.velocity = transform.right * deltaR * currentSpeedMultiplier + Vector3.up * bodyRB.velocity.y + deltaF * transform.forward * currentSpeedMultiplier;
            yield return null;
        }
    }
    IEnumerator invokeJump()
    {
        airborn = true;
        jumping = true;
        float startTime = Time.fixedTime;
        float endTime = startTime + jumpAccelerationTime;
        float offset = calculateDipAccelerateOffset();
        Vector3 initialLocalCameraPos = cameraTransform.localPosition;
        Vector3 modifiedCameraPos = initialLocalCameraPos;
        Vector3 velocity = Vector3.zero;
        float currentTime = startTime;

        while (currentTime < endTime)
        {
            float timeElapsed = currentTime - startTime;
            float deltaNextFrame = Time.fixedDeltaTime;// * currentSpeedMultiplier;
            if (timeElapsed / jumpAccelerationTime < jumpDipPercent)//one third of jump is dipping to wind
            {
                float acceleration = timeElapsed * (timeElapsed - offset) - 9.81f;
                bodyRB.AddForce(Vector3.up * acceleration, ForceMode.Acceleration);
                modifiedCameraPos = modifiedCameraPos + velocity * deltaNextFrame;
                velocity += Vector3.up * acceleration * deltaNextFrame;
            }
            else
            {
                //This is wrong code but it looks fine, animation wise
                float acceleration = Mathf.Lerp(jumpMaxAcceleration, 0, timeElapsed - jumpAccelerationTime * jumpDipPercent);

                bodyRB.AddForce(Vector3.up * acceleration, ForceMode.Acceleration);
                if (transform.position.y < 1)
                {
                    modifiedCameraPos = modifiedCameraPos + velocity * deltaNextFrame;
                    velocity += Vector3.up * acceleration * deltaNextFrame;
                }
                else
                {
                    modifiedCameraPos = Vector3.Lerp(modifiedCameraPos, initialLocalCameraPos, Mathf.Sqrt((timeElapsed - jumpAccelerationTime * jumpDipPercent) / (jumpAccelerationTime * (1 - jumpDipPercent))));
                }
            }
            cameraTransform.localPosition = modifiedCameraPos;
            currentTime += deltaNextFrame;
            yield return new WaitForFixedUpdate();
        }
        jumping = false;
    }
    float calculateDipAccelerateOffset()
    {
        float dipPeriod = jumpDipPercent * jumpAccelerationTime;
        return (9.81f + jumpMaxAcceleration - dipPeriod * dipPeriod) / dipPeriod;
    }

    List<Rigidbody> bodies = new List<Rigidbody>();

    IEnumerator slowTimeController()
    {
        while (true)
        {
            while (!screenlock)
            {
                yield return null;
            }

            //Capture all local rigidbodies, slow down time on them
            Collider[] results = Physics.OverlapSphere(transform.position, slowRadius, (1 << 8));
            if (results.Length > 0)
            {
                foreach (Collider hit in results)
                {
                    // if (hit.name != "Player")
                    {
                        Rigidbody rb = hit.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            bodies.Add(rb);
                        }
                    }
                }

                //Capture time sequence
                foreach (Rigidbody rb in bodies)
                {
                    rb.useGravity = false;
                    rb.velocity *= slowMultiplier;
                }
                //Iterate through time, slow motion
                while (screenlock)
                {

                    results = Physics.OverlapSphere(transform.position, slowRadius, (1 << 8));
                    if (results.Length > 0)
                    {
                        foreach (Collider bod in results)
                        {
                            Rigidbody rb = bod.GetComponent<Rigidbody>();
                            if (rb != null && !bodies.Contains(rb))
                            {
                                bodies.Add(rb);
                                rb.useGravity = false;
                                rb.velocity *= slowMultiplier;
                            }
                        }
                    }

                    float delta = Time.fixedDeltaTime * slowMultiplier;
                    for (int i = bodies.Count - 1; i >= 0; i--)
                    {
                        //Vector3 rawResult = positions[i] + velocities[i] * delta;
                        //float position_y = Mathf.Max(1f, rawResult.y);
                        //positions[i] = new Vector3(rawResult.x, position_y, rawResult.z);
                        //bodies[i].position = positions[i];

                        if (bodies[i] == null)
                            goto destroyer;
                        bodies[i].velocity += Physics.gravity * delta;

                    destroyer:
                        if (bodies[i] == null || (bodies[i].position - transform.position).magnitude > slowRadius)
                        {
                            if (bodies[i] != null)
                            {
                                bodies[i].useGravity = true;
                                bodies[i].velocity /= slowMultiplier;
                            }
                            bodies.RemoveAt(i);
                        }
                    }

                    yield return new WaitForFixedUpdate();
                }

                //Reset time sequence
                for (int i = 0; i < bodies.Count; i++)
                {
                    bodies[i].useGravity = true;
                    bodies[i].velocity /= slowMultiplier;
                }

                bodies.Clear();
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
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "Ground")// || collision.collider.name.Contains("Fence") || collision.collider.name.Contains("Tree"))
        {
            airborn = false;
        }
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
}
