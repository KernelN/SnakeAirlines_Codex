using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    [SerializeField] private SnakeFood[] foodPrefabs;
    [SerializeField] private Board board;
    [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero;
    [SerializeField] private Vector2 spawnAreaSize = new(24f, 16f);
    [SerializeField] private LayerMask spawnBlockingLayers = ~0;
    [SerializeField] private float spawnRaycastDistance = 10f;
    [SerializeField] private FoodParticleController[] foodEatEffectPool;

    private SnakeFood activeFood;
    private readonly Queue<FoodParticleController> availableEatEffects = new();

    public Vector2 CurrentFoodPosition => activeFood != null ? activeFood.WorldPosition : new Vector2(float.MinValue, float.MinValue);

    private void Awake()
    {
        if (foodEatEffectPool == null)
        {
            return;
        }

        foreach (FoodParticleController effect in foodEatEffectPool)
        {
            if (effect == null)
            {
                continue;
            }

            PrepareEffect(effect);
            availableEatEffects.Enqueue(effect);
        }
    }

    public void SpawnFood()
    {
        if (foodPrefabs == null || foodPrefabs.Length == 0)
        {
            Debug.LogWarning("Food prefabs are missing on FoodManager.");
            return;
        }

        if (activeFood != null)
        {
            Destroy(activeFood.gameObject);
        }

        Vector2 spawnPosition = FindFreeSpawnPosition();
        SnakeFood selectedPrefab = foodPrefabs[Random.Range(0, foodPrefabs.Length)];
        activeFood = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
    }

    public void PlayFoodEatEffect(Vector2 position)
    {
        if (availableEatEffects.Count == 0)
        {
            return;
        }

        FoodParticleController effect = availableEatEffects.Dequeue();
        effect.PlayAt(position);
    }

    private Vector2 FindFreeSpawnPosition()
    {
        Vector2 areaCenter = board ? board.Center : spawnAreaCenter;
        Vector2 areaSize = board ? board.Size : spawnAreaSize;
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

    private void PrepareEffect(FoodParticleController effect)
    {
        effect.Initialize(this);
        effect.Prepare();
    }

    internal void ReturnEatEffectToPool(FoodParticleController effect)
    {
        if (effect == null)
        {
            return;
        }

        PrepareEffect(effect);
        availableEatEffects.Enqueue(effect);
    }
}
