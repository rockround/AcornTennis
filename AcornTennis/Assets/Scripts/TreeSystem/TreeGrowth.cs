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

    public Collider leafArea;

    public delegate void OnStateChanged(GrowthState newState);
    public OnStateChanged onStateChanged;

    Vector3 startingPos;
    internal GrowthState currentState;
    
    // Start is called before the first frame update
    void Start()
    {
        grownScale *= .5f + Random.value * 1.5f;
        saplingScale *= .5f + Random.value * 1.5f;
        matureScale *= .5f + Random.value * 1.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    internal void BeginGrowTree()
    {
        StartCoroutine(growTreeSequence());
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
        onStateChanged?.Invoke(currentState);

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
        onStateChanged?.Invoke(currentState);

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
        onStateChanged?.Invoke(currentState);
    }
}
public enum GrowthState { Seed, Sapling, Grown, Mature}
