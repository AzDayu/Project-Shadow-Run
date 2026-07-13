using UnityEngine;

public class PlayerSight : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler InputHandler;
    [SerializeField] private Transform LeanPivot;
    [SerializeField] private Transform SightTarget;

    [SerializeField] private float MouseSensitivity = 0.1f;
    [SerializeField] private float MinPitch = -80f;
    [SerializeField] private float MaxPitch = 80f;

    [SerializeField] private float LeanDistance = 1.5f;
    [SerializeField] private float LeanAngle = 10f;
    [SerializeField] private float LeanSpeed = 6f;

    [SerializeField] private LayerMask LeanCollisionMask;
    [SerializeField] private float LeanCollisionRadius = 0.15f;
    [SerializeField] private float LeanCollisionPadding = 0.02f;

    private float _pitch;
    private float _currentLean;
    private Vector3 _leanPivotDefaultPosition;

    private void Awake()
    {
        _leanPivotDefaultPosition = LeanPivot.localPosition;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Lean();
    }

    private void Look()
    {
        Vector2 lookInput = InputHandler.LookInput;

        float mouseX = lookInput.x * MouseSensitivity;
        float mouseY = lookInput.y * MouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
    }

    private void Lean()
    {
        float inputLean = InputHandler.LeanInput;

        float targetLean = GetCollisionLimitedLean(inputLean);

        _currentLean = Mathf.MoveTowards(
            _currentLean,
            targetLean,
            LeanSpeed * Time.deltaTime
        );

        LeanPivot.localPosition =
            _leanPivotDefaultPosition +
            Vector3.right *
            (_currentLean * LeanDistance);

        LeanPivot.localRotation = Quaternion.Euler(
            _pitch,
            0f,
            -_currentLean * LeanAngle
        );
    }

    private float GetCollisionLimitedLean(float targetLean)
    {
        if (Mathf.Approximately(targetLean, 0f))
            return 0f;

        Transform pivotParent = LeanPivot.parent;

        Quaternion centerRotation =
            Quaternion.Euler(_pitch, 0f, 0f);

        Quaternion targetRotation =
            Quaternion.Euler(
                _pitch,
                0f,
                -targetLean * LeanAngle
            );

        Vector3 centerPivotPosition =
            _leanPivotDefaultPosition;

        Vector3 targetPivotPosition =
            _leanPivotDefaultPosition +
            Vector3.right *
            (targetLean * LeanDistance);

        Vector3 sightLocalPosition =
            SightTarget.localPosition;

        Vector3 centerSightPosition =
            pivotParent.TransformPoint(
                centerPivotPosition +
                centerRotation * sightLocalPosition
            );

        Vector3 targetSightPosition =
            pivotParent.TransformPoint(
                targetPivotPosition +
                targetRotation * sightLocalPosition
            );

        Vector3 checkDirection =
            targetSightPosition - centerSightPosition;

        float checkDistance = checkDirection.magnitude;

        if (checkDistance <= Mathf.Epsilon)
            return targetLean;

        checkDirection.Normalize();

        bool hasCollision = Physics.SphereCast(
            centerSightPosition,
            LeanCollisionRadius,
            checkDirection,
            out RaycastHit hit,
            checkDistance,
            LeanCollisionMask,
            QueryTriggerInteraction.Ignore
        );

        if (!hasCollision)
            return targetLean;

        float allowedDistance = Mathf.Max(
            0f,
            hit.distance - LeanCollisionPadding
        );

        float allowedRatio = Mathf.Clamp01(
            allowedDistance / checkDistance
        );

        return targetLean * allowedRatio;
    }

    public Transform GetPlayerSightTransform()
    {
        return SightTarget;
    }
}
