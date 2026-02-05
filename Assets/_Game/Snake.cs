using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(LineRenderer))]
public class Snake : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private FoodManager foodManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private float moveInterval = 0.12f;
    [SerializeField] private int initialGrowth = 2;

    private readonly List<Vector2Int> snakeCells = new();

    private LineRenderer trailRenderer;
    private InputAction moveAction;

    private Vector2Int currentDirection = Vector2Int.right;
    private Vector2Int queuedDirection = Vector2Int.right;

    private float moveTimer;
    private int pendingGrowth;

    private void Awake()
    {
        trailRenderer = GetComponent<LineRenderer>();
        moveAction = moveActionReference != null ? moveActionReference.action : null;
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.Disable();
        }
    }

    private void Start()
    {
        ResetSnake();
    }

    private void Update()
    {
        moveTimer += Time.deltaTime;
        if (moveTimer < moveInterval)
        {
            return;
        }

        moveTimer = 0f;
        StepSnake();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        QueueDirection(input);
    }

    private void QueueDirection(Vector2 input)
    {
        Vector2Int desiredDirection = input switch
        {
            { x: > 0.5f } => Vector2Int.right,
            { x: < -0.5f } => Vector2Int.left,
            { y: > 0.5f } => Vector2Int.up,
            { y: < -0.5f } => Vector2Int.down,
            _ => queuedDirection
        };

        if (desiredDirection == -currentDirection)
        {
            return;
        }

        queuedDirection = desiredDirection;
    }

    private void ResetSnake()
    {
        if (board == null || foodManager == null || scoreManager == null || moveAction == null)
        {
            Debug.LogError("Snake requires Board, FoodManager, ScoreManager, and Move InputAction references.");
            enabled = false;
            return;
        }

        snakeCells.Clear();
        snakeCells.Add(board.GetStartCell());

        currentDirection = Vector2Int.right;
        queuedDirection = currentDirection;

        pendingGrowth = initialGrowth;
        scoreManager.ResetScore();

        foodManager.SpawnFood(board, snakeCells);
        RefreshVisuals();
    }

    private void StepSnake()
    {
        currentDirection = queuedDirection;
        Vector2Int nextHead = board.WrapPosition(snakeCells[0] + currentDirection);

        int collisionIndex = FindBodyCollisionIndex(nextHead);
        if (collisionIndex >= 0)
        {
            int removed = RemoveBodyFrom(collisionIndex);
            scoreManager.RemoveBodyPoints(removed);
        }

        snakeCells.Insert(0, nextHead);

        if (pendingGrowth > 0)
        {
            pendingGrowth--;
        }
        else
        {
            snakeCells.RemoveAt(snakeCells.Count - 1);
        }

        if (foodManager.IsFoodAt(nextHead))
        {
            pendingGrowth++;
            scoreManager.AddFoodPoints();
            foodManager.SpawnFood(board, snakeCells);
        }

        RefreshVisuals();
    }

    private int FindBodyCollisionIndex(Vector2Int headPosition)
    {
        for (int i = 1; i < snakeCells.Count; i++)
        {
            if (snakeCells[i] == headPosition)
            {
                return i;
            }
        }

        return -1;
    }

    private int RemoveBodyFrom(int collisionIndex)
    {
        int removedSegments = snakeCells.Count - collisionIndex;

        for (int i = snakeCells.Count - 1; i >= collisionIndex; i--)
        {
            snakeCells.RemoveAt(i);
        }

        return removedSegments;
    }

    private void RefreshVisuals()
    {
        transform.position = board.GridToWorld(snakeCells[0]);

        trailRenderer.positionCount = snakeCells.Count;
        for (int i = 0; i < snakeCells.Count; i++)
        {
            trailRenderer.SetPosition(i, board.GridToWorld(snakeCells[i]));
        }
    }
}
