using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    [SerializeField] private SnakeFood foodPrefab;

    private SnakeFood activeFood;

    public Vector2Int CurrentFoodCell => activeFood != null ? activeFood.GridPosition : new Vector2Int(int.MinValue, int.MinValue);

    public void SpawnFood(Board board, IReadOnlyList<Vector2Int> occupiedCells)
    {
        if (foodPrefab == null)
        {
            Debug.LogWarning("Food prefab is missing on FoodManager.");
            return;
        }

        if (activeFood != null)
        {
            Destroy(activeFood.gameObject);
        }

        Vector2Int spawnCell = board.GetRandomFreeCell(occupiedCells);
        activeFood = Instantiate(foodPrefab, board.GridToWorld(spawnCell), Quaternion.identity);
        activeFood.SetGridPosition(spawnCell);
    }

    public bool IsFoodAt(Vector2Int cell)
    {
        return activeFood != null && activeFood.GridPosition == cell;
    }
}
