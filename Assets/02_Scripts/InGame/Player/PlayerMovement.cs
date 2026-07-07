using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler InputHandler;
    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float JumpPower = 5f;

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

        Vector3 moveDir = transform.forward * input.y + transform.right * input.x;
        moveDir.Normalize();

        _rigidbody.linearVelocity = new Vector3(
            moveDir.x * MoveSpeed,
            _rigidbody.linearVelocity.y,
            moveDir.z * MoveSpeed
        );
    }

    private void Jump()
    {
        if(_isGrounded)
        {
            _rigidbody.AddForce(Vector3.up * JumpPower, ForceMode.Impulse);
        }
    }
}