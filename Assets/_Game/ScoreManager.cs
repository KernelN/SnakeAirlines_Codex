using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private int pointsPerFood = 10;
    [SerializeField] private int pointsPerRemovedSegment = 10;

    public int Score { get; private set; }
    public event Action<int> ScoreChanged;

    private void Start()
    {
        Notify();
    }

    public void AddFoodPoints()
    {
        Score += pointsPerFood;
        Notify();
    }

    public void RemoveBodyPoints(int removedSegments)
    {
        int deducted = removedSegments * pointsPerRemovedSegment;
        Score = Mathf.Max(0, Score - deducted);
        Notify();
    }

    public void ResetScore()
    {
        Score = 0;
        Notify();
    }

    private void Notify()
    {
        ScoreChanged?.Invoke(Score);
    }
}
