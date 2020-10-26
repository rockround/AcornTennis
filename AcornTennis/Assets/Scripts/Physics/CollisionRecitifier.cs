using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionRecitifier : MonoBehaviour
{
    public SwingMotion parent;
    private void OnCollisionEnter(Collision collision)
    {
        parent.OnCollisionEnter(collision);
    }
}
