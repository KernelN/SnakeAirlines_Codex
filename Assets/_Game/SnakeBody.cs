using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SnakeBody : MonoBehaviour
{
    private readonly List<Vector2Int> snakeCells = new();

    private LineRenderer trailRenderer;
    private Board board;

    public IReadOnlyList<Vector2Int> SnakeCells => snakeCells;
    public Vector2Int HeadCell => snakeCells.Count > 0 ? snakeCells[0] : Vector2Int.zero;

    private void Awake()
    {
        trailRenderer = GetComponent<LineRenderer>();
    }

    public void Initialize(Board boardReference)
    {
        board = boardReference;
    }

    public void ResetBody(Vector2Int startCell, int initialGrowth)
    {
        snakeCells.Clear();
        snakeCells.Add(startCell);

        for (int i = 0; i < initialGrowth; i++)
        {
            snakeCells.Add(startCell);
        }

        RefreshVisuals();
    }

    public void MoveTo(Vector2Int newHead, bool shouldGrow)
    {
        snakeCells.Insert(0, newHead);

        if (!shouldGrow && snakeCells.Count > 0)
        {
            snakeCells.RemoveAt(snakeCells.Count - 1);
        }

        RefreshVisuals();
    }

    public int FindBodyCollisionIndex(Vector2Int headPosition)
    {
        for (int i = 1; i < snakeCells.Count; i++)
        {
            if (snakeCells[i] == headPosition)
            {
                return i;
            }
        }

        return -1;
    }

    public int TrimFromIndex(int collisionIndex)
    {
        int removedSegments = snakeCells.Count - collisionIndex;

        for (int i = snakeCells.Count - 1; i >= collisionIndex; i--)
        {
            snakeCells.RemoveAt(i);
        }

        RefreshVisuals();
        return removedSegments;
    }

    private void RefreshVisuals()
    {
        if (board == null || snakeCells.Count == 0)
        {
            return;
        }

        transform.position = board.GridToWorld(snakeCells[0]);

        trailRenderer.positionCount = snakeCells.Count;
        for (int i = 0; i < snakeCells.Count; i++)
        {
            trailRenderer.SetPosition(i, board.GridToWorld(snakeCells[i]));
        }
    }
}
