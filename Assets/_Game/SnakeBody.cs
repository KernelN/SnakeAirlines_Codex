using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBody : MonoBehaviour
{
    [SerializeField] private int initialGrowth = 2;

    private readonly List<Vector2Int> bodyCells = new();

    private LineRenderer trailRenderer;
    private Board board;

    public IReadOnlyList<Vector2Int> BodyCells => bodyCells;

    private void Awake()
    {
        trailRenderer = GetComponent<LineRenderer>();
    }

    public void Initialize(Board boardReference)
    {
        board = boardReference;
    }

    public void ResetBody(Vector2Int headCell)
    {
        bodyCells.Clear();

        for (int i = 0; i < initialGrowth; i++)
        {
            bodyCells.Add(headCell);
        }

        RefreshVisuals(headCell);
    }

    public void MoveTo(Vector2Int headCell, Vector2Int previousHead, bool shouldGrow)
    {
        bodyCells.Insert(0, previousHead);

        if (!shouldGrow && bodyCells.Count > 0)
        {
            bodyCells.RemoveAt(bodyCells.Count - 1);
        }

        RefreshVisuals(headCell);
    }

    public int FindBodyCollisionIndex(Vector2Int headPosition)
    {
        for (int i = 0; i < bodyCells.Count; i++)
        {
            if (bodyCells[i] == headPosition)
            {
                return i;
            }
        }

        return -1;
    }

    public int TrimFromIndex(int collisionIndex)
    {
        int removedSegments = bodyCells.Count - collisionIndex;

        for (int i = bodyCells.Count - 1; i >= collisionIndex; i--)
        {
            bodyCells.RemoveAt(i);
        }

        return removedSegments;
    }

    public int TotalSegments => bodyCells.Count + 1;

    public void RefreshVisuals(Vector2Int headCell)
    {
        if (board == null)
        {
            return;
        }

        trailRenderer.positionCount = bodyCells.Count + 1;
        trailRenderer.SetPosition(0, board.GridToWorld(headCell));

        for (int i = 0; i < bodyCells.Count; i++)
        {
            trailRenderer.SetPosition(i + 1, board.GridToWorld(bodyCells[i]));
        }
    }
}
