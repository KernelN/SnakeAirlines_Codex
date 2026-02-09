using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBody : MonoBehaviour
{
    [SerializeField] private int initialGrowth = 5;
    [SerializeField] int minTrimIndex = 2;
    [SerializeField] private float segmentSpacing = 0.25f; // Smaller spacing for smoother look
    [SerializeField] private float pathResolution = 0.05f; // How often to record the path

    // "pathHistory" stores the raw path the head has taken
    private readonly List<Vector2> pathHistory = new();
    
    // "bodySegments" are the calculated points along that path (for collision/visuals)
    private readonly List<Vector2> bodySegments = new();

    private LineRenderer trailRenderer;
    private int targetSegments;

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
        pathHistory.Clear();
        bodySegments.Clear();
        targetSegments = initialGrowth;

        // Initialize the path extending backward from the head so the snake starts fully formed
        Vector2 startPos = headPosition;
        Vector2 backDir = -direction.normalized;
        
        // Create a straight line history to start
        float requiredLength = (targetSegments + 1) * segmentSpacing;
        pathHistory.Add(startPos);
        pathHistory.Add(startPos + (backDir * requiredLength));

        UpdateBodySegments(headPosition);
        RefreshVisuals(headPosition);
    }

    // Updated to accept 'direction' (fixing the mismatch with SnakeHead), though we mainly use history now.
    public void Advance(Vector2 headPosition, Vector2 direction, bool shouldGrow)
    {
        if (shouldGrow)
        {
            targetSegments++;
        }

        // 1. Record Path History
        // We add the current head position to the history list
        // To save memory, we only record if the head has moved enough from the last point
        if (pathHistory.Count == 0)
        {
            pathHistory.Insert(0, headPosition);
        }
        else
        {
            float dist = Vector2.Distance(pathHistory[0], headPosition);
            if (dist >= pathResolution)
            {
                pathHistory.Insert(0, headPosition);
            }
            else
            {
                // Always update the very first point to the exact head position 
                // to prevent the neck from appearing detached during small moves
                pathHistory[0] = headPosition;
            }
        }

        // 2. Reconstruct Body Segments
        // This calculates where the body parts should be based on the path history
        UpdateBodySegments(headPosition);

        // 3. Visuals
        RefreshVisuals(headPosition);
        
        // 4. Cleanup Memory
        // Remove history points that are too far back and no longer needed by the tail
        CleanupHistory();
    }

    private void UpdateBodySegments(Vector2 headPosition)
    {
        bodySegments.Clear();
        
        // We walk backwards along the pathHistory to find points at exact intervals
        float currentDistanceWanted = segmentSpacing;
        int segmentsFound = 0;
        float distanceTravelled = 0f;

        // Start from the head (index 0)
        Vector2 prevPoint = pathHistory[0];

        for (int i = 1; i < pathHistory.Count; i++)
        {
            Vector2 currentPoint = pathHistory[i];
            float distToNext = Vector2.Distance(prevPoint, currentPoint);

            // While the current segment of history contains the next body point(s)
            while (distanceTravelled + distToNext >= currentDistanceWanted)
            {
                // Interpolate to find the exact point
                float remainingDist = currentDistanceWanted - distanceTravelled;
                float t = remainingDist / distToNext;
                Vector2 finalPos = Vector2.Lerp(prevPoint, currentPoint, t);

                bodySegments.Add(finalPos);
                segmentsFound++;

                if (segmentsFound >= targetSegments)
                    return; // We have all the segments we need

                currentDistanceWanted += segmentSpacing;
            }

            distanceTravelled += distToNext;
            prevPoint = currentPoint;
        }
    }

    private void CleanupHistory()
    {
        // Calculate max necessary history length
        float maxDist = (targetSegments + 2) * segmentSpacing;
        float currentLen = 0;

        for (int i = 0; i < pathHistory.Count - 1; i++)
        {
            currentLen += Vector2.Distance(pathHistory[i], pathHistory[i+1]);
            if (currentLen > maxDist)
            {
                // Remove everything after this point
                int removeCount = pathHistory.Count - (i + 2);
                if (removeCount > 0)
                {
                    pathHistory.RemoveRange(i + 2, removeCount);
                }
                return;
            }
        }
    }

    public int TrimFromIndex(int collisionIndex, Vector2 headPosition)
    {
        // If we hit segment 5, we keep 0-4. The removed count is (Total - 5).
        int removedSegments = bodySegments.Count - collisionIndex;
        
        // Just reducing the targetSegments is enough; the UpdateBodySegments 
        // function will automatically stop generating the tail next frame.
        targetSegments = collisionIndex;

        // Force an immediate update so the visual cut happens this frame
        UpdateBodySegments(headPosition);
        RefreshVisuals(headPosition);
        
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

    // Collision logic remains effectively the same
    public int FindCollisionIndex(Vector2 headPosition, float collisionRadius)
    {
        if (bodySegments.Count == 0) return -1;

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