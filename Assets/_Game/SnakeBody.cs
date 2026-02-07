using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBody : MonoBehaviour
{
    [SerializeField] private int initialGrowth = 2;
    [SerializeField] private float segmentSpacing = 0.5f;

    private readonly List<Vector2> bodySegments = new();

    private LineRenderer trailRenderer;
    private int targetSegments;
    private Vector2 lastSegmentAnchor;

    public IReadOnlyList<Vector2> BodySegments => bodySegments;

    private void Awake()
    {
        trailRenderer = GetComponent<LineRenderer>();
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

    public int FindBodyCollisionIndex(Vector2 headPosition, float collisionRadius)
    {
        float radiusSquared = collisionRadius * collisionRadius;
        for (int i = 0; i < bodySegments.Count; i++)
        {
            if ((bodySegments[i] - headPosition).sqrMagnitude <= radiusSquared)
            {
                return i;
            }
        }

        return -1;
    }

    public int TrimFromIndex(int collisionIndex)
    {
        int removedSegments = bodySegments.Count - collisionIndex;

        for (int i = bodySegments.Count - 1; i >= collisionIndex; i--)
        {
            bodySegments.RemoveAt(i);
        }

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
    }
}
