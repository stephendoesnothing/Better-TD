using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float edgePanSize = 20f;
    [SerializeField] private bool enableEdgePanning = true;
    [SerializeField] private Vector2 moveBounds = new Vector2(50f, 50f);

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 10f;
    [SerializeField] private float maxZoom = 50f;

    [Header("Camera Angle")]
    [SerializeField] private float cameraAngle = 45f;
    [SerializeField] private float cameraHeight = 20f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.E;
    [SerializeField] private bool enableMiddleMouseRotation = true;
    [SerializeField] private float mouseRotationSpeed = 3f;
    [SerializeField] private bool snapRotation = false;
    [SerializeField] private float snapAngle = 90f; // Snap to 90° increments if enabled

    private Vector3 targetPosition;
    private float targetDistance;
    private float targetRotation; // Y-axis rotation
    private Camera cam;
    private bool isRotatingWithMouse = false;
    private float lastMouseX;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        if (cam == null)
        {
            Debug.LogError("No camera found!");
            return;
        }

        // Initialize
        targetPosition = transform.position;
        targetDistance = cameraHeight;
        targetRotation = transform.eulerAngles.y;

        UpdateCameraPosition();
    }

    void Update()
    {
        HandleKeyboardMovement();
        HandleEdgePanning();
        HandleZoom();
        HandleRotation();
    }

    void LateUpdate()
    {
        // Clamp position to bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, -moveBounds.x, moveBounds.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -moveBounds.y, moveBounds.y);
        targetPosition.y = 0f;

        // Smoothly move and rotate to target
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);

        // Smooth rotation
        float currentY = transform.eulerAngles.y;
        float newY = Mathf.LerpAngle(currentY, targetRotation, Time.deltaTime * 8f);
        transform.rotation = Quaternion.Euler(0f, newY, 0f);

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (cam == null) return;

        // Position camera at fixed angle and distance (relative to parent's rotation)
        Vector3 direction = Quaternion.Euler(cameraAngle, 0f, 0f) * Vector3.back;
        cam.transform.localPosition = direction * targetDistance;
        cam.transform.localRotation = Quaternion.Euler(cameraAngle, 0f, 0f);
    }

    void HandleKeyboardMovement()
    {
        Vector2 moveInput = Vector2.zero;

        if (Input.GetKey(KeyCode.W)) moveInput.y += 1f;
        if (Input.GetKey(KeyCode.S)) moveInput.y -= 1f;
        if (Input.GetKey(KeyCode.A)) moveInput.x -= 1f;
        if (Input.GetKey(KeyCode.D)) moveInput.x += 1f;

        if (moveInput.magnitude > 0.1f)
        {
            MoveCamera(moveInput.normalized);
        }
    }

    void HandleEdgePanning()
    {
        if (!enableEdgePanning) return;

        Vector3 mousePos = Input.mousePosition;
        Vector2 moveDirection = Vector2.zero;

        if (mousePos.x < edgePanSize)
            moveDirection.x = -1f;
        else if (mousePos.x > Screen.width - edgePanSize)
            moveDirection.x = 1f;

        if (mousePos.y < edgePanSize)
            moveDirection.y = -1f;
        else if (mousePos.y > Screen.height - edgePanSize)
            moveDirection.y = 1f;

        if (moveDirection.magnitude > 0.1f)
        {
            MoveCamera(moveDirection.normalized);
        }
    }

    void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            targetDistance -= scrollInput * zoomSpeed * 10f;
            targetDistance = Mathf.Clamp(targetDistance, minZoom, maxZoom);
        }
    }

    void HandleRotation()
    {
        // Keyboard rotation
        if (Input.GetKey(rotateLeftKey))
        {
            targetRotation -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(rotateRightKey))
        {
            targetRotation += rotationSpeed * Time.deltaTime;
        }

        // Middle mouse button rotation
        if (enableMiddleMouseRotation)
        {
            if (Input.GetMouseButtonDown(2)) // Middle mouse button pressed
            {
                isRotatingWithMouse = true;
                lastMouseX = Input.mousePosition.x;
            }
            else if (Input.GetMouseButtonUp(2)) // Middle mouse button released
            {
                isRotatingWithMouse = false;

                // Snap to nearest angle if enabled
                if (snapRotation)
                {
                    SnapRotation();
                }
            }

            if (isRotatingWithMouse)
            {
                float deltaX = Input.mousePosition.x - lastMouseX;
                targetRotation += deltaX * mouseRotationSpeed;
                lastMouseX = Input.mousePosition.x;
            }
        }

        // Normalize rotation to 0-360
        targetRotation = Mathf.Repeat(targetRotation, 360f);
    }

    void SnapRotation()
    {
        // Snap to nearest multiple of snapAngle
        targetRotation = Mathf.Round(targetRotation / snapAngle) * snapAngle;
    }

    void MoveCamera(Vector2 direction)
    {
        // Move camera relative to current rotation
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (right * direction.x + forward * direction.y);
        targetPosition += moveDirection * moveSpeed * Time.deltaTime;
    }

    // Public methods
    public void FocusOnPosition(Vector3 position)
    {
        targetPosition = position;
        targetPosition.y = 0f;
    }

    public void ResetCamera()
    {
        targetPosition = Vector3.zero;
        targetDistance = cameraHeight;
        targetRotation = 0f;
    }

    public void RotateToAngle(float angle)
    {
        targetRotation = angle;
    }

    public void RotateBy(float degrees)
    {
        targetRotation += degrees;
    }
}