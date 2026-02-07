using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class SnakeHead : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private FoodManager foodManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private SnakeBody snakeBody;
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private float moveInterval = 0.12f;

    private InputAction moveAction;
    private Vector2Int headCell;
    private Vector2Int currentDirection = Vector2Int.right;
    private Vector2Int queuedDirection = Vector2Int.right;
    private float moveTimer;
    private int pendingGrowth;

    private void Awake()
    {
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
        if (board == null || foodManager == null || scoreManager == null || snakeBody == null || moveAction == null)
        {
            Debug.LogError("SnakeHead requires Board, FoodManager, ScoreManager, SnakeBody, and Move InputAction references.");
            enabled = false;
            return;
        }

        headCell = board.GetStartCell();
        transform.position = board.GridToWorld(headCell);

        snakeBody.Initialize(board);
        snakeBody.ResetBody(headCell);

        currentDirection = Vector2Int.right;
        queuedDirection = currentDirection;
        pendingGrowth = snakeBody.BodyCells.Count;

        scoreManager.ResetScore();
        foodManager.SpawnFood(board, BuildOccupiedCells());
        snakeBody.RefreshVisuals(headCell);
        UpdateHeadRotation(currentDirection);
    }

    private void StepSnake()
    {
        currentDirection = queuedDirection;
        UpdateHeadRotation(currentDirection);
        Vector2Int nextHead = board.WrapPosition(headCell + currentDirection);

        int collisionIndex = snakeBody.FindBodyCollisionIndex(nextHead);
        if (collisionIndex >= 0)
        {
            int removed = snakeBody.TrimFromIndex(collisionIndex);
            scoreManager.RemoveBodyPoints(removed);
        }

        bool ateFood = foodManager.IsFoodAt(nextHead);
        if (ateFood)
        {
            pendingGrowth++;
        }

        bool shouldGrow = pendingGrowth > 0;
        if (shouldGrow)
        {
            pendingGrowth--;
        }

        Vector2Int previousHead = headCell;
        headCell = nextHead;
        transform.position = board.GridToWorld(headCell);
        snakeBody.MoveTo(headCell, previousHead, shouldGrow);

        if (ateFood)
        {
            scoreManager.AddFoodPoints();
            foodManager.SpawnFood(board, BuildOccupiedCells());
        }
    }

    private void UpdateHeadRotation(Vector2Int direction)
    {
        float angle = direction switch
        {
            { x: > 0 } => 0f,
            { x: < 0 } => 180f,
            { y: > 0 } => 90f,
            { y: < 0 } => -90f,
            _ => transform.eulerAngles.z
        };

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private List<Vector2Int> BuildOccupiedCells()
    {
        var occupied = new List<Vector2Int>(snakeBody.BodyCells.Count + 1)
        {
            headCell
        };
        occupied.AddRange(snakeBody.BodyCells);
        return occupied;
    }
}
