using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using Sirenix.OdinInspector;
using TNS.InputMiddleware;
using UnityEngine;

public class Controller3D : MonoBehaviour
{
    private InputProvider _inputProvider;

    private InputState InputState => _inputProvider;

    private static bool InputFocus
    {
        get => !Cursor.visible;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    [Title("Collision Settings")]
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckVerticalOffset;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private float gravity = 9f;
    [SerializeField] private float groundDistanceCheck = 10f;
    [SerializeField] private float groundOffset;
    [SerializeField] private float collisionDamping;
    private const int c_MaxCollisions = 1;
    private Collider[] _groundCheckColliders;
    private Vector3 _groundCheckPosition;
    private bool _wasGroundedLastFrame;
    private RaycastHit[] _groundDistanceColliders;
    private Vector3 _groundPosition;
    private float _slopeYPos;
    private float _groundScale;
    private bool _isDetectingGround;

    [Title("Camera Settings")]
    [SerializeField] private new Camera camera;
    [SerializeField] private float lookSensitivity = 1f;
    private float _yaw;
    private float _pitch;

    [Title("Movement Settings")]
    [SerializeField, PropertyRange(0.8f, 1f)]
    private float smooth = 0.90f;
    [SerializeField] private float moveSpeed = 1f;
    private Vector3 _moveDirection;
    private Vector3 _position;
    private Vector3 _velocity;

    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
            GatherCollisionInfo();

        Gizmos.color = _wasGroundedLastFrame ? Color.green : Color.red;
        Gizmos.DrawWireSphere(_groundCheckPosition, groundCheckRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_groundCheckPosition, Vector3.down * groundDistanceCheck);
    }

    private void Awake()
    {
        _inputProvider = InputMiddlewareService.InputProvider;
        InputFocus = true;

        _position = transform.position;

        StartCoroutine(FixedSteps());
    }

    private IEnumerator FixedSteps()
    {
        while (true)
        {
            FixedUpdateManual();
            yield return new WaitForSeconds(0.01f); // 100 fps
        }
    }

    private void Update()
    {
        if (Application.isPlaying == false)
            return;

        HandleCameraMovement();
    }


    private void FixedUpdateManual()
    {
        GatherCollisionInfo();

        _velocity += Vector3.down * gravity * Time.fixedDeltaTime;
        _position += _velocity * Time.deltaTime;

        HandleGroundCollision();

        HandlePlayerMovement();

        transform.position = _position; // move
    }

    private void HandleGroundCollision()
    {
        // Ground the player if they are below the ground
        float scaledOffset = groundOffset * _groundScale;

        float yPos;
        if (OnSlope())
        {
            yPos = _slopeYPos;
        }
        else
        {
            yPos = _groundPosition.y;
        }

        float floorPos = Math.Abs(yPos) - scaledOffset;

        if (Math.Abs(_position.y) > floorPos && _isDetectingGround)
        {
            _position.y = floorPos * Math.Sign(_position.y);
            _velocity.y *= -1 * collisionDamping;
        }
    }


    private void HandlePlayerMovement()
    {
        Vector3 right = camera.transform.right;
        Vector3 forward = camera.transform.forward;
        forward.y = 0;

        _moveDirection = InputState.movementDirection.y * forward + InputState.movementDirection.x * right;
        var moveAmount = _moveDirection * (Time.fixedDeltaTime * moveSpeed);

        if (OnSlope())
        {
            GatherSlopeInfo(_groundDistanceColliders[0]);
        }

        _velocity += moveAmount;

        // Decelerate 
        _velocity.x *= smooth;
        _velocity.z *= smooth;
    }

    private void GatherSlopeInfo(RaycastHit groundHit)
    {
        float angle = Vector3.Angle(groundHit.normal, Vector3.up) * Mathf.Deg2Rad;

        Vector2 player = new Vector2(_position.x, _position.z);
        Vector2 ground = new Vector2(groundHit.transform.position.x, groundHit.transform.position.z);
        float b = Vector2.Distance(player, ground);

        // float bz = _position.z - groundHit.transform.position.z;
        // float bx = _position.x - groundHit.transform.position.x;

        // float maxB = Mathf.Max(bz, bx);

        // Vector2 vectorA = new Vector2(groundHit.transform.forward.x, groundHit.transform.forward.z);
        // Vector2 vectorB = Vector2.up;
        // float platformAngle = Vector2.Angle(vectorA, vectorB) * Mathf.Deg2Rad;
        
        var bxz = b / Mathf.Cos(angle);
        var groundPositionY = _groundPosition.y - bxz * Mathf.Tan(angle);


        // float dot = Vector2.Dot(vectorA, vectorB);
        // float magnitudes = vectorA.magnitude * vectorB.magnitude;
        //
        // float cosTheta = dot / magnitudes;
        // float angleRad = Mathf.Acos(cosTheta);


        _slopeYPos = groundPositionY;
    }

    private void HandleCameraMovement()
    {
        if (InputFocus)
        {
            _yaw += Input.GetAxis("Mouse X") * lookSensitivity; // Right/Left camera movement
            _pitch -= Input.GetAxis("Mouse Y") * lookSensitivity; // Up/Down camera movement
            _pitch = Mathf.Clamp(_pitch, -90, 90);

            transform.localRotation = Quaternion.Euler(0, _yaw, 0f); // Rotate head left/right to rotate cam
            camera.transform.localRotation = Quaternion.Euler(_pitch, 0, 0); // Rotate just cam up/down

            // leave focus mode stuff
            if (Input.GetKeyDown(KeyCode.Escape))
                InputFocus = false;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            InputFocus = true;
        }
    }

    private void GatherCollisionInfo()
    {
        _groundCheckPosition = groundCheckTransform.position + Vector3.up * groundCheckVerticalOffset;
        _groundCheckColliders = new Collider[c_MaxCollisions];
        _groundDistanceColliders = new RaycastHit[c_MaxCollisions];

        // Check distance to ground
        Ray ray = new Ray(_groundCheckPosition, Vector3.down);
        if (Physics.RaycastNonAlloc(ray, _groundDistanceColliders, groundDistanceCheck, groundLayer) > 0)
        {
            _groundPosition = _groundDistanceColliders[0].transform.position;

            _groundScale = _groundDistanceColliders[0].transform.localScale.y;
            _isDetectingGround = true;
        }
        else
        {
            _isDetectingGround = false;
        }

        // Check whether player is currently grounded
        _wasGroundedLastFrame = Physics.OverlapSphereNonAlloc(_groundCheckPosition, groundCheckRadius, _groundCheckColliders, groundLayer) > 0;
    }


    private bool OnSlope()
    {
        float angle = Vector3.Angle(_groundDistanceColliders[0].normal, Vector3.up);

        if (_isDetectingGround)
        {
            return angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(_moveDirection, _groundDistanceColliders[0].normal).normalized;
    }
}