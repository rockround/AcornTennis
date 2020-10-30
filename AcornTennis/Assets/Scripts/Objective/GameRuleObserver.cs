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
            Vector2 moveInstruction = GetMoveInstruction();
            agent.InvokeInput( moveInstruction.y, moveInstruction.x, 0, 0, false);
        }
    }

    internal Vector2 GetMoveInstruction()
    {
        Vector2 result;
        float minDistance = 10000000;
        Vector3 delta = Vector3.zero;
        bool ballShouldBeInfront = agent.team == 1;
        //Vector3 behindOffset = sign * Vector3.forward * agent.swingRadius / Mathf.Sqrt(2);


        foreach (Acorn c in acornsOnMySide)
        {
            Vector3 currentDelta = (c.transform.position - agent.transform.position);

            float distance = currentDelta.sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                delta = currentDelta;
            }
        }

        bool behindBall = false;

        float goodZDist = agent.swingRadius / Mathf.Sqrt(2);


        //Be at least a good enough distance from the ball
        if ((delta.z > 0) == ballShouldBeInfront)
        {
            behindBall = true;
        }

        //If close enough do special actions, and behind the ball
        if (minDistance > goodZDist && minDistance <= agent.swingRadius && behindBall)
        {
            //if need to back step

            result = Vector2.zero;
        }
        else
        {
            if (Mathf.Abs(delta.x) < agent.swingRadius / Mathf.Sqrt(2))
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
                    if(Mathf.Abs(delta.z) < goodZDist-.1f)//Around 30 cm of leeway
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
        print(result);
        return result;
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
            print("IS ON MY SIDE");
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
