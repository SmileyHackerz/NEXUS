using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = 6.0f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float rotationSmoothing = 10f;

    [Header("Dependencies")]
    [SerializeField] private ThirdPersonCamera cameraSystem;

    private CharacterController _controller;
    private Vector2 _inputMove = Vector2.zero;
    private Vector3 _velocity = Vector3.zero;
    private bool _isRunning = false;
    private bool _isGrounded = false;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (cameraSystem == null)
        {
            cameraSystem = FindFirstObjectByType<ThirdPersonCamera>();
        }
    }

    public void OnMove(InputAction.CallbackContext context) => _inputMove = context.ReadValue<Vector2>();
    public void OnRun(InputAction.CallbackContext context) => _isRunning = context.ReadValueAsButton();
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && _isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * 2f * -Physics.gravity.y);
    }

    private void Update()
    {
        HandleDirectInput();

        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        Vector3 moveDirection = Vector3.zero;
        if (cameraSystem != null)
        {
            // Use the PURE rotation from the camera, not the smoothed transform.forward
            Vector3 camForward = cameraSystem.GetForwardDirection();
            Vector3 camRight = cameraSystem.GetRightDirection();
            moveDirection = (camForward * _inputMove.y + camRight * _inputMove.x).normalized;
        }
        else
        {
            moveDirection = new Vector3(_inputMove.x, 0, _inputMove.y).normalized;
        }

        float currentSpeed = _isRunning ? runSpeed : walkSpeed;
        
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            // Character rotates to face the move direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing * Time.deltaTime);
            
            _controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        _velocity.y += Physics.gravity.y * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void HandleDirectInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Direct keyboard input fallback
        float x = 0;
        float z = 0;

        if (keyboard.wKey.isPressed) z += 1;
        if (keyboard.sKey.isPressed) z -= 1;
        if (keyboard.aKey.isPressed) x -= 1;
        if (keyboard.dKey.isPressed) x += 1;

        _inputMove = new Vector2(x, z);
        _isRunning = keyboard.leftShiftKey.isPressed;
        
        if (keyboard.spaceKey.wasPressedThisFrame && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * 2f * -Physics.gravity.y);
        }
    }
}
