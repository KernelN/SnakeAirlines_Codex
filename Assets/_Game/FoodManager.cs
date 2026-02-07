using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    [SerializeField] private SnakeFood foodPrefab;
    [SerializeField] private float spawnClearance = 0.5f;

    private SnakeFood activeFood;

    public Vector2 CurrentFoodPosition => activeFood != null ? activeFood.WorldPosition : new Vector2(float.MinValue, float.MinValue);

    public void SpawnFood(Board board, IReadOnlyList<Vector2> occupiedPositions)
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

        Vector2 spawnPosition = board.GetRandomFreePosition(occupiedPositions, spawnClearance);
        activeFood = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
    }

    public bool IsFoodNear(Vector2 position, float radius)
    {
        return activeFood != null && Vector2.Distance(activeFood.WorldPosition, position) <= radius;
    }
}
