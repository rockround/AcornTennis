using System;
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

    public Color slowColor;

    public Color rangeColor;

    bool useDiscrete = false;
    bool hideMouseDefault = false;
    List<Acorn> possibleAcorns;

    public AudioSource tennisServe;
    public GameObject slowMotionHolder;
    AudioSource slowMotionEnter, slowMotionExit;
    public AudioSource[] allOtherSounds;

    public void Start()
    {
        useDiscrete = StaticInfoContainer.useDiscrete;
        hideMouseDefault = StaticInfoContainer.hideMouseDefault;
        AudioSource[] audio = slowMotionHolder.GetComponents<AudioSource>();
        slowMotionEnter = audio[0];
        slowMotionExit = audio[1];
        possibleAcorns = new List<Acorn>();

        StartCoroutine(update());
        StartCoroutine(slowTimeController());
        volume.profile.TryGet(out vignette);

        if (hideMouseDefault)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }
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
        tennisServe.Play();
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
        yield break;
    }
    private void Update()
    {
        Collider[] results = Physics.OverlapSphere(Camera.main.transform.position, slowRadius, (1 << 8));
        //If in results and not in possible, this is a new entry -> results.removeAll(possible)
        //If in possible and not in results, this entry must be discarded -> possible.removeAll(
        //If appearing in both, check if it's close enough to strike
        if (results.Length > 0)
        {
            for (int i = possibleAcorns.Count - 1; i >= 0; i--)
            {
                if (Array.FindIndex(results, x => x == possibleAcorns[i].currentCollider) == -1)
                {
                    possibleAcorns[i].auraMaterial.SetColor("_color", Color.black);
                    possibleAcorns.RemoveAt(i);
                }
            }

            foreach (Collider col in results)
            {
                Acorn acorn = col.GetComponent<Acorn>();
                if (acorn != null)
                {
                    Color colorClass = (col.ClosestPoint(Camera.main.transform.position) - Camera.main.transform.position).magnitude <= swingRadius ? rangeColor : slowColor;
                    if (possibleAcorns.IndexOf(acorn) != -1)
                    {
                        //result may have come closer or gone further
                        acorn.auraMaterial.SetColor("_color", colorClass);
                    }
                    else
                    {
                        //This result is new
                        acorn.auraMaterial.SetColor("_color", colorClass);
                        possibleAcorns.Add(acorn);
                    }


                }
            }

        }
        else
        {
            foreach (Acorn acorn in possibleAcorns)
            {
                acorn.auraMaterial.SetColor("_color", Color.black);
            }
            possibleAcorns.Clear();
        }
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
                        Vector3 targetDir = -hit.normal;
                        if (useDiscrete)
                        {
                            Bounds holder = hit.collider.bounds;
                            Vector3 min = Camera.main.WorldToScreenPoint(holder.min);
                            Vector3 max = Camera.main.WorldToScreenPoint(holder.max);
                            float deltax = max.x - min.x;
                            float deltay = max.y - min.y;
                            int height = 0;
                            int width = 0;
                            if (Input.mousePosition.x < min.x + deltax / 3)
                            {
                                width = 2;
                            }
                            else if (Input.mousePosition.x < min.x + deltax * 2f / 3)
                            {
                                width = 1;
                            }
                            else
                            {
                                width = 0;
                            }
                            if (Input.mousePosition.y < min.y + deltay / 3)
                            {
                                height = 2;
                            }
                            else if (Input.mousePosition.y < min.y + deltay * 2f / 3)
                            {
                                height = 1;
                            }
                            else
                            {
                                height = 0;
                            }
                            int output = 3 * height + width;
                            int sign = 1;//Assume player always plays on team 1;
                            targetDir = getDirectionOut(output, sign);
                            targetPos = hit.collider.ClosestPoint(target.position - targetDir * .5f);


                        }
                        Vector2 timeForce = swinger.calculateTimeAndForce(swinger.transform, target.position,targetPos, swinger.transform.right, targetDir);//, swingApex,windEndDir);
                        if (timeForce == Vector2.zero)
                            return;
                        StartCoroutine(strikeAction(timeForce.x, timeForce.y, target.GetComponent<Rigidbody>(), -hit.normal));
                    }
                }
                return;

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
    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    Vector3 getDirectionOut(int direction, int sign)
    {
        Vector3 directionOut = Vector3.zero;
        switch (direction)
        {
            case 1:
                {
                    directionOut = (new Vector3(.1f, -.05f, sign)).normalized;
                    break;
                }
            case 2:
                {
                    directionOut = (new Vector3(0, -.05f, sign)).normalized;
                    break;
                }
            case 3:
                {
                    directionOut = (new Vector3(.1f, -.05f, sign)).normalized;
                    break;
                }
            case 4:
                {
                    directionOut = (new Vector3(.1f, 0.1f, sign)).normalized;
                    break;
                }
            case 5:
                {
                    directionOut = (new Vector3(0, 0.1f, sign)).normalized;
                    break;
                }
            case 6:
                {
                    directionOut = (new Vector3(-.1f, 0.1f, sign)).normalized;
                    break;
                }
            case 7:
                {
                    directionOut = (new Vector3(.1f, .2f, sign)).normalized;
                    break;
                }
            case 8:
                {
                    directionOut = (new Vector3(0, .2f, sign)).normalized;
                    break;
                }
            case 9:
                {
                    directionOut = (new Vector3(-.1f, .2f, sign)).normalized;
                    break;
                }
        }
        return directionOut;
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
                if (hideMouseDefault)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }

                screenlock = true;
                currentSpeedMultiplier = slowMultiplier;
                StartCoroutine(focusAnimation(true));
                if (airborn && jumping)
                {
                    bodyRB.velocity = new Vector3(bodyRB.velocity.x, bodyRB.velocity.y * currentSpeedMultiplier, bodyRB.velocity.z);
                }
                slowMotionEnter.Play();
                foreach(AudioSource aud in allOtherSounds)
                {
                    aud.volume *= .1f;
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                if (hideMouseDefault)
                {
                    Cursor.lockState = CursorLockMode.Confined;
                    Cursor.visible = false;
                }
                screenlock = false;
                currentSpeedMultiplier = 1;
                StartCoroutine(focusAnimation(false));
                slowMotionExit.Play();
                foreach (AudioSource aud in allOtherSounds)
                {
                    aud.volume /= .1f;
                }
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
            Collider[] results = Physics.OverlapSphere(Camera.main.transform.position, slowRadius, (1 << 8));
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

                    results = Physics.OverlapSphere(Camera.main.transform.position, slowRadius, (1 << 8));
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
                        if (bodies[i] == null || (bodies[i].position - Camera.main.transform.position).magnitude > slowRadius)
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
