using UnityEngine;

public class SnakeBodySegment : MonoBehaviour
{
    public int Index { get; private set; }
    public SnakeBody Owner { get; private set; }

    public void Initialize(SnakeBody owner)
    {
        Owner = owner;
    }

    public void SetIndex(int index)
    {
        Index = index;
    }
}
