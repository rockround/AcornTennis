using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGrowth : MonoBehaviour
{
    internal bool grow;
    public float initialGrowthTime = 1;
    public float growthTime = 1;
    public float maturityTime = 1;

    float startScale = 0.01f;

    public float saplingScale = 1;
    public float grownScale = 2;
    public float matureScale = 3;

    public float timeToGrow = 5;
    public float timeToDevelop = 5;
    public float pauseIntervalToGrown = 5;
    public float pauseIntervalToMature = 8;
    public float spawnPeriod = 10;



    public Collider leafArea;
    internal bool alive;

    public delegate void OnStateChanged(GrowthState newState);
    public OnStateChanged onStateChanged;

    Vector3 startingPos;
    internal GrowthState currentState;
    public GameObject acornPrefab;

    public bool startGrown = false;
    // Start is called before the first frame update
    void Start()
    {
        grownScale *= .5f + Random.value * 1.5f;
        saplingScale *= .5f + Random.value * 1.5f;
        matureScale *= .5f + Random.value * 1.5f;
        alive = true;
        if (startGrown)
        {
            StartCoroutine(startGrownSeq());
        }
    }

    internal void BeginGrowTree()
    {
        StartCoroutine(growTreeSequence());
    }
    IEnumerator startGrownSeq()
    {
        float startTime = Time.fixedTime;
        float endTime = startTime + maturityTime;
        float currentScale = grownScale;
        while (Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / growthTime;
            currentScale = Mathf.Lerp(grownScale, matureScale, progress);
            Vector3 rawScale = Vector3.one * currentScale;
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;
            yield return null;
        }
        currentState = GrowthState.Mature;
        StartCoroutine(DropAcorns());
    }
    IEnumerator growTreeSequence()
    {
        //Vector3 grownTrunkSize = startingTrunkSize * growthScale;
        //Vector3 matureTrunkSize = startingTrunkSize * matureScale;

        float currentScale = startScale;
        float startTime = Time.fixedTime;
        float endTime = startTime + initialGrowthTime;

        while (Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / initialGrowthTime;
            currentScale = Mathf.Lerp(startScale, saplingScale, progress);
            Vector3 rawScale = Vector3.one * currentScale;
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;
            yield return null;
        }

        currentState = GrowthState.Sapling;
        StartCoroutine(continueGrowthAfterTime(pauseIntervalToGrown));

        yield return new WaitUntil(() => grow == true);
        grow = false;


        startTime = Time.fixedTime;
        endTime = startTime + growthTime;

        while (Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / growthTime;
            currentScale = Mathf.Lerp(saplingScale, grownScale, progress);
            Vector3 rawScale = Vector3.one * currentScale;
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;
            yield return null;
        }
        currentState = GrowthState.Grown;
        StartCoroutine(continueGrowthAfterTime(pauseIntervalToMature));

        yield return new WaitUntil(() => grow == true);
        grow = false;


        startTime = Time.fixedTime;
        endTime = startTime + maturityTime;
        while(Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / growthTime;
            currentScale = Mathf.Lerp(grownScale, matureScale, progress);
            Vector3 rawScale = Vector3.one *currentScale;
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;
            yield return null;
        }
        currentState = GrowthState.Mature;
        StartCoroutine(DropAcorns());

    }
    IEnumerator continueGrowthAfterTime(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        grow = true;
    }

    void OnTreeStateChanged(GrowthState state)
    {
        switch (state)
        {
            case GrowthState.Sapling:
                {
                    break;
                }
            case GrowthState.Grown:
                {
                    break;
                }
            case GrowthState.Mature:
                {
                    break;
                }
        }

    }
    IEnumerator DropAcorns()
    {
        Bounds capBounds = leafArea.bounds;
        Vector3 max = capBounds.max;
        Vector3 min = capBounds.min;
        Vector3 deltas = max - min;
        while (alive)
        {
            float growthPeriod = timeToDevelop * (Random.value + .5f);
            Vector3 spawnPos = min + new Vector3(deltas.x * Random.value, -.005f, deltas.z * Random.value);
            GameObject newAcorn = Instantiate(acornPrefab, spawnPos, Quaternion.identity);
            newAcorn.GetComponent<Rigidbody>().isKinematic = true;

            float startTime = Time.unscaledTime;
            float endTime = startTime + growthPeriod;
            while (Time.unscaledTime < endTime)
            {
                Vector3 rawScale = Mathf.Lerp(0.01f, 0.5f, (Time.unscaledTime - startTime) / growthPeriod) * Vector3.one;
                newAcorn.transform.localScale = rawScale;
                newAcorn.transform.position = spawnPos - Vector3.up * rawScale.y / 2;
                yield return null;
            }

            newAcorn.GetComponent<Rigidbody>().isKinematic = false;

            yield return new WaitForSecondsRealtime(spawnPeriod * (Random.value + .1f));
        }
    }
}
public enum GrowthState { Seed, Sapling, Grown, Mature}
