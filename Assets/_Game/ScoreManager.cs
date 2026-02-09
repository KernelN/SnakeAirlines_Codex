using System;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private int pointsPerFood = 10;
    [SerializeField] private int pointsPerRemovedSegment = 10;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string scoreFormat = "Points: {0}";

    public int Score { get; private set; }
    public event Action<int> ScoreChanged;

    private void Start()
    {
        UpdateScoreText();
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
        UpdateScoreText();
        ScoreChanged?.Invoke(Score);
    }

    private void UpdateScoreText()
    {
        if (scoreText == null)
        {
            return;
        }

        scoreText.text = string.Format(scoreFormat, Score);
    }
}
