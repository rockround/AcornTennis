using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameRuleObserver : MonoBehaviour
{
    Vector3 centerField1 = new Vector3(-3.92f, 5, -8.21f);
    Vector3 extentField1 = new Vector3(6, 10, 8);
    Vector3 centerField2 = new Vector3(-3.92f, 5, 8.23f);
    Vector3 extentField2 = new Vector3(6, 10, 8);
    internal List<Collider> acornsOnMySide;
    public AcornTennisHeuristicAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        acornsOnMySide = new List<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = acornsOnMySide.Count-1; i >= 0; i--)
        {
            if (acornsOnMySide[i] == null)
            {
                acornsOnMySide.RemoveAt(i);
                //acorn has turned into a tree
            }
        }
    }
    //Got it on my side, must fi
    internal void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.name.Contains("Tree"))
            return;
        bool isOnField1 = (other.transform.position - centerField1).magnitude < (other.transform.position - centerField2).magnitude;
        bool isTeam1 = agent.team == 1;
        bool isOnMySide = isTeam1 == isOnField1;
        if (isOnMySide)
        {
            if (!acornsOnMySide.Contains(other))
                acornsOnMySide.Add(other);
        }

    }
    //Got it on the other side, reward
    internal void OnTriggerExit(Collider other)
    {
        if (acornsOnMySide.Contains(other))
        {
            acornsOnMySide.Remove(other);
        }

    }
}
