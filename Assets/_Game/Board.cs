using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private Vector2Int boardSize = new(24, 16);
    [SerializeField] private float cellSize = 1f;

    public Vector2 WorldSize => new Vector2(boardSize.x * cellSize, boardSize.y * cellSize);

    public Vector2Int GetStartCell()
    {
        return new Vector2Int(boardSize.x / 2, boardSize.y / 2);
    }

    public Vector2Int WrapPosition(Vector2Int position)
    {
        int x = (position.x + boardSize.x) % boardSize.x;
        int y = (position.y + boardSize.y) % boardSize.y;
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * cellSize, gridPosition.y * cellSize, 0f);
    }

    public Vector2 WrapWorldPosition(Vector2 position)
    {
        Vector2 size = WorldSize;
        float wrappedX = Mathf.Repeat(position.x, size.x);
        float wrappedY = Mathf.Repeat(position.y, size.y);
        return new Vector2(wrappedX, wrappedY);
    }

    public Vector2 ClampWorldPosition(Vector2 position)
    {
        Vector2 size = WorldSize;
        float clampedX = Mathf.Clamp(position.x, 0f, size.x);
        float clampedY = Mathf.Clamp(position.y, 0f, size.y);
        return new Vector2(clampedX, clampedY);
    }

    public Vector2Int GetRandomFreeCell(IReadOnlyList<Vector2Int> occupied)
    {
        int maxTries = boardSize.x * boardSize.y;
        for (int i = 0; i < maxTries; i++)
        {
            Vector2Int random = new(
                Random.Range(0, boardSize.x),
                Random.Range(0, boardSize.y));

            bool isOccupied = false;
            for (int j = 0; j < occupied.Count; j++)
            {
                if (occupied[j] == random)
                {
                    isOccupied = true;
                    break;
                }
            }

            if (!isOccupied)
            {
                return random;
            }
        }

        return Vector2Int.zero;
    }

    public Vector2 GetRandomFreePosition(IReadOnlyList<Vector2> occupied, float minDistance)
    {
        int maxTries = boardSize.x * boardSize.y;
        for (int i = 0; i < maxTries; i++)
        {
            Vector2Int randomCell = new(
                Random.Range(0, boardSize.x),
                Random.Range(0, boardSize.y));
            Vector2 position = new Vector2(randomCell.x * cellSize, randomCell.y * cellSize);

            bool isOccupied = false;
            for (int j = 0; j < occupied.Count; j++)
            {
                if (Vector2.Distance(occupied[j], position) <= minDistance)
                {
                    isOccupied = true;
                    break;
                }
            }

            if (!isOccupied)
            {
                return position;
            }
        }

        return Vector2.zero;
    }
}
