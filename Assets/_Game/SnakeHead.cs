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
    [SerializeField] private InputActionReference dragPositionActionReference;
    [SerializeField] private InputActionReference dragPressActionReference;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float eatRadius = 0.4f;
    [SerializeField] private float bodyCollisionRadius = 0.35f;

    private InputAction moveAction;
    private InputAction dragPositionAction;
    private InputAction dragPressAction;
    private Vector2 headPosition;
    private Vector2 currentDirection = Vector2.right;
    private Vector2 lastDragDirection = Vector2.right;
    private bool isDragging;
    private int pendingGrowth;
    private Camera mainCamera;

    private void Awake()
    {
        moveAction = moveActionReference != null ? moveActionReference.action : null;
        dragPositionAction = dragPositionActionReference != null ? dragPositionActionReference.action : null;
        dragPressAction = dragPressActionReference != null ? dragPressActionReference.action : null;
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.Enable();
        }

        if (dragPressAction != null)
        {
            dragPressAction.started += OnDragStarted;
            dragPressAction.canceled += OnDragCanceled;
            dragPressAction.Enable();
        }

        if (dragPositionAction != null)
        {
            dragPositionAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.Disable();
        }

        if (dragPressAction != null)
        {
            dragPressAction.started -= OnDragStarted;
            dragPressAction.canceled -= OnDragCanceled;
            dragPressAction.Disable();
        }

        if (dragPositionAction != null)
        {
            dragPositionAction.Disable();
        }
    }

    private void Start()
    {
        ResetSnake();
    }

    private void Update()
    {
        UpdateDirectionFromInput();
        StepSnake();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        UpdateDirectionFromVector(input);
    }

    private void OnDragStarted(InputAction.CallbackContext context)
    {
        isDragging = true;
    }

    private void OnDragCanceled(InputAction.CallbackContext context)
    {
        isDragging = false;
        currentDirection = lastDragDirection;
    }

    private void UpdateDirectionFromInput()
    {
        if (isDragging && dragPositionAction != null && mainCamera != null)
        {
            Vector2 screenPosition = dragPositionAction.ReadValue<Vector2>();
            Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(
                screenPosition.x,
                screenPosition.y,
                -mainCamera.transform.position.z));
            Vector2 direction = (Vector2)(worldPoint - transform.position);
            UpdateDirectionFromVector(direction);
        }
    }

    private void UpdateDirectionFromVector(Vector2 input)
    {
        if (input.sqrMagnitude <= 0.01f)
        {
            return;
        }

        currentDirection = input.normalized;
        lastDragDirection = currentDirection;
    }

    private void ResetSnake()
    {
        if (board == null || foodManager == null || scoreManager == null || snakeBody == null)
        {
            Debug.LogError("SnakeHead requires Board, FoodManager, ScoreManager, and SnakeBody references.");
            enabled = false;
            return;
        }

        Vector2Int startCell = board.GetStartCell();
        headPosition = board.GridToWorld(startCell);
        transform.position = new Vector3(headPosition.x, headPosition.y, 0f);

        currentDirection = Vector2.right;
        lastDragDirection = currentDirection;
        snakeBody.ResetBody(headPosition, currentDirection);
        pendingGrowth = snakeBody.BodySegments.Count;

        scoreManager.ResetScore();
        foodManager.SpawnFood(board, BuildOccupiedPositions());
        snakeBody.RefreshVisuals(headPosition);
        UpdateHeadRotation(currentDirection);
    }

    private void StepSnake()
    {
        UpdateHeadRotation(currentDirection);
        float distance = moveSpeed * Time.deltaTime;
        headPosition += currentDirection * distance;
        headPosition = board.ClampWorldPosition(headPosition);

        int collisionIndex = snakeBody.FindBodyCollisionIndex(headPosition, bodyCollisionRadius);
        if (collisionIndex >= 0)
        {
            int removed = snakeBody.TrimFromIndex(collisionIndex);
            scoreManager.RemoveBodyPoints(removed);
        }

        bool ateFood = foodManager.IsFoodNear(headPosition, eatRadius);
        if (ateFood)
        {
            pendingGrowth++;
        }

        bool shouldGrow = pendingGrowth > 0;
        if (shouldGrow)
        {
            pendingGrowth--;
        }

        transform.position = new Vector3(headPosition.x, headPosition.y, 0f);
        snakeBody.Advance(headPosition, shouldGrow);

        if (ateFood)
        {
            scoreManager.AddFoodPoints();
            foodManager.SpawnFood(board, BuildOccupiedPositions());
        }
    }

    private void UpdateHeadRotation(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.01f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private List<Vector2> BuildOccupiedPositions()
    {
        var occupied = new List<Vector2>(snakeBody.BodySegments.Count + 1)
        {
            headPosition
        };
        occupied.AddRange(snakeBody.BodySegments);
        return occupied;
    }
}
