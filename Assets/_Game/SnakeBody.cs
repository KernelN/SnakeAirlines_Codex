using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBody : MonoBehaviour
{
    [SerializeField] private int initialGrowth = 2;
    [SerializeField] int minTrimIndex = 2;
    [SerializeField] private float segmentSpacing = 0.5f;
    [SerializeField] private float safeDistance = 0.3f;

    private readonly List<Vector2> bodySegments = new();

    private LineRenderer trailRenderer;
    private int targetSegments;
    private Vector2 lastSegmentAnchor;
    private Vector2 lastMoveDirection = Vector2.right;

    public IReadOnlyList<Vector2> BodySegments => bodySegments;
    public float SegmentSpacing => segmentSpacing;

    private void Awake()
    {
        trailRenderer = GetComponent<LineRenderer>();
        if (TryGetComponent(out EdgeCollider2D collider))
        {
            collider.enabled = false;
        }
    }

    public void ResetBody(Vector2 headPosition, Vector2 direction)
    {
        bodySegments.Clear();
        targetSegments = initialGrowth;
        lastMoveDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
        lastSegmentAnchor = headPosition - (lastMoveDirection * (safeDistance + segmentSpacing));

        for (int i = 0; i < targetSegments; i++)
        {
            bodySegments.Add(headPosition - (lastMoveDirection * (safeDistance + segmentSpacing * (i + 1))));
        }

        RefreshVisuals(headPosition);
    }

    public void Advance(Vector2 headPosition, Vector2 direction, bool shouldGrow)
    {
        if (shouldGrow)
        {
            targetSegments++;
        }

        if (direction.sqrMagnitude > 0.001f)
        {
            lastMoveDirection = direction.normalized;
        }

        Vector2 safeAnchor = headPosition - (lastMoveDirection * safeDistance);
        Vector2 firstSegmentTarget = safeAnchor - (lastMoveDirection * segmentSpacing);
        float distanceFromAnchor = Vector2.Distance(lastSegmentAnchor, firstSegmentTarget);
        while (distanceFromAnchor >= segmentSpacing)
        {
            Vector2 anchorDirection = (firstSegmentTarget - lastSegmentAnchor).normalized;
            lastSegmentAnchor += anchorDirection * segmentSpacing;
            if (bodySegments.Count < targetSegments)
            {
                bodySegments.Insert(0, lastSegmentAnchor);
            }

            UpdateTrailingSegments();
            distanceFromAnchor = Vector2.Distance(lastSegmentAnchor, firstSegmentTarget);
        }

        if (bodySegments.Count < targetSegments)
        {
            AddTailSegment();
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

    public int TotalSegments => bodySegments.Count + 2;

    public void RefreshVisuals(Vector2 headPosition)
    {
        trailRenderer.positionCount = bodySegments.Count + 2;
        trailRenderer.SetPosition(0, new Vector3(headPosition.x, headPosition.y, 0f));

        Vector2 safePoint = headPosition - (lastMoveDirection * safeDistance);
        trailRenderer.SetPosition(1, new Vector3(safePoint.x, safePoint.y, 0f));

        for (int i = 0; i < bodySegments.Count; i++)
        {
            Vector2 segment = bodySegments[i];
            trailRenderer.SetPosition(i + 2, new Vector3(segment.x, segment.y, 0f));
        }
    }

    private void UpdateTrailingSegments()
    {
        if (bodySegments.Count == 0)
        {
            return;
        }

        bodySegments[0] = lastSegmentAnchor;
        Vector2 previous = bodySegments[0];
        for (int i = 1; i < bodySegments.Count; i++)
        {
            Vector2 current = bodySegments[i];
            Vector2 toCurrent = previous - current;
            Vector2 segmentDirection = toCurrent.sqrMagnitude > 0.001f ? toCurrent.normalized : -lastMoveDirection;
            bodySegments[i] = previous - (segmentDirection * segmentSpacing);
            previous = bodySegments[i];
        }
    }

    private void AddTailSegment()
    {
        if (bodySegments.Count == 0)
        {
            bodySegments.Add(lastSegmentAnchor);
            return;
        }

        Vector2 tailDirection = -lastMoveDirection;
        if (bodySegments.Count >= 2)
        {
            Vector2 direction = bodySegments[^1] - bodySegments[^2];
            if (direction.sqrMagnitude > 0.001f)
            {
                tailDirection = direction.normalized;
            }
        }

        bodySegments.Add(bodySegments[^1] + (tailDirection * segmentSpacing));
    }

    public int FindClosestSegmentIndex(Vector2 worldPoint)
    {
        if (bodySegments.Count == 0)
        {
            return -1;
        }

        int closestIndex = -1;
        float closestDistanceSquared = float.MaxValue;

        for (int i = minTrimIndex; i < bodySegments.Count; i++)
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

    public int FindCollisionIndex(Vector2 headPosition, float collisionRadius)
    {
        if (bodySegments.Count == 0)
        {
            return -1;
        }

        float radiusSquared = collisionRadius * collisionRadius;
        for (int i = minTrimIndex; i < bodySegments.Count; i++)
        {
            if ((bodySegments[i] - headPosition).sqrMagnitude <= radiusSquared)
            {
                return i;
            }
        }

        return -1;
    }
}
