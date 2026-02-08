using UnityEngine;

public class FoodManager : MonoBehaviour
{
    [SerializeField] private SnakeFood foodPrefab;
    [SerializeField] private Board board;
    [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero;
    [SerializeField] private Vector2 spawnAreaSize = new(24f, 16f);
    [SerializeField] private LayerMask spawnBlockingLayers = ~0;
    [SerializeField] private float spawnRaycastDistance = 10f;

    private SnakeFood activeFood;

    public Vector2 CurrentFoodPosition => activeFood != null ? activeFood.WorldPosition : new Vector2(float.MinValue, float.MinValue);

    public void SpawnFood()
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

        Vector2 spawnPosition = FindFreeSpawnPosition();
        activeFood = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
    }

    private Vector2 FindFreeSpawnPosition()
    {
        Vector2 areaCenter = board != null ? board.Center : spawnAreaCenter;
        Vector2 areaSize = board != null ? board.Size : spawnAreaSize;
        Vector2 halfSize = areaSize * 0.5f;
        Vector2 min = areaCenter - halfSize;
        Vector2 max = areaCenter + halfSize;
        int maxTries = Mathf.CeilToInt(areaSize.x * areaSize.y);

        for (int i = 0; i < maxTries; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y));

            if (IsSpawnLocationFree(candidate))
            {
                return candidate;
            }
        }

        return areaCenter;
    }

    private bool IsSpawnLocationFree(Vector2 candidate)
    {
        Vector2 origin = new Vector2(candidate.x, candidate.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, spawnRaycastDistance, spawnBlockingLayers);
        return !hit;
    }
}
