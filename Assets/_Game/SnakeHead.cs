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
    [SerializeField] private int initialGrowth = 2;

    private InputAction moveAction;
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

        snakeBody.Initialize(board);
        snakeBody.ResetBody(board.GetStartCell(), initialGrowth);

        currentDirection = Vector2Int.right;
        queuedDirection = currentDirection;
        pendingGrowth = initialGrowth;

        scoreManager.ResetScore();
        foodManager.SpawnFood(board, snakeBody.SnakeCells);
    }

    private void StepSnake()
    {
        currentDirection = queuedDirection;
        Vector2Int nextHead = board.WrapPosition(snakeBody.HeadCell + currentDirection);

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

        snakeBody.MoveTo(nextHead, shouldGrow);

        if (ateFood)
        {
            scoreManager.AddFoodPoints();
            foodManager.SpawnFood(board, snakeBody.SnakeCells);
        }
    }
}
