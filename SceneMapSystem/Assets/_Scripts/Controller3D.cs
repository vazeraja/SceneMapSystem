using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Title("Debug")]
    [SerializeField] private bool enableDebug;

    [Title("Collision Settings")]
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckVerticalOffset;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private float gravity = 9f;
    [SerializeField] private float groundDistanceCheckOffset;
    [SerializeField] private float groundDistanceCheck = 10f;
    [SerializeField] private float groundOffset;
    [SerializeField] private float collisionDamping;
    private const int c_MaxCollisions = 10;
    private Collider[] _groundCheckColliders;
    private Vector3 _groundCheckPosition;
    private bool _wasGroundedLastFrame;

    private RaycastHit _groundHit;
    private bool _isDetectingGround;
    private Vector3 _groundPosition;
    private float _groundScale;
    private float _groundAngle;
    private float _slopeYPos;

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
        if (!enableDebug) return;

        _groundCheckPosition = groundCheckTransform.position + Vector3.up * groundCheckVerticalOffset;

        Gizmos.color = _wasGroundedLastFrame ? Color.green : Color.red;
        Gizmos.DrawWireSphere(_groundCheckPosition, groundCheckRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_groundCheckPosition + Vector3.up * groundDistanceCheckOffset, Vector3.down * groundDistanceCheck);

        Gizmos.color = Color.green;
        Ray ray = new Ray(transform.position, transform.forward);
        Gizmos.DrawRay(ray);
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

        GatherCollisionInfo();
        HandleCameraMovement();
    }


    private void FixedUpdateManual()
    {
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

        float yPos = _groundAngle != 0 ? _slopeYPos : _groundPosition.y;
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

        _velocity += moveAmount;

        // Decelerate 
        _velocity.x *= smooth;
        _velocity.z *= smooth;
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

        // Check where the ground is below the player 
        Ray ray = new Ray(_groundCheckPosition + Vector3.up * groundDistanceCheckOffset, Vector3.down);
        if (Physics.Raycast(ray, out _groundHit, groundDistanceCheck, groundLayer))
        {
            _isDetectingGround = true;

            _groundPosition = _groundHit.transform.position;
            _groundScale = _groundHit.transform.localScale.y;

            _groundAngle = Vector3.Angle(_groundHit.normal, Vector3.up);

            // Find what y height the player should land on
            // If it is slope surface then calculate it
            if (_groundAngle != 0)
            {
                GatherSlopeInfo(_groundHit);
            }
        }
        else
        {
            _isDetectingGround = false;
        }

        // Check whether player is currently grounded
        _wasGroundedLastFrame = Physics.OverlapSphereNonAlloc(_groundCheckPosition, groundCheckRadius, _groundCheckColliders, groundLayer) > 0;
    }

    private void GatherSlopeInfo(RaycastHit groundHit)
    {
        float angle = _groundAngle * Mathf.Deg2Rad;

        // Attempt 1:
        // float bz = _position.z - groundHit.transform.position.z;
        // float bx = _position.x - groundHit.transform.position.x;
        // var groundPositionY = _groundPosition.y - bz * Mathf.Tan(angle);

        // Attempt 2:
        Vector3 direction = transform.forward; // Direction along which we want to measure the distance
        Vector3 relativePosition = groundHit.transform.position - transform.position; // Position of object2 relative to object1
        
        Debug.DrawRay(groundHit.transform.position, groundHit.transform.forward, Color.cyan);

        Debug.DrawRay(groundHit.transform.position, -relativePosition.normalized * 5, Color.magenta);

        Vector3 projection = Vector3.Project(-relativePosition, direction); // Projecting relative position onto the direction

        Debug.DrawRay(groundHit.transform.position, projection, Color.yellow);

        float distance = projection.magnitude; // This gives the distance along the direction

        // If you want to consider direction (i.e., whether object2 is in front or behind object1)
        float signedDistance = (Vector3.Dot(projection, direction) > 0) ? distance : -distance;
        var groundPositionY = _groundPosition.y - signedDistance * Mathf.Tan(angle);

        _slopeYPos = groundPositionY;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(_moveDirection, _groundHit.normal).normalized;
    }
}