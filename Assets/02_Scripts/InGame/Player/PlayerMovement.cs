using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler InputHandler;

    [Header("Movement")]
    [SerializeField] private float WalkSpeed = 2f;
    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float SprintSpeed = 8f;
    [SerializeField] private float JumpPower = 5f;

    [Header("Ground Check")]
    [SerializeField] private Transform GroundCheck;
    [SerializeField] private float GroundRadius = 0.2f;
    [SerializeField] private LayerMask GroundMask;

    private bool _isGrounded;
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        InputHandler.JumpPerformed += Jump;
    }

    private void OnDisable()
    {
        InputHandler.JumpPerformed -= Jump;
    }

    private void FixedUpdate()
    {
        Move();
        CheckGround();
    }

    private void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(
            GroundCheck.position,
            GroundRadius,
            GroundMask
        );
    }

    private void Move()
    {
        Vector2 input = InputHandler.MoveInput;

        Vector3 moveDir =
            transform.forward * input.y +
            transform.right * input.x;

        moveDir.Normalize();

        float currentSpeed = GetCurrentMoveSpeed(input);

        _rigidbody.linearVelocity 
            = new Vector3(moveDir.x * currentSpeed, _rigidbody.linearVelocity.y, moveDir.z * currentSpeed);
    }

    private float GetCurrentMoveSpeed(Vector2 input)
    {
        bool canSprint = InputHandler.IsSprintPressed && input.y > 0f;

        if (canSprint)
            return SprintSpeed;

        if (InputHandler.IsWalkPressed)
            return WalkSpeed;

        return MoveSpeed;
    }

    private void Jump()
    {
        if (!_isGrounded)
            return;

        _rigidbody.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
    }
}