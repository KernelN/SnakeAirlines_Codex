using UnityEngine;

public class SnakeFood : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    public void SetGridPosition(Vector2Int position)
    {
        GridPosition = position;
    }
}
