using UnityEngine;
using UnityEngine.Splines;

public class BoundaryGenerator : MonoBehaviour
{
    public SplineContainer splineContainer;

    public GameObject wallPrefab;

    public float roadWidth = 10f;
    public float boundaryOffset = 1f;

    public int samples = 200;

    public Transform boundaryParent;

    public void GenerateBoundaries()
    {
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)samples;

            Vector3 center =
                (Vector3)splineContainer
                .EvaluatePosition(t);

            Vector3 tangent =
                (Vector3)splineContainer
                .EvaluateTangent(t);

            tangent.Normalize();

            Vector3 right =
                Vector3.Cross(
                    Vector3.up,
                    tangent
                ).normalized;

            Vector3 leftPos =
                center -
                right *
                (roadWidth * 0.5f + boundaryOffset);

            Vector3 rightPos =
                center +
                right *
                (roadWidth * 0.5f + boundaryOffset);

            Quaternion rot =
                Quaternion.LookRotation(
                    tangent
                );

            Instantiate(
                wallPrefab,
                leftPos,
                rot,
                boundaryParent
            );

            Instantiate(
                wallPrefab,
                rightPos,
                rot,
                boundaryParent
            );
        }
    }
}