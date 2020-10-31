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

    public Color[] leafLit, leafShaded, leafHighlightColor;
    float[] leafSteps = new float[] {6,3.88f,4.99f,.561f };
    float[] leafOffsets = new float[] {.34f,.55f,.47f,.19f };
    float[] leafSpreads = new float[] {.652f, .62f,.652f,.738f };
    float[] leafHighlight = new float[] {.034f, 0,.026f,.005f };
    public Texture[] leafTextures;
    public MeshRenderer leaf1, leaf2;
    Material leaf1Mat, leaf2Mat;
    public int leafTypeFirst, leafTypeSecond;

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
        leaf1Mat = leaf1.material;
        leaf2Mat = leaf2.material;
        grownScale *= .5f + Random.value * .5f;
        saplingScale *= .5f + Random.value * .5f;
        matureScale *= .5f + Random.value * .5f;
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

        int nextLeafType1 = Mathf.Min(3, leafTypeFirst + 1);
        int nextLeafType2 = Mathf.Min(3, leafTypeSecond + 1);
        leaf1Mat.SetTexture("Texture2D_F9676FD0", leafTextures[leafTypeFirst]);
        leaf2Mat.SetTexture("Texture2D_F9676FD0", leafTextures[leafTypeSecond]);

        while (Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / growthTime;
            currentScale = Mathf.Lerp(grownScale, matureScale, progress);
            Vector3 rawScale = Vector3.one * currentScale;
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;

            leaf1Mat.SetColor("Color_F689E8B9", Color.Lerp(leafLit[leafTypeFirst], leafLit[nextLeafType1], progress));
            leaf1Mat.SetColor("Color_28180829", Color.Lerp(leafShaded[leafTypeFirst], leafShaded[nextLeafType1], progress));
            leaf1Mat.SetColor("Color_57D43A16", Color.Lerp(leafHighlightColor[leafTypeFirst], leafHighlightColor[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_CF8B0D8D", Mathf.Lerp(leafSteps[leafTypeFirst], leafSteps[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_F3F6B787", Mathf.Lerp(leafOffsets[leafTypeFirst], leafOffsets[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_38BD3441", Mathf.Lerp(leafSpreads[leafTypeFirst], leafSpreads[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_ED5ABFD9", Mathf.Lerp(leafHighlight[leafTypeFirst], leafHighlight[nextLeafType1], progress));

            leaf2Mat.SetColor("Color_F689E8B9", Color.Lerp(leafLit[leafTypeSecond], leafLit[nextLeafType2], progress));
            leaf2Mat.SetColor("Color_28180829", Color.Lerp(leafShaded[leafTypeSecond], leafShaded[nextLeafType2], progress));
            leaf2Mat.SetColor("Color_57D43A16", Color.Lerp(leafHighlightColor[leafTypeSecond], leafHighlightColor[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_CF8B0D8D", Mathf.Lerp(leafSteps[leafTypeSecond], leafSteps[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_F3F6B787", Mathf.Lerp(leafOffsets[leafTypeSecond], leafOffsets[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_38BD3441", Mathf.Lerp(leafSpreads[leafTypeSecond], leafSpreads[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_ED5ABFD9", Mathf.Lerp(leafHighlight[leafTypeSecond], leafHighlight[nextLeafType2], progress));

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

        int nextLeafType1 = Mathf.Min(3,leafTypeFirst + 1);
        int nextLeafType2 = Mathf.Min(3,leafTypeSecond + 1);
        while (Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / initialGrowthTime;
            currentScale = Mathf.Lerp(startScale, saplingScale, progress);
            Vector3 rawScale = Vector3.one * currentScale;
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;

            leaf1Mat.SetColor("Color_F689E8B9", Color.Lerp(leafLit[leafTypeFirst], leafLit[nextLeafType1], progress));
            leaf1Mat.SetColor("Color_28180829", Color.Lerp(leafShaded[leafTypeFirst], leafShaded[nextLeafType1], progress));
            leaf1Mat.SetColor("Color_57D43A16", Color.Lerp(leafHighlightColor[leafTypeFirst], leafHighlightColor[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_CF8B0D8D", Mathf.Lerp(leafSteps[leafTypeFirst], leafSteps[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_F3F6B787", Mathf.Lerp(leafOffsets[leafTypeFirst], leafOffsets[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_38BD3441", Mathf.Lerp(leafSpreads[leafTypeFirst], leafSpreads[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_ED5ABFD9", Mathf.Lerp(leafHighlight[leafTypeFirst], leafHighlight[nextLeafType1], progress));

            leaf2Mat.SetColor("Color_F689E8B9", Color.Lerp(leafLit[leafTypeSecond], leafLit[nextLeafType2], progress));
            leaf2Mat.SetColor("Color_28180829", Color.Lerp(leafShaded[leafTypeSecond], leafShaded[nextLeafType2], progress));
            leaf2Mat.SetColor("Color_57D43A16", Color.Lerp(leafHighlightColor[leafTypeSecond], leafHighlightColor[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_CF8B0D8D", Mathf.Lerp(leafSteps[leafTypeSecond], leafSteps[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_F3F6B787", Mathf.Lerp(leafOffsets[leafTypeSecond], leafOffsets[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_38BD3441", Mathf.Lerp(leafSpreads[leafTypeSecond], leafSpreads[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_ED5ABFD9", Mathf.Lerp(leafHighlight[leafTypeSecond], leafHighlight[nextLeafType2], progress));


            yield return null;
        }

        currentState = GrowthState.Sapling;
        StartCoroutine(continueGrowthAfterTime(pauseIntervalToGrown));

        yield return new WaitUntil(() => grow == true);
        grow = false;


        startTime = Time.fixedTime;
        endTime = startTime + growthTime;

        leafTypeFirst = nextLeafType1;
        leafTypeSecond = nextLeafType2;
        nextLeafType1 = Mathf.Min(3, leafTypeFirst + 1);
        nextLeafType2 = Mathf.Min(3, leafTypeSecond + 1);
        leaf1Mat.SetTexture("Texture2D_F9676FD0", leafTextures[leafTypeFirst]);
        leaf2Mat.SetTexture("Texture2D_F9676FD0", leafTextures[leafTypeSecond]);

        while (Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / growthTime;
            currentScale = Mathf.Lerp(saplingScale, grownScale, progress);
            Vector3 rawScale = Vector3.one * currentScale;
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;

            leaf1Mat.SetColor("Color_F689E8B9", Color.Lerp(leafLit[leafTypeFirst], leafLit[nextLeafType1], progress));
            leaf1Mat.SetColor("Color_28180829", Color.Lerp(leafShaded[leafTypeFirst], leafShaded[nextLeafType1], progress));
            leaf1Mat.SetColor("Color_57D43A16", Color.Lerp(leafHighlightColor[leafTypeFirst], leafHighlightColor[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_CF8B0D8D", Mathf.Lerp(leafSteps[leafTypeFirst], leafSteps[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_F3F6B787", Mathf.Lerp(leafOffsets[leafTypeFirst], leafOffsets[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_38BD3441", Mathf.Lerp(leafSpreads[leafTypeFirst], leafSpreads[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_ED5ABFD9", Mathf.Lerp(leafHighlight[leafTypeFirst], leafHighlight[nextLeafType1], progress));

            leaf2Mat.SetColor("Color_F689E8B9", Color.Lerp(leafLit[leafTypeSecond], leafLit[nextLeafType2], progress));
            leaf2Mat.SetColor("Color_28180829", Color.Lerp(leafShaded[leafTypeSecond], leafShaded[nextLeafType2], progress));
            leaf2Mat.SetColor("Color_57D43A16", Color.Lerp(leafHighlightColor[leafTypeSecond], leafHighlightColor[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_CF8B0D8D", Mathf.Lerp(leafSteps[leafTypeSecond], leafSteps[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_F3F6B787", Mathf.Lerp(leafOffsets[leafTypeSecond], leafOffsets[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_38BD3441", Mathf.Lerp(leafSpreads[leafTypeSecond], leafSpreads[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_ED5ABFD9", Mathf.Lerp(leafHighlight[leafTypeSecond], leafHighlight[nextLeafType2], progress));

            yield return null;
        }
        currentState = GrowthState.Grown;
        StartCoroutine(continueGrowthAfterTime(pauseIntervalToMature));

        yield return new WaitUntil(() => grow == true);
        grow = false;


        startTime = Time.fixedTime;
        endTime = startTime + maturityTime;

        leafTypeFirst = nextLeafType1;
        leafTypeSecond = nextLeafType2;
        nextLeafType1 = Mathf.Min(3, leafTypeFirst + 1);
        nextLeafType2 = Mathf.Min(3, leafTypeSecond + 1);
        leaf1Mat.SetTexture("Texture2D_F9676FD0", leafTextures[leafTypeFirst]);
        leaf2Mat.SetTexture("Texture2D_F9676FD0", leafTextures[leafTypeSecond]);

        while (Time.fixedTime < endTime)
        {
            float progress = (Time.fixedTime - startTime) / growthTime;
            currentScale = Mathf.Lerp(grownScale, matureScale, progress);
            Vector3 rawScale = Vector3.one *currentScale;
            Vector3 displacementUpTrunk = -Vector3.up * rawScale.y * .5f;
            transform.localScale = rawScale;

            leaf1Mat.SetColor("Color_F689E8B9", Color.Lerp(leafLit[leafTypeFirst], leafLit[nextLeafType1], progress));
            leaf1Mat.SetColor("Color_28180829", Color.Lerp(leafShaded[leafTypeFirst], leafShaded[nextLeafType1], progress));
            leaf1Mat.SetColor("Color_57D43A16", Color.Lerp(leafHighlightColor[leafTypeFirst], leafHighlightColor[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_CF8B0D8D", Mathf.Lerp(leafSteps[leafTypeFirst], leafSteps[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_F3F6B787", Mathf.Lerp(leafOffsets[leafTypeFirst], leafOffsets[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_38BD3441", Mathf.Lerp(leafSpreads[leafTypeFirst], leafSpreads[nextLeafType1], progress));
            leaf1Mat.SetFloat("Vector1_ED5ABFD9", Mathf.Lerp(leafHighlight[leafTypeFirst], leafHighlight[nextLeafType1], progress));

            leaf2Mat.SetColor("Color_F689E8B9", Color.Lerp(leafLit[leafTypeSecond], leafLit[nextLeafType2], progress));
            leaf2Mat.SetColor("Color_28180829", Color.Lerp(leafShaded[leafTypeSecond], leafShaded[nextLeafType2], progress));
            leaf2Mat.SetColor("Color_57D43A16", Color.Lerp(leafHighlightColor[leafTypeSecond], leafHighlightColor[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_CF8B0D8D", Mathf.Lerp(leafSteps[leafTypeSecond], leafSteps[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_F3F6B787", Mathf.Lerp(leafOffsets[leafTypeSecond], leafOffsets[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_38BD3441", Mathf.Lerp(leafSpreads[leafTypeSecond], leafSpreads[nextLeafType2], progress));
            leaf2Mat.SetFloat("Vector1_ED5ABFD9", Mathf.Lerp(leafHighlight[leafTypeSecond], leafHighlight[nextLeafType2], progress));

            yield return null;
        }
        currentState = GrowthState.Mature;

        leafTypeFirst = nextLeafType1;
        leafTypeSecond = nextLeafType2;
        leaf1Mat.SetTexture("Texture2D_F9676FD0", leafTextures[leafTypeFirst]);
        leaf2Mat.SetTexture("Texture2D_F9676FD0", leafTextures[leafTypeSecond]);
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
            while (Time.unscaledTime < endTime && newAcorn.GetComponent<Rigidbody>().isKinematic)
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
