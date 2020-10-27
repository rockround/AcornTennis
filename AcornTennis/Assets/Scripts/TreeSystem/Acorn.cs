using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the mechanics behind trees
/// </summary>
public class Acorn : MonoBehaviour
{
    GameObject acornPrefab;
    public TreeGrowth tree;
    public float timeToGrow = 5;
    public float pauseIntervalToGrown = 5;
    public float pauseIntervalToMature = 8;
    public float spawnPeriod = 10;

    internal bool alive;
    internal bool grounded;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public Collider currentCollider;
    public Rigidbody currentRigidbody;
    // Start is called before the first frame update
    void Start()
    {
        acornPrefab = Resources.Load<GameObject>("Prefabs/Acorn");
        tree.onStateChanged += OnTreeStateChanged;
    }
    private void OnDestroy()
    {
        tree.onStateChanged -= OnTreeStateChanged;
    }
    // Update is called once per frame
    void Update()
    {

    }
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


        //Destroy shell
        Destroy(currentCollider);
        Destroy(meshFilter);
        Destroy(meshRenderer);
        Destroy(currentRigidbody);
        //Correct transform
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        //Expose insides
        tree.gameObject.SetActive(true);
        tree.BeginGrowTree();
    }
    IEnumerator continueGrowthAfterTime(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        tree.grow = true;
    }

    void OnTreeStateChanged(GrowthState state)
    {
        switch (state)
        {
            case GrowthState.Sapling:
                {
                    StartCoroutine(continueGrowthAfterTime(pauseIntervalToGrown));
                    break;
                }
            case GrowthState.Grown:
                {
                    StartCoroutine(continueGrowthAfterTime(pauseIntervalToMature));
                    break;
                }
            case GrowthState.Mature:
                {
                    StartCoroutine(DropAcorns());
                    break;
                }
        }

    }
    IEnumerator DropAcorns()
    {
        Bounds capBounds = tree.leafArea.bounds;
        Vector3 max = capBounds.max;
        Vector3 min = capBounds.min;
        Vector3 deltas = max - min;
        while (alive)
        {
            float growthPeriod = timeToGrow * (Random.value + .5f);
            Vector3 spawnPos = min + new Vector3(deltas.x * Random.value, -.005f, deltas.z * Random.value);
            GameObject newAcorn = Instantiate(acornPrefab, spawnPos, Quaternion.identity);
            newAcorn.GetComponent<Rigidbody>().isKinematic = true;

            float startTime = Time.unscaledTime;
            float endTime = startTime + timeToGrow;
            while (Time.unscaledTime < endTime)
            {
                Vector3 rawScale = Mathf.Lerp(0.01f, 0.5f, (Time.unscaledTime - startTime) / growthPeriod) * Vector3.one;
                newAcorn.transform.localScale = rawScale;
                newAcorn.transform.position = spawnPos - Vector3.up * rawScale.y / 2;
                yield return null;
            }

            newAcorn.GetComponent<Rigidbody>().isKinematic = false;

            yield return new WaitForSecondsRealtime(spawnPeriod * (Random.value+.1f));
        }
    }
}
