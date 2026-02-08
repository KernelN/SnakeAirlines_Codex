using UnityEngine;

public class SnakeFood : MonoBehaviour
{
    public Vector2 WorldPosition => transform.position;

    private void Awake()
    {
        if (TryGetComponent(out Collider2D collider))
        {
            collider.enabled = false;
        }
    }
}
