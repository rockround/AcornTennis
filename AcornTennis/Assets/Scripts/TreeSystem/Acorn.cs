using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the mechanics behind trees
/// </summary>
public class Acorn : MonoBehaviour
{
    public GameObject treePrefab;
    public float timeToGrow = 5;

    internal bool alive;
    internal bool grounded;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public Collider currentCollider;
    public Rigidbody currentRigidbody;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "Ground")
        {
            if (!alive)
            {
                alive = true;
                StartCoroutine(growthTimer());
            }
            grounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.name == "Ground")
        {
            grounded = false;
        }
    }
    
    IEnumerator growthTimer()
    {
        float startTime = Time.unscaledTime;
        float endTime = startTime + timeToGrow;
        while (Time.unscaledTime < endTime)
        {
            yield return null;
        }

        yield return new WaitUntil(() => grounded == true);


        float minY = currentCollider.bounds.min.y;

        GameObject newTree = Instantiate(treePrefab, new Vector3(transform.position.x, minY, transform.position.z), Quaternion.identity);
        newTree.GetComponent<TreeGrowth>().BeginGrowTree();

        Destroy(gameObject);
    }

}
