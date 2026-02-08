using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class SnakeHead : MonoBehaviour
{
    [SerializeField] FoodManager foodManager;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] SnakeBody snakeBody;
    [SerializeField] InputActionReference dragPositionActionReference;
    InputAction dragPositionAction;
    [SerializeField] InputActionReference dragPressActionReference;
    InputAction dragPressAction;
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] Rigidbody2D headRigidbody;
    [SerializeField] private float rotationSpeed = 720f;

    Vector2 headPosition;
    Vector2 currentDirection = Vector2.right;
    Vector2 lastDragDirection = Vector2.right;
    bool isDragging;
    int pendingGrowth;
    Camera mainCamera;

    private void Awake()
    {
        dragPositionAction = dragPositionActionReference ? dragPositionActionReference.action : null;
        dragPressAction = dragPressActionReference ? dragPressActionReference.action : null;
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
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
    
    private void OnDragStarted(InputAction.CallbackContext context)
    {
        isDragging = true;
    }

    private void OnDragCanceled(InputAction.CallbackContext context)
    {
        isDragging = false;
    }

    private void UpdateDirectionFromInput()
    {
        if (isDragging && dragPositionAction != null && mainCamera)
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
            return;

        currentDirection = input.normalized;
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
        headPosition += (Vector2)transform.up * distance;

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

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        if (rotationSpeed <= 0f)
        {
            transform.rotation = targetRotation;
            return;
        }

        float maxStep = rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxStep);
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
        if (!collidedBody)
        {
            collidedBody = collision.collider.GetComponentInParent<SnakeBody>();
        }

        if (!collidedBody || collidedBody != snakeBody)
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
            int removed = snakeBody.TrimFromIndex(hitIndex, headPosition);
            scoreManager.RemoveBodyPoints(removed);
        }
    }
}
