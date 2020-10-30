using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingPath : MonoBehaviour
{
    public float granularity = .05f;
    public LineRenderer lr;
    public Transform face;
    /// <summary>
    /// Center of rotation of arms
    /// </summary>
    public Transform body;

    //How quickly wind up
    public float windSpeedLinear = 10;
    public float swingTorque = 30;
    public float angularAdjustmentSpeed = 100;

    public int swingRadius;
    internal bool canHit(Transform start, Vector3 targetPos, Vector3 endDir)
    {
        Vector3 delta = targetPos - start.position;
        Vector3 deltaLocal = start.InverseTransformDirection(delta);
        bool leftSide = false;
        if (deltaLocal.x < 0)
            leftSide = true;

        bool leftExit = start.InverseTransformDirection(endDir).x < 0;// && sign != -1;

        bool largeSwing = leftSide ^ leftExit;

        Vector3 BToE = targetPos - body.position;
        float a = Vector3.Dot(endDir, endDir);
        float b = -2 * Vector3.Dot(endDir, BToE);
        float c = Vector3.Dot(BToE, BToE) -swingRadius * swingRadius;
        float inSqrt = b * b - 4 * a * c;
        if (inSqrt < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    // Update is called once per frame
    internal Vector2 calculateTimeAndForce(Transform start, Vector3 targetCenter, Vector3 targetPos, Vector3 startDir, Vector3 endDir)//, Vector3 windEndPos, Vector3 windEndDir)
    {


        Vector3 delta = targetPos - start.position;
        Vector3 deltaLocal = start.InverseTransformDirection(delta);
        bool leftSide = false;
        if (deltaLocal.x < 0)
            leftSide = true;

        bool leftExit = start.InverseTransformDirection(endDir).x < 0;// && sign != -1;

        bool largeSwing = leftSide ^ leftExit;

        //print("Left side? " + leftSide + " Left exit? " + leftExit + " largeSwing? " + largeSwing);

        //print("Will use large swing: " + largeSwing);

        //which face hits the ball?
        if (leftExit)
            startDir *= -1;

        //print("Hit with back face: " + leftExit);
        //if (isBackHand)
        //    startDir *= -1;
        List<Vector3> points;
        List<Vector3> directions;
        int swapIndex;
        if (largeSwing)//compute two-part hit
        {


            Vector3 windEndPos = calculateWindPos(endDir, targetPos, body.position, swingRadius);
            if (windEndPos == Vector3.zero)
            {
                return Vector2.zero;
            }

            Vector3 calculatedPosRotDir = (Quaternion.FromToRotation(start.position - body.position, windEndPos - body.position) * startDir).normalized;

            Vector3 windEndDir = -calculatedPosRotDir;

            //Wind
            var pointData = generatePoints(start.position, windEndPos, startDir, windEndDir);
            points = pointData.Item1;
            directions = pointData.Item2;


            //Swing
            swapIndex = points.Count - 1;

            //targetPos += (endDir + -sign * start.parent.right)/2 * .5f; //follow through
            pointData = generatePoints(windEndPos, targetPos, -windEndDir, endDir);
            points.AddRange(pointData.Item1);
            directions.AddRange(pointData.Item2);
        }
        else//compute simple parabola hit
        {
            var pointData = generatePoints(start.position, targetPos, -startDir, endDir);
            points = pointData.Item1;
            directions = pointData.Item2;
            swapIndex = points.Count / 2;
        }




        lr.positionCount = 0;
        for (int i = 0; i < points.Count; i++)
        {
            lr.positionCount += 1;
            //point is centroid of tube
            lr.SetPosition(i, points[i]);
        }
        StartCoroutine(followSequence(points, directions, swapIndex,start));
        return calculateTimeAndForce(points, directions, swapIndex);
    }
    Vector2 calculateTimeAndForce(List<Vector3> points, List<Vector3> directions, int swapIndex)
    {
        float currentSpeed = 0;
        Vector3 position = points[swapIndex + 1];
        float totalTime = 0;
        for (int i = 1; i < swapIndex; i++)
        {
            float startTime = Time.fixedTime;

            Vector3 startDir = directions[i - 1];
            Vector3 endDir = directions[i];
            Vector3 startPos = points[i - 1];
            Vector3 endPos = points[i];
            totalTime += Vector3.Dot(startDir, endDir) / angularAdjustmentSpeed + (startPos - endPos).magnitude / windSpeedLinear;
        }

        for (int i = swapIndex + 2; i < points.Count; i++)
        {
            Vector3 startDir = directions[i - 1];
            Vector3 endDir = directions[i];
            Vector3 startPos = points[i - 1];
            Vector3 endPos = points[i];
            //Guaranteed that previous velocity and next are in their respective directions, and that magnitude will be previous + remaning acceleration;
            float lossTheta = Vector3.Angle(startDir, endDir);

            float availableAcceleration = Mathf.Max(0, swingTorque - lossTheta);

            float distance = (startPos - endPos).magnitude;

            float time = (-currentSpeed + Mathf.Sqrt(currentSpeed * currentSpeed + 2 * availableAcceleration * distance)) / availableAcceleration;
            totalTime += time;
            currentSpeed += availableAcceleration * time;
        }

        return new Vector2(totalTime, currentSpeed);
    }
    Vector3 calculateWindPos(Vector3 endDir, Vector3 endPos, Vector3 bodyPos, float radius)
    {
        Vector3 BToE = endPos - bodyPos;
        float a = Vector3.Dot(endDir, endDir);
        float b = -2 * Vector3.Dot(endDir, BToE);
        float c = Vector3.Dot(BToE, BToE) - radius * radius;
        float inSqrt = b * b - 4 * a * c;
        if (inSqrt < 0)
        {
            print("No solution without shift");
            return Vector3.zero;
        }
        float n1 = (-b + Mathf.Sqrt(inSqrt)) / (2 * a);
        float n2 = (-b - Mathf.Sqrt(inSqrt)) / (2 * a);
        //print(a * n1 * n1 + b * n1 + c);
        //print(a * n2 * n2 + b * n2 + c);
        //print("Solution 1 n: " + n1 + ", Solution 2 n: " + n2);

        Vector3 windEndPos;

        if (n1 > 0)
        {
            n1 *= .5f;
            windEndPos = -n1 * endDir + endPos;
        }
        else if (n2 > 0)
        {
            n2 *= .5f;
            windEndPos = -n2 * endDir + endPos;
        }
        else
        {
            if (n1 > 0 && n2 > 0)
            {
                windEndPos = -Mathf.Max(n1, n2) * endDir + endPos;
            }
            else
            {
                print("No solution");
                return Vector3.zero;
            }
        }
        //print(windEndPos + " " + (windEndPos - bodyPos).magnitude + " radius: " + radius);
        return windEndPos;
    }
    IEnumerator followSequence(List<Vector3> points, List<Vector3> directions, int swapIndex, Transform obj)
    {

        for (int i = 1; i < swapIndex; i++)
        {
            float startTime = Time.fixedTime;

            Vector3 startDir = directions[i - 1];
            Vector3 endDir = directions[i];
            Vector3 startPos = points[i - 1];
            Vector3 endPos = points[i];
            float totalDelta = Vector3.Dot(startDir, endDir) / angularAdjustmentSpeed + (startPos - endPos).magnitude / windSpeedLinear;
            float endTime = startTime + totalDelta;
            while (Time.fixedTime < endTime)
            {
                float progress = (Time.fixedTime - startTime) / totalDelta;
                face.position = Vector3.Lerp(startPos, endPos, progress);
                face.forward = Vector3.Lerp(startDir, endDir, progress);
                yield return new WaitForFixedUpdate();
            }
        }
        float currentSpeed = 0;
        Vector3 position = points[swapIndex + 1];
        for (int i = swapIndex + 2; i < points.Count; i++)
        {
            float startTime = Time.fixedTime;

            Vector3 startDir = directions[i - 1];
            Vector3 endDir = directions[i];
            Vector3 startPos = points[i - 1];
            Vector3 endPos = points[i];

            //Guaranteed that previous velocity and next are in their respective directions, and that magnitude will be previous + remaning acceleration;
            float lossTheta = Vector3.Angle(startDir, endDir);

            float availableAcceleration = Mathf.Max(0, swingTorque - lossTheta);

            float distance = (startPos - endPos).magnitude;

            float time = (-currentSpeed + Mathf.Sqrt(currentSpeed * currentSpeed + 2 * availableAcceleration * distance)) / availableAcceleration;

            //float totalDelta = Vector3.Dot(startDir, endDir) / angularAdjustmentSpeed + (startPos - endPos).magnitude / swingTorque;
            float endTime = startTime + time;// totalDelta;
            //Guarantee: Each point is a flat line apart. Direction is merely the orientation
            while (Time.fixedTime < endTime)
            {
                float progress = (Time.fixedTime - startTime) / time;
                face.position = Vector3.Lerp(startPos, endPos, progress);
                face.forward = Vector3.Lerp(startDir, endDir, progress);
                yield return new WaitForFixedUpdate();
            }
            currentSpeed += availableAcceleration * time;
        }
        if (float.IsNaN(currentSpeed))
        {
            print("NANI");
        }

        Quaternion before = face.localRotation;
        Quaternion after = Quaternion.Euler(0, 90, 45);
        Vector3 beforeLocal = face.localPosition;
        Vector3 afterLocal = new Vector3(0, 0.035f, 0.837f);
         float startTimeE = Time.fixedTime;
         float endTimeE = startTimeE + .5f;
        while(Time.fixedTime < endTimeE)
        {
            float progress = (Time.fixedTime - startTimeE) / .5f;
            face.localPosition = Vector3.Slerp(beforeLocal, afterLocal, progress);
            face.localRotation = Quaternion.Slerp(before, after, progress);
            yield return new WaitForFixedUpdate();
        }
        face.localRotation = after;
        face.localPosition = afterLocal;

    }
    Tuple<List<Vector3>, List<Vector3>, float, float> generatePoints(Vector3 start, Vector3 end, Vector3 startNorm, Vector3 endNorm)
    {
        List<Vector3> points = new List<Vector3>();
        List<Vector3> directions = new List<Vector3>();
        int iterations = Mathf.RoundToInt(1f / granularity);
        Vector3 prevDirection = startNorm;
        Vector3 prevPoint = start;

        points.Add(start);
        directions.Add(startNorm);

        float totalDistance = 0;
        float totalTheta = 0;
        for (int i = 0; i <= iterations; i++)
        {
            float t = i * granularity;
            Vector3 point = (2 * t * t * t - 3 * t * t + 1) * start + (t * t * t - 2 * t * t + t) * startNorm + (-2 * t * t * t + 3 * t * t) * end + (t * t * t - t * t) * endNorm;
            Vector3 direction = (6 * t * t - 6 * t) * start + (3 * t * t - 4 * t + 1) * startNorm + (-6 * t * t + 6 * t) * end + (3 * t * t - 2 * t) * endNorm;
            //If curved enough, add point and move on. Else check if next point is bent enough
            if (Vector3.Angle(direction, prevDirection) > 5f)
            {
                points.Add(point);
                directions.Add(direction);

                totalDistance += (point - prevPoint).magnitude;
                totalTheta += Vector3.Angle(direction, prevDirection);

                prevDirection = direction;
                prevPoint = point;

            }
            else
            {
                continue;
            }

        }
        if ((points[points.Count - 1] != end))
        {
            points.Add(end);
            directions.Add(endNorm);
            totalDistance += (end - prevPoint).magnitude;
            totalTheta += Vector3.Angle(endNorm, prevDirection);
        }
        return new Tuple<List<Vector3>, List<Vector3>, float, float>(points, directions, totalDistance, totalTheta);
    }
}
