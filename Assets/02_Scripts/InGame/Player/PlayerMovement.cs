using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler InputHandler;
    // Test : 일시적으로 속도값을 지정
    [SerializeField] private float MoveSpeed = 5f;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = this.GetComponentInChildren<Rigidbody>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 input = InputHandler.MoveInput;

        Vector3 moveDir = new Vector3(input.x, 0f, input.y);

        _rigidbody.linearVelocity = moveDir * MoveSpeed;
    }
}
