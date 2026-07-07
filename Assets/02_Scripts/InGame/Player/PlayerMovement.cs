using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler InputHandler;
    // Test : 일시적으로 속도값을 지정
    [SerializeField] private float MoveSpeed = 5f;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = this.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Move();
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
}
