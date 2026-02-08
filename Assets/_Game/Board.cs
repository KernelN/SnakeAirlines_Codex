using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private Vector2 center = Vector2.zero;
    [SerializeField] private Vector2 size = new(24f, 16f);

    public Vector2 Center => center;
    public Vector2 Size => size;

    public Vector2 Min => center - size * 0.5f;
    public Vector2 Max => center + size * 0.5f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);
    }
}
