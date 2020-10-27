﻿using System.Collections;
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
    public float trunkGirthRate = 0.5f;
    public float trunkHeightRate = 1.5f;
    public float capGirthRate = 1.5f;
    public float capHeightRate = 0.9f;

    public Transform treeTrunk;
    public Transform treeCap;

    public delegate void OnStateChanged(GrowthState newState);
    public OnStateChanged onStateChanged;

    Vector3 startingPos;
    internal GrowthState currentState;
    
    // Start is called before the first frame update
    void Start()
    {
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

        float startTime = Time.fixedTime;
        float endTime = startTime + initialGrowthTime;

        while (Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / initialGrowthTime;
            Vector3 rawScale = Vector3.one * Mathf.Lerp(startScale, saplingScale, progress);
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;
            yield return null;
        }

        currentState = GrowthState.Sapling;
        onStateChanged?.Invoke(currentState);

        yield return new WaitUntil(() => grow == true);
        grow = false;

        //Adjust proportional growth rate
        //treeTrunk.localScale = Vector3.Scale(treeTrunk.localScale, new Vector3(trunkGirthRate, trunkHeightRate, trunkGirthRate));
        treeCap.localScale = Vector3.Scale(treeCap.localScale, new Vector3(capGirthRate, capHeightRate, capGirthRate));

        startTime = Time.fixedTime;
        endTime = startTime + growthTime;

        while (Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / growthTime;
            Vector3 rawScale = Vector3.one * Mathf.Lerp(saplingScale, grownScale, progress);
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;
            yield return null;
        }
        currentState = GrowthState.Grown;
        onStateChanged?.Invoke(currentState);

        yield return new WaitUntil(() => grow == true);
        grow = false;

        //Adjust proportional growth rate again
        //treeTrunk.localScale = Vector3.Scale(treeTrunk.localScale, new Vector3(trunkGirthRate, trunkHeightRate, trunkGirthRate));
        treeCap.localScale = Vector3.Scale(treeCap.localScale, new Vector3(capGirthRate, capHeightRate, capGirthRate));

        startTime = Time.fixedTime;
        endTime = startTime + maturityTime;
        while(Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / growthTime;
            Vector3 rawScale = Vector3.one * Mathf.Lerp(grownScale, matureScale, progress);
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;
            yield return null;
        }
        currentState = GrowthState.Mature;
        onStateChanged?.Invoke(currentState);
    }
}
public enum GrowthState { Seed, Sapling, Grown, Mature}
