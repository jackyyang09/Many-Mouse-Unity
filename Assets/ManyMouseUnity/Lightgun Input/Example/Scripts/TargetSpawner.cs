using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGroup
{

}
public enum MovementStyle
{
    Fixed,
    Oscellate,
    PingPong,
}
public enum ShapeType
{
    Line,
    Curve,
    Circle,
    Random
}
public class TargetSpawner : MonoBehaviour
{
    public Game game;
    public new Camera camera;
    public AnimationCurve distanceSpawnChance;
    public float timeUntilSpawn;
    public AnimationCurve spawnRateOverGameTime;
    public Vector2 randomSpawnInterval = new Vector2(1, 3);
    public Target targetPrefab;
    public float viewportSize = 0.9f;
    public Rect viewportSpawnRect => new Rect((1f - viewportSize) * 0.5f, (1f - viewportSize) * 0.5f, viewportSize, viewportSize);
    float waveCompletionBonusTime = 0;

    void OnEnable()
    {
        // timeUntilSpawn = 0f;
        Spawn();
    }
    void OnDisable()
    {
        Clear();
    }

    void Update()
    {
        if (FindObjectsOfType<Target>().Length == 0)
        {
            WaveComplete();
            Spawn();
        }
        // timeUntilSpawn -= Time.deltaTime;
        // if(timeUntilSpawn < 0) {
        //     timeUntilSpawn += Random.Range(randomSpawnInterval.x, randomSpawnInterval.y) / spawnRateOverGameTime.Evaluate(game.gameTime);
        // }
    }

    public void Clear()
    {
        foreach (var target in FindObjectsOfType<Target>())
        {
            Destroy(target.gameObject);
        }
    }


    void Spawn()
    {
        // distanceSpawnChance.keys[distanceSpawnChance.keys.Length - 1].time
        // var areaUnderCurve = AreaUnderCurve(distanceSpawnChance, 1, 1);
        // var distance = distanceSpawnChance.Evaluate(Random.value);
        // Vector3[] outCorners = new Vector3[4];
        // camera.CalculateFrustumCorners(viewportSpawnRect, distance, Camera.MonoOrStereoscopicEye.Mono, outCorners);
        // var position = camera.transform.position + new Vector3(Mathf.Lerp(outCorners[0].x, outCorners[2].x, Random.value), Mathf.Lerp(outCorners[0].y, outCorners[2].y, Random.value), Mathf.Lerp(outCorners[0].z, outCorners[2].z, Random.value));
        // Instantiate(targetPrefab, position, targetPrefab.transform.rotation);

        ShapeType shapeType = Random.value < 0.5 ? ShapeType.Line : ShapeType.Circle;
        if (shapeType == ShapeType.Line)
        {
            var startDistance = distanceSpawnChance.Evaluate(Random.value);
            var endDistance = distanceSpawnChance.Evaluate(Random.value);
            var startViewportPos = new Vector3(Random.Range(viewportSpawnRect.xMin, viewportSpawnRect.xMax), Random.Range(viewportSpawnRect.yMin, viewportSpawnRect.yMax), startDistance);
            Vector3 endViewportPos = Vector3.zero;
            do
            {
                endViewportPos = new Vector3(Random.Range(viewportSpawnRect.xMin, viewportSpawnRect.xMax), Random.Range(viewportSpawnRect.yMin, viewportSpawnRect.yMax), endDistance);
            } 
            while (Vector2.Distance(startViewportPos, endViewportPos) < 0.4f);

            var lineStart = camera.ViewportToWorldPoint(startViewportPos);
            var lineEnd = camera.ViewportToWorldPoint(endViewportPos);
            var lineLength = Vector3.Distance(lineStart, lineEnd);
            var minSpacing = 1.75f;
            var numTargets = Mathf.Max(Mathf.FloorToInt(lineLength / minSpacing), 1);
            var separation = lineLength / numTargets;

            for (int i = 0; i < numTargets; i++)
            {
                var target = Instantiate(targetPrefab, lineStart + (lineEnd - lineStart) * i / numTargets, targetPrefab.transform.rotation);
                target.movementStyle = MovementStyle.Fixed;
            }
            waveCompletionBonusTime = Mathf.Min(numTargets * 0.5f, 3f);
        }
        else if (shapeType == ShapeType.Curve)
        {

        }
        else if (shapeType == ShapeType.Circle)
        {

        }
        else if (shapeType == ShapeType.Random)
        {

        }
    }

    void WaveComplete()
    {
        Game.Instance.remainingTime += waveCompletionBonusTime;
        Object.Instantiate<GainTimeUI>(Game.Instance.gainTimeUIPrefab, Game.Instance.canvas.transform.position, Game.Instance.canvas.transform.rotation, Game.Instance.canvas.transform).Init(waveCompletionBonusTime);
    }

    // https://answers.unity.com/questions/1259647/calculate-surface-under-a-curve-from-an-animationc.html
    public float AreaUnderCurve(AnimationCurve curve, float w, float h)
    {
        float areaUnderCurve = 0f;
        var keys = curve.keys;

        for (int i = 0; i < keys.Length - 1; i++)
        {
            // Calculate the 4 cubic Bezier control points from Unity AnimationCurve (a hermite cubic spline) 
            Keyframe K1 = keys[i];
            Keyframe K2 = keys[i + 1];
            Vector2 A = new Vector2(K1.time * w, K1.value * h);
            Vector2 D = new Vector2(K2.time * w, K2.value * h);
            float e = (D.x - A.x) / 3.0f;
            float f = h / w;
            Vector2 B = A + new Vector2(e, e * f * K1.outTangent);
            Vector2 C = D + new Vector2(-e, -e * f * K2.inTangent);

            /*
            * The cubic Bezier curve function looks like this:
            * 
            * f(x) = A(1 - x)^3 + 3B(1 - x)^2 x + 3C(1 - x) x^2 + Dx^3
            * 
            * Where A, B, C and D are the control points and, 
            * for the purpose of evaluating an instance of the Bezier curve, 
            * are constants. 
            * 
            * Multiplying everything out and collecting terms yields the expanded polynomial form:
            * f(x) = (-A + 3B -3C + D)x^3 + (3A - 6B + 3C)x^2 + (-3A + 3B)x + A
            * 
            * If we say:
            * a = -A + 3B - 3C + D
            * b = 3A - 6B + 3C
            * c = -3A + 3B
            * d = A
            * 
            * Then we have the expanded polynomal:
            * f(x) = ax^3 + bx^2 + cx + d
            * 
            * Whos indefinite integral is:
            * a/4 x^4 + b/3 x^3 + c/2 x^2 + dx + E
            * Where E is a new constant introduced by integration.
            * 
            * The indefinite integral of the quadratic Bezier curve is:
            * (-A + 3B - 3C + D)/4 x^4 + (A - 2B + C) x^3 + 3/2 (B - A) x^2 + Ax + E
            */

            float a, b, c, d;
            a = -A.y + 3.0f * B.y - 3.0f * C.y + D.y;
            b = 3.0f * A.y - 6.0f * B.y + 3.0f * C.y;
            c = -3.0f * A.y + 3.0f * B.y;
            d = A.y;

            /* 
            * a, b, c, d, now contain the y component from the Bezier control points.
            * In other words - the AnimationCurve Keyframe value * h data!
            * 
            * What about the x component for the Bezier control points - the AnimationCurve
            * time data?  We will need to evaluate the x component when time = 1.
            * 
            * x^4, x^3, X^2, X all equal 1, so we can conveniently drop this coeffiecient.
            * 
            * Lastly, for each segment on the AnimationCurve we get the time difference of the 
            * Keyframes and multiply by w.
            * 
            * Iterate through the segments and add up all the areas for 
            * the total area under the AnimationCurve!
            */

            float t = (K2.time - K1.time) * w;

            float area = ((a / 4.0f) + (b / 3.0f) + (c / 2.0f) + d) * t;

            areaUnderCurve += area;
        }
        return areaUnderCurve;
    }

}
