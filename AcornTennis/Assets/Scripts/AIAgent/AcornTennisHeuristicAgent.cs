using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

/// <summary>
/// Class dedicated to single player control
/// Physics slowing is also controlled here. Make sure objects to slow physics on is in layer 8
/// </summary>
public class AcornTennisHeuristicAgent : MonoBehaviour
{

    internal float deltaF, deltaR;
    public float moveSpeed = 2;

    public Rigidbody bodyRB;//synced player rigidbody

    public float slowRadius = 3;
    public float slowMultiplier = 0.05f;
    public bool screenlock;
    public float jumpAccelerationTime = 0.3f;
    public float jumpMaxAcceleration = 20;

    float currentSpeedMultiplier = 1;

    bool airborn = true;
    public float restHeight = .385f;
    public Transform cameraTransform;
    public float jumpDipPercent = 0.35f;

    bool lockSequence = false;
    bool jumping = false;


    public float swingRadius = 1;

    public SwingPath swinger;

    public Collider groundCollider;

    public Vector3 updraftForce = new Vector3(0, 8, 0);

    public int team;

    public void Start()
    {
        StartCoroutine(slowTimeController());

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

    //Only cast to applicable distance. Sphere around
    private void Update()
    {
        RaycastHit hit;
        bool canHit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, swingRadius, 1 << 8);
        if (canHit)
        {
            if (swinger.canHit(swinger.transform, hit.point, -hit.normal))
            {

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
                        StartCoroutine(strikeAction(timeForce.x, timeForce.y, target.GetComponent<Rigidbody>(), -hit.normal));
                    }
                }
            }
            else
            {
                //Do tree hitting here
            }
        }
    }
    //Use this to populate possible targets
    //Choose closest one to hit and go to it
    private void FixedUpdate()
    {
        Physics.OverlapSphere(transform.position, swingRadius);

        if (!(airborn && jumping))
        {
            bodyRB.velocity += currentSpeedMultiplier * Physics.gravity * Time.deltaTime;
        }
    }
    // Update is called once per frame
    //vertical -> -1 back 0 none 1 up
    //horizontal -> -1 left 0 none 1 right
    //power -> -1 slow 0 none 1 updraft
    //Jump
    //swing -> 0 none, 1 TR, 2 T, 3 TL, 4 L, 5 Center, 6 R, 7 BL, 8B, 9 BR
    int previousPower = 0;
    public void InvokeInput(float vertical, float horizontal, int power, int swing, bool jump)
    {
        deltaF = vertical * moveSpeed;

        deltaR = horizontal * moveSpeed;

        //Toggle control
        if (power != previousPower)
        {
            if (power > 0 && !airborn)
            {
                float min_y = groundCollider.bounds.min.y;
                Collider[] results = Physics.OverlapSphere(new Vector3(transform.position.x, min_y, transform.position.z), 2, 1 << 8);
                foreach (Collider c in results)
                {
                    if (c.transform.position.y - min_y > .5f)
                        continue;
                    Rigidbody body = c.GetComponent<Rigidbody>();
                    body.velocity += updraftForce * currentSpeedMultiplier;
                }
            }
            else if(power < 0)//slow time can happen whenever
            {
                screenlock = true;
                currentSpeedMultiplier = slowMultiplier;
                if (airborn && jumping)
                {
                    bodyRB.velocity = new Vector3(bodyRB.velocity.x, bodyRB.velocity.y * currentSpeedMultiplier, bodyRB.velocity.z);
                }
            }

            //Reset slow time 
            if (previousPower == -1)
            {
                screenlock = false;
                currentSpeedMultiplier = 1;
            }
        }
        previousPower = power;

        if (jump)
        {
            //bodyRB.velocity += Vector3.up * jumpHeight;
            if (!airborn)
                StartCoroutine(invokeJump());
        }

        //If in the air, simulate physics
        bodyRB.velocity = Vector3.right * deltaR * currentSpeedMultiplier + Vector3.up * bodyRB.velocity.y + deltaF * Vector3.forward * currentSpeedMultiplier;
    }
    IEnumerator invokeJump()
    {
        airborn = true;
        jumping = true;
        float startTime = Time.fixedTime;
        float endTime = startTime + jumpAccelerationTime;
        float offset = calculateDipAccelerateOffset();
        float currentTime = startTime;

        while (currentTime < endTime)
        {
            float timeElapsed = currentTime - startTime;
            float deltaNextFrame = Time.fixedDeltaTime;// * currentSpeedMultiplier;
            if (timeElapsed / jumpAccelerationTime < jumpDipPercent)//one third of jump is dipping to wind
            {
                float acceleration = timeElapsed * (timeElapsed - offset) - 9.81f;
                bodyRB.AddForce(Vector3.up * acceleration, ForceMode.Acceleration);
            }
            else
            {
                //This is wrong code but it looks fine, animation wise
                float acceleration = Mathf.Lerp(jumpMaxAcceleration, 0, timeElapsed - jumpAccelerationTime * jumpDipPercent);
                bodyRB.AddForce(Vector3.up * acceleration, ForceMode.Acceleration);
            }
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

}
