using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using TNS.InputMiddleware;
using UnityEngine;

public class Controller3D : MonoBehaviour
{
    [Title("Components")]
    [SerializeField] private InputProvider inputProvider;
    [SerializeField] private new PlayerCamera camera;
    [SerializeField] private new Rigidbody rigidbody;
    [SerializeField] private new CapsuleCollider collider;

    private Vector3 _velocity;
    private Vector3 _position;

    public static bool InputFocus
    {
        get => !Cursor.visible;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    private InputState InputState => inputState = inputProvider;


    [Title("Ground Collision Settings")]
    [SerializeField] private float collisionDamping;
    [SerializeField] private float groundOffset;
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float slopeLimit = 45f;
    private RaycastHit _groundHit;
    [ShowInInspector, ReadOnly]
    private float _groundDistance;
    [ShowInInspector, ReadOnly]
    private bool _isGrounded;

    [Title("Movement Settings")]
    [SerializeField, PropertyRange(0.2f, 1f)]
    private float smooth = 0.95f;
    [SerializeField, PropertyRange(1, 200)]
    private float moveSpeed = 1f;
    private Vector3 _moveDirection;
    private Vector3 _moveSpeed;

    [Title("Debug")]
    [SerializeField] private bool focusOnPlay = true;
    [SerializeField] private bool enableDebug;
    [SerializeField, ReadOnly] // Show input state values in inspector
    private InputState inputState;
    [SerializeField, ReadOnly]
    private float floorPosY;

    private void Awake()
    {
        if (focusOnPlay) InputManager.InputFocus = true;

        camera.SetMainTarget(transform);
        camera.Init();

        inputProvider.OnLeftClickPressed += () => InputManager.InputFocus = true;

        _position = transform.position;
    }

    private void OnDrawGizmos()
    {
        if (!enableDebug) return;
    }

    private void Update()
    {
        // ----- Rotate the camera -----
        camera.RotateCamera(InputState.mouseDeltaX, InputState.mouseDeltaY);

        // ----- Apply velocity changes to position before handling collisions (VERY IMPORTANT) -----
        _velocity += Vector3.down * gravity * Time.fixedDeltaTime; // Gravity

        // ---- Update Position ----
        _position += _velocity * Time.deltaTime;

        // ----- Handle collisions -----
        // HandleGroundCollision(); // Collide with ground
        // HandleSlopeCollision();

        transform.position = _position; // move
    }

    // private void HandleGroundCollision()
    // {
    //     // Ground the player if they are below the ground
    //     floorPosY = Math.Abs(groundCastDown.Hit.point.y) - groundOffset;
    //     // ReSharper disable once InvertIf
    //     if (Math.Abs(_position.y) > floorPosY && groundCastDown.IsDetectingHit)
    //     {
    //         _position.y = floorPosY * Math.Sign(_position.y);
    //         _velocity.y *= -1 * collisionDamping;
    //     }
    // }

}