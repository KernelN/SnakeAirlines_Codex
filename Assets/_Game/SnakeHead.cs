using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class SnakeHead : MonoBehaviour
{
    [SerializeField] private FoodManager foodManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private SnakeBody snakeBody;
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private InputActionReference dragPositionActionReference;
    [SerializeField] private InputActionReference dragPressActionReference;
    [SerializeField] private float moveSpeed = 4f;

    private InputAction moveAction;
    private InputAction dragPositionAction;
    private InputAction dragPressAction;
    private Vector2 headPosition;
    private Vector2 currentDirection = Vector2.right;
    private Vector2 lastDragDirection = Vector2.right;
    private bool isDragging;
    private int pendingGrowth;
    private Camera mainCamera;
    private Rigidbody2D headRigidbody;

    private void Awake()
    {
        moveAction = moveActionReference != null ? moveActionReference.action : null;
        dragPositionAction = dragPositionActionReference != null ? dragPositionActionReference.action : null;
        dragPressAction = dragPressActionReference != null ? dragPressActionReference.action : null;
        mainCamera = Camera.main;
        headRigidbody = GetComponent<Rigidbody2D>();
        headRigidbody.gravityScale = 0f;
        headRigidbody.isKinematic = true;
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
        if (foodManager == null || scoreManager == null || snakeBody == null)
        {
            Debug.LogError("SnakeHead requires FoodManager, ScoreManager, and SnakeBody references.");
            enabled = false;
            return;
        }

        headPosition = new Vector2(transform.position.x, transform.position.y);

        currentDirection = Vector2.right;
        lastDragDirection = currentDirection;
        snakeBody.ResetBody(headPosition, currentDirection);
        pendingGrowth = snakeBody.BodySegments.Count;

        scoreManager.ResetScore();
        foodManager.SpawnFood();
        snakeBody.RefreshVisuals(headPosition);
        UpdateHeadRotation(currentDirection);
    }

    private void StepSnake()
    {
        UpdateHeadRotation(currentDirection);
        float distance = moveSpeed * Time.deltaTime;
        headPosition += currentDirection * distance;

        bool shouldGrow = pendingGrowth > 0;
        if (shouldGrow)
        {
            pendingGrowth--;
        }

        headRigidbody.MovePosition(new Vector2(headPosition.x, headPosition.y));
        snakeBody.Advance(headPosition, shouldGrow);

        // Food collection is handled via OnCollisionEnter2D.
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out SnakeFood _))
        {
            pendingGrowth++;
            scoreManager.AddFoodPoints();
            foodManager.SpawnFood();
            return;
        }

        SnakeBody collidedBody = collision.collider.GetComponent<SnakeBody>();
        if (collidedBody == null)
        {
            collidedBody = collision.collider.GetComponentInParent<SnakeBody>();
        }

        if (collidedBody == null || collidedBody != snakeBody)
        {
            return;
        }

        if (collision.contactCount == 0)
        {
            return;
        }

        Vector2 contactPoint = collision.GetContact(0).point;
        int hitIndex = snakeBody.FindClosestSegmentIndex(contactPoint);
        if (hitIndex >= 0)
        {
            int removed = snakeBody.TrimFromIndex(hitIndex);
            scoreManager.RemoveBodyPoints(removed);
        }
    }
}
