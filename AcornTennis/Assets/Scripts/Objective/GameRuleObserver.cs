using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameRuleObserver : MonoBehaviour
{
    Vector3 centerField1 = new Vector3(-3.92f, 5, -8.21f);
    Vector3 extentField1 = new Vector3(6, 10, 8);
    Vector3 centerField2 = new Vector3(-3.92f, 5, 8.23f);
    Vector3 extentField2 = new Vector3(6, 10, 8);
    internal List<Acorn> acornsOnMySide;
    public AcornTennisHeuristicAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        acornsOnMySide = new List<Acorn>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = acornsOnMySide.Count - 1; i >= 0; i--)
        {
            if (acornsOnMySide[i] == null)
            {
                acornsOnMySide.RemoveAt(i);
                //acorn has turned into a tree
            }
        }
        if (acornsOnMySide.Count > 0)
        {
            var moveInstruction = GetMoveInstruction();
            agent.InvokeInput(moveInstruction.Item1, moveInstruction.Item2, moveInstruction.Item3, moveInstruction.Item4, moveInstruction.Item5);
        }
    }

    internal Tuple<float, float, int, int, bool> GetMoveInstruction()
    {
        Vector2 result;
        float minDistance = 10000000;
        Vector3 delta = Vector3.zero;
        bool ballShouldBeInfront = agent.team == 1;
        //Vector3 behindOffset = sign * Vector3.forward * agent.swingRadius / Mathf.Sqrt(2);
        Acorn target = null;
        bool slow = false;
        int power = 0;
        int swing = 0;

        foreach (Acorn c in acornsOnMySide)
        {
            Vector3 currentDelta = Vector3.Scale((c.transform.position - agent.transform.position), new Vector3(1, 0, 1));

            float distance = currentDelta.sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                delta = currentDelta;
                target = c;
            }
        }

        bool behindBall = false;
        float goodZDist = agent.swingRadius / Mathf.Sqrt(3);


        //Be at least a good enough distance from the ball
        if ((delta.z > 0) == ballShouldBeInfront)
        {
            behindBall = true;
        }

        float displacement = target.transform.position.y - agent.transform.position.y;
        float farZ = Mathf.Abs(target.transform.position.z - agent.transform.position.z);
        bool farEnough = farZ >= .25f;
        //If close enough do special actions, and behind the ball
        //Spike if too close for full swing, high enough to spike correctly, and behind ball and far enough to hit properly
        bool spikeable = minDistance <= agent.swingRadius && target.transform.position.y > 2;

        //Can be normally striked if far enough away to swing, and close enough to reach (Later differentiate into small and large swing candidates)
        bool normalStrikeAble = minDistance >= goodZDist && minDistance <= agent.swingRadius && behindBall && farZ > .3f && Mathf.Abs(displacement) < .3f;

        bool mustJump = minDistance <= agent.swingRadius && displacement > agent.swingRadius;

        bool mustUpdraft = minDistance <= agent.swingRadius && displacement < -.3f;

        //Swing if it is within range and at a good height a distance away, or if it is straight in front and want to go for the spike
        if (behindBall && farEnough && (spikeable || normalStrikeAble || mustJump || mustUpdraft))
        {
            //If close enough, swing
            if (spikeable || normalStrikeAble)
            {
                int swingDirection = ballShouldBeInfront ? 1 : -1;
                if (spikeable)
                {
                    if(target.transform.position.y > 4)
                        agent.tryStrikeAction(target.GetComponent<Rigidbody>(), 2, swingDirection);
                    else if(target.transform.position.y > 3)
                        agent.tryStrikeAction(target.GetComponent<Rigidbody>(), 5, swingDirection);
                    else
                        agent.tryStrikeAction(target.GetComponent<Rigidbody>(), 8, swingDirection);
                }
                else
                {
                    agent.tryStrikeAction(target.GetComponent<Rigidbody>(), 8, swingDirection);
                }
            }
            //If below, use power. If above, jump
            else if (mustJump)
            {
                power = 1;
            }
            else if (mustUpdraft)
            {
                if (agent.forceFieldCooldown)//if on cooldown, wait
                {
                    power = 0;
                }
                else
                {
                    power = -1;
                }
            }
            result = Vector2.zero;
        }
        else
        {
            if (Mathf.Abs(delta.x) < agent.swingRadius / Mathf.Sqrt(3))
            {
                result.x = 0;
            }
            else if (delta.x > 0)
            {
                result.x = 1;
            }
            else
            {
                result.x = -1;
            }

            //complex function for moving back (don't move if already behind ball and close enough to hit, vertically
            //if ahead ball, move behind without hesitation
            if (!behindBall)
            {
                result.y = ballShouldBeInfront ? -1 : 1;
            }
            else
            {
                //If behind ball, if too far move forwards. Else don't
                if (Mathf.Abs(delta.z) > goodZDist)
                {
                    result.y = ballShouldBeInfront ? 1 : -1;
                }
                else
                {
                    if (Mathf.Abs(delta.z) < goodZDist - .1f)//Around 10 cm of leeway
                    {
                        result.y = ballShouldBeInfront ? -.5f : .5f;
                    }
                    else
                    {
                        result.y = 0;
                    }
                }

            }

        }

        return new Tuple<float, float, int, int, bool>(result.y, result.x, power, swing, slow);
    }
    //Got it on my side, must fi
    internal void OnTriggerEnter(Collider other)
    {
        Acorn acorn = other.GetComponent<Acorn>();
        if (acorn == null)
        {
            return;
        }

        if (other.transform.root.name.Contains("Tree"))
            return;

        bool isOnField1 = (other.transform.position - centerField1).magnitude < (other.transform.position - centerField2).magnitude;
        bool isTeam1 = agent.team == 1;
        bool isOnMySide = isTeam1 == isOnField1;

        if (isOnMySide)
        {
            if (!acornsOnMySide.Contains(acorn))
                acornsOnMySide.Add(acorn);
        }

    }
    //Got it on the other side, reward
    internal void OnTriggerExit(Collider other)
    {
        Acorn acorn = other.GetComponent<Acorn>();

        if (acorn == null)
        {
            return;
        }

        if (acornsOnMySide.Contains(acorn))
        {
            acornsOnMySide.Remove(acorn);
        }

    }
}
