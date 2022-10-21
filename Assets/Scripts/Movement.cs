using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour {
    public bool CanMove { get; private set; } = true;
    public bool IsSprinting => canSprint && Input.GetKey(sprintKey) && _currentInput.x > 0.1f;
    public bool ShouldJump => Input.GetKeyDown(jumpKey) && _characterController.isGrounded;

    [Header("Movement Parameters")] [SerializeField] [Min(0)]
    private float walkMaxSpeed = 4.0f;

    [SerializeField] private float gravity = 0.24f;

    [Header("Sprint Parameters")] [SerializeField]
    private bool canSprint = true;

    [SerializeField] [Min(0)] private float sprintMaxSpeed = 6.0f;
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Jump Parameters")] [SerializeField]
    private bool canJump = true;

    [SerializeField] [Min(0)] private float jumpForce = 8.0f;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Crouch Parameters")] [SerializeField]
    private bool canCrouch = true;

    [SerializeField] [Min(0)] private float crouchMaxSpeed = 1.0f;
    [SerializeField] private float crouchHeight = 0.9f;
    [SerializeField] private float standHeight = 1.9f;
    [SerializeField] private float timeToCrouch = 0.2f;
    [SerializeField] private Vector3 standCenter = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    private bool _isCrouching = false;
    private Coroutine _crouchCoroutine;

    [Header("HeadBob Parameters")] [SerializeField]
    private bool canHeadbob = true;

    [SerializeField] private float headbobTriggerSpeed = 1f;
    [SerializeField] private float walkHeadbobSpeed = 9f;
    [SerializeField] private float walkHeadbobAmount = 0.015f;
    [SerializeField] private float sprintHeadbobSpeed = 11f;
    [SerializeField] private float sprintHeadbobAmount = 0.025f;
    [SerializeField] private float crouchHeadbobSpeed = 4f;
    [SerializeField] private float crouchHeadbobAmount = 0.01f;
    private Vector3 _defaultCameraLocalPos;
    private float _headbobTimer = 0f;

    [Header("Slope Parameters")] [SerializeField]
    private bool canSlideOnSlope = true;

    [SerializeField] private float slopeSlideSpeed = 8f;
    private Vector3 _hitPointNormal;

    private bool IsSliding {
        get {
            if (_characterController.isGrounded &&
                Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHitInfo, 2f)) {
                _hitPointNormal = slopeHitInfo.normal;

                return Vector3.Angle(Vector3.up, _hitPointNormal) > _characterController.slopeLimit;
            } else {
                return false;
            }
        }
    }

    [Header("Look Parameters")] [SerializeField, Range(1, 10)]
    private float lookSpeedX = 2.0f;

    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 90)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 90)] private float lowerLookLimit = 80.0f;

    private CharacterController _characterController;
    private Camera _playerCamera;

    private Vector3 _moveDirection;
    private Vector2 _currentInput;

    private float _currentMaxSpeed;
    private float _cameraRotationX = 0;

    private void Awake() {
        _playerCamera = GetComponentInChildren<Camera>();
        _characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _defaultCameraLocalPos = _playerCamera.transform.localPosition;
    }

    private void Update() {
        if (CanMove) {
            ControlMaxSpeed();
            LimitDiagonalSpeed();

            if (canJump) {
                HandleJump();
            }

            if (canCrouch) {
                HandleCrouch();
            }

            HandleMouseLook();
        }

        HandleMovementDirectionInWorldSpace();
        ApplyFinalMovement();
    }

    private void LateUpdate() {
        if (CanMove) {
            if (canHeadbob) {
                HandleHeadbob();
                ResetHeadbob();
            }
        }
    }

    private void ControlMaxSpeed() {
        if (IsSprinting) {
            _currentMaxSpeed = sprintMaxSpeed;
        } else {
            _currentMaxSpeed = walkMaxSpeed;
        }

        if (_isCrouching) {
            _currentMaxSpeed = crouchMaxSpeed;
        }
    }

    private void LimitDiagonalSpeed() {
        _currentInput = new Vector2(_currentMaxSpeed * Input.GetAxis("Vertical"),
            _currentMaxSpeed * Input.GetAxis("Horizontal"));

        if (_currentInput.x != 0 && _currentInput.y != 0) {
            if (IsSprinting) {
                float x = ValueToReduce(sprintMaxSpeed, walkMaxSpeed);
                _currentInput.x = Mathf.Clamp(_currentInput.x, -(sprintMaxSpeed - x), sprintMaxSpeed - x);
                _currentInput.y = Mathf.Clamp(_currentInput.y, -(walkMaxSpeed - x), walkMaxSpeed - x);
            } else {
                float speed = Mathf.Sqrt((_currentMaxSpeed * _currentMaxSpeed) / 2);
                _currentInput.x = Mathf.Clamp(_currentInput.x, -speed, speed);
                _currentInput.y = Mathf.Clamp(_currentInput.y, -speed, speed);
            }
        }
    }

    // a, b > 0
    private float ValueToReduce(float a, float b) {
        if (b > a) {
            float temp = a;
            a = b;
            b = temp;
        }

        float delta = Mathf.Pow(-2 * a - 2 * b, 2) - 4 * 2 * b * b;
        float x1 = (-(-2 * a - 2 * b) + Mathf.Sqrt(delta)) / (2 * 2);
        float x2 = (-(-2 * a - 2 * b) - Mathf.Sqrt(delta)) / (2 * 2);

        return x1 < x2 ? x1 : x2;
    }

    private void HandleMovementDirectionInWorldSpace() {
        float moveDirectionY = _moveDirection.y;
        _moveDirection = transform.TransformDirection(Vector3.forward) * _currentInput.x +
                         transform.TransformDirection(Vector3.right) * _currentInput.y;
        _moveDirection.y = moveDirectionY;
    }

    public void ToggleFreeze() {
        CanMove = !CanMove;
        if (!CanMove) _currentInput = Vector2.zero;

        Cursor.visible = !Cursor.visible;
        Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }

    private void HandleJump() {
        if (ShouldJump) {
            _moveDirection.y = jumpForce;
        }
    }

    private void HandleCrouch() {
        if (Input.GetKeyUp(crouchKey) || (!Input.GetKey(crouchKey) && _isCrouching)) {
            if (!Physics.Raycast(_playerCamera.transform.position, Vector3.up, 1f)) {
                if (_crouchCoroutine != null) {
                    StopCoroutine(_crouchCoroutine);
                }

                _crouchCoroutine = StartCoroutine(CrouchOrStand());
            }
        }

        if (Input.GetKeyDown(crouchKey) && !_isCrouching) {
            if (_crouchCoroutine != null) {
                StopCoroutine(_crouchCoroutine);
            }

            _crouchCoroutine = StartCoroutine(CrouchOrStand());
        }
    }

    private IEnumerator CrouchOrStand() {
        _isCrouching = !_isCrouching;

        float timeElapsed = 0f;

        float targetHeight = _isCrouching ? crouchHeight : standHeight;
        float currentHeight = _characterController.height;

        Vector3 targetCenter = _isCrouching ? crouchCenter : standCenter;
        Vector3 currentCenter = _characterController.center;

        while (timeElapsed < timeToCrouch) {
            _characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            _characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        _characterController.height = targetHeight;
        _characterController.center = targetCenter;
    }

    private void HandleHeadbob() {
        if (!_characterController.isGrounded)
            return;

        if (CurrentPlayerSpeed() >= headbobTriggerSpeed) {
            _playerCamera.transform.localPosition = new Vector3(
                _defaultCameraLocalPos.x + HeadBobMotion().x,
                _defaultCameraLocalPos.y + HeadBobMotion().y,
                _playerCamera.transform.localPosition.z);
        }
    }

    private Vector3 HeadBobMotion() {
        Vector3 pos = Vector3.zero;
        //làm headbob mượt hơn
        _headbobTimer += Time.deltaTime;

        pos.y = Mathf.Sin(_headbobTimer *
                          (_isCrouching ? crouchHeadbobSpeed : IsSprinting ? sprintHeadbobSpeed : walkHeadbobSpeed))
                * (_isCrouching ? crouchHeadbobAmount : IsSprinting ? sprintHeadbobAmount : walkHeadbobAmount);
        pos.x = Mathf.Sin(_headbobTimer *
                    (_isCrouching ? crouchHeadbobSpeed : IsSprinting ? sprintHeadbobSpeed : walkHeadbobSpeed) / 2)
                * (_isCrouching ? crouchHeadbobAmount : IsSprinting ? sprintHeadbobAmount : walkHeadbobAmount)
                * 2;
        return pos;
    }

    private void ResetHeadbob() {
        if (_playerCamera.transform.localPosition == _defaultCameraLocalPos)
            return;

        if (CurrentPlayerSpeed() < headbobTriggerSpeed) {
            _playerCamera.transform.localPosition = Vector3.Lerp(_playerCamera.transform.localPosition,
                _defaultCameraLocalPos, 2 * Time.deltaTime);
            _headbobTimer = 0;
        }
    }

    private float CurrentPlayerSpeed() {
        return Mathf.Sqrt(_moveDirection.x * _moveDirection.x + _moveDirection.z * _moveDirection.z);
    }

    private void HandleMouseLook() {
        _cameraRotationX += Input.GetAxis("Mouse Y") * lookSpeedY;
        _cameraRotationX = Mathf.Clamp(_cameraRotationX, -lowerLookLimit, upperLookLimit);
        _playerCamera.transform.localRotation = Quaternion.Inverse(Quaternion.Euler(_cameraRotationX, 0, 0));

        transform.localRotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void ApplyFinalMovement() {
        if (!_characterController.isGrounded) {
            _moveDirection.y -= gravity * Time.deltaTime;
        }

        if (canSlideOnSlope && IsSliding) {
            _moveDirection += new Vector3(_hitPointNormal.x, -_hitPointNormal.y, _hitPointNormal.z) * slopeSlideSpeed;
        }

        _characterController.Move(_moveDirection * Time.deltaTime);
    }
}