using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingPath : MonoBehaviour
{
    public float granularity = .05f;
    public LineRenderer lr;
    public Transform face;
    // Update is called once per frame
    internal void UpdatePoints(Transform start, Vector3 targetPos, Vector3 startDir, Vector3 endDir, Vector3 windEndPos, Vector3 windEndDir)
    {
        Vector3 delta = targetPos - start.position;
        Vector3 deltaLocal = start.InverseTransformDirection(delta);
        int sign = 1;
        if (deltaLocal.x < 0)
            sign = -1;

        startDir *= sign;
        windEndDir *= sign;
        //Wind
        var pointData = generatePoints(start.position, windEndPos, startDir, windEndDir);
        List<Vector3> points = pointData.Item1;
        List<Vector3> directions = pointData.Item2;

        /*lr.positionCount = 0;
        for (int i = 0; i < points.Count; i++)
        {
            lr.positionCount += 1;
            //point is centroid of tube
            lr.SetPosition(i, points[i]);
        }
        StartCoroutine(followSequence(points, directions));*/

        //Swing
        int swapIndex = points.Count-1;

      //  targetPos += (endDir + -sign * start.parent.right)/2 * .5f; //follow through
        pointData = generatePoints(windEndPos, targetPos, -windEndDir, endDir);
        points.AddRange(pointData.Item1);
        directions.AddRange(pointData.Item2);

        lr.positionCount = 0;
        for (int i = 0; i < points.Count; i++)
        {
            lr.positionCount += 1;
            //point is centroid of tube
            lr.SetPosition(i, points[i]);
        }
        StartCoroutine(followSequence(points, directions,swapIndex));
    }
    IEnumerator followSequence(List<Vector3> points, List<Vector3> directions, int swapIndex)
    {
        float timePerSegment = .02f;
        for(int i=1; i< points.Count; i++)
        {
            float startTime = Time.unscaledTime;
            if(i > swapIndex)
            {
                timePerSegment /= 1.04f;
            }
            float endTime = startTime + timePerSegment;
            Vector3 startDir = directions[i - 1];
            Vector3 endDir = directions[i];
            Vector3 startPos = points[i-1];
            Vector3 endPos = points[i];
            while(Time.unscaledTime < endTime)
            {
                float progress = (Time.unscaledTime - startTime) / timePerSegment;
                face.position = Vector3.Lerp(startPos,endPos,progress);
                face.forward = Vector3.Lerp(startDir, endDir, progress);
                yield return null;
            }
        }
    }
    Tuple<List<Vector3>, List<Vector3>> generatePoints(Vector3 start, Vector3 end, Vector3 startNorm, Vector3 endNorm)
    {
        List<Vector3> points = new List<Vector3>();
        List<Vector3> directions = new List<Vector3>();
        Vector3 prevPoint = Vector3.zero;
        int iterations = Mathf.RoundToInt(1f / granularity);
        for (int i = 0; i <= iterations; i++)
        {
            float t = i * granularity;
            Vector3 point = (2 * t * t * t - 3 * t * t + 1) * start + (t * t * t - 2 * t * t + t) * startNorm + (-2 * t * t * t + 3 * t * t) * end + (t * t * t - t * t) * endNorm;
            Vector3 direction = (6 * t * t - 6 * t) * start + (3 * t * t - 4 * t + 1) * startNorm + (-6 * t * t + 6 * t) * end + (3 * t * t - 2 * t) * endNorm;
            points.Add(point);
            directions.Add(direction);

        }

        return new Tuple<List<Vector3>, List<Vector3>>(points, directions);
    }
}
