using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private Vector2Int boardSize = new(24, 16);
    [SerializeField] private float cellSize = 1f;

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
}
