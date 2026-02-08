using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class SnakeBody : MonoBehaviour
{
    [SerializeField] private int initialGrowth = 2;
    [SerializeField] private float segmentSpacing = 0.5f;

    private readonly List<Vector2> bodySegments = new();

    private LineRenderer trailRenderer;
    private EdgeCollider2D edgeCollider;
    private int targetSegments;
    private Vector2 lastSegmentAnchor;

    public IReadOnlyList<Vector2> BodySegments => bodySegments;

    private void Awake()
    {
        trailRenderer = GetComponent<LineRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();
    }

    public void ResetBody(Vector2 headPosition, Vector2 direction)
    {
        bodySegments.Clear();
        targetSegments = initialGrowth;
        lastSegmentAnchor = headPosition;

        for (int i = 0; i < targetSegments; i++)
        {
            bodySegments.Add(headPosition - (direction.normalized * segmentSpacing * (i + 1)));
        }

        RefreshVisuals(headPosition);
    }

    public void Advance(Vector2 headPosition, bool shouldGrow)
    {
        if (shouldGrow)
        {
            targetSegments++;
        }

        float distanceFromAnchor = Vector2.Distance(lastSegmentAnchor, headPosition);
        while (distanceFromAnchor >= segmentSpacing)
        {
            Vector2 direction = (headPosition - lastSegmentAnchor).normalized;
            lastSegmentAnchor += direction * segmentSpacing;
            bodySegments.Insert(0, lastSegmentAnchor);
            distanceFromAnchor = Vector2.Distance(lastSegmentAnchor, headPosition);

            if (bodySegments.Count > targetSegments)
            {
                bodySegments.RemoveAt(bodySegments.Count - 1);
            }
        }

        if (bodySegments.Count < targetSegments && bodySegments.Count > 0)
        {
            bodySegments.Add(bodySegments[^1]);
        }

        RefreshVisuals(headPosition);
    }

    public int TrimFromIndex(int collisionIndex, Vector2 headPosition)
    {
        int removedSegments = bodySegments.Count - collisionIndex;

        for (int i = bodySegments.Count - 1; i >= collisionIndex; i--)
        {
            bodySegments.RemoveAt(i);
        }

        RefreshVisuals(headPosition);
        targetSegments = bodySegments.Count;
        return removedSegments;
    }

    public int TotalSegments => bodySegments.Count + 1;

    public void RefreshVisuals(Vector2 headPosition)
    {
        trailRenderer.positionCount = bodySegments.Count + 1;
        trailRenderer.SetPosition(0, new Vector3(headPosition.x, headPosition.y, 0f));

        for (int i = 0; i < bodySegments.Count; i++)
        {
            Vector2 segment = bodySegments[i];
            trailRenderer.SetPosition(i + 1, new Vector3(segment.x, segment.y, 0f));
        }

        UpdateEdgeCollider();
    }

    public int FindClosestSegmentIndex(Vector2 worldPoint)
    {
        if (bodySegments.Count == 0)
        {
            return -1;
        }

        int closestIndex = 0;
        float closestDistanceSquared = float.MaxValue;

        for (int i = 0; i < bodySegments.Count; i++)
        {
            float distanceSquared = (bodySegments[i] - worldPoint).sqrMagnitude;
            if (distanceSquared < closestDistanceSquared)
            {
                closestDistanceSquared = distanceSquared;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    public bool TryFindCollisionIndex(Vector2 headPosition, float collisionRadius, int ignoreSegments, out int hitIndex)
    {
        hitIndex = -1;
        if (bodySegments.Count == 0)
        {
            return false;
        }

        float radiusSquared = collisionRadius * collisionRadius;
        int startIndex = Mathf.Clamp(ignoreSegments, 0, bodySegments.Count);

        for (int i = startIndex; i < bodySegments.Count; i++)
        {
            if ((bodySegments[i] - headPosition).sqrMagnitude <= radiusSquared)
            {
                hitIndex = i;
                return true;
            }
        }

        return false;
    }

    private void UpdateEdgeCollider()
    {
        if (edgeCollider == null)
        {
            return;
        }

        int totalPoints = bodySegments.Count;
        if (totalPoints < 2)
        {
            edgeCollider.points = System.Array.Empty<Vector2>();
            return;
        }

        Vector2[] points = new Vector2[totalPoints];
        for (int i = 0; i < bodySegments.Count; i++)
        {
            points[i] = transform.InverseTransformPoint(bodySegments[i]);
        }

        edgeCollider.points = points;
    }
}
