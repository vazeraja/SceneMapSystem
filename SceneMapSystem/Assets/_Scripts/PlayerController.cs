using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TNS.InputMiddleware;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputProvider _inputProvider;
    private InputState InputState => _inputProvider;

    [Title("Camera Settings")]
    [SerializeField] private Transform cam;
    [SerializeField] private float lookSensitivity;

    [Title("Player Movement")]
    [SerializeField] private float gravity;
    [SerializeField, PropertyRange(0.8f, 1f)]
    private float smoof = 0.99f;
    [SerializeField] private float moveSpeed = 1f;

    private float _yaw;
    private float _pitch;
    private Vector2 _moveInput;
    private Vector3 _moveVel;

    private static bool InputFocus
    {
        get => !Cursor.visible;
        set
        {
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }
    }

    private void Awake()
    {
        if (Application.isPlaying == false) return;

        _inputProvider = InputMiddlewareService.InputProvider;

        InputFocus = true;
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

    private void FixedUpdateManual()
    {
        if (Application.isPlaying == false)
            return;

        if (InputFocus)
        {
            Vector3 right = cam.right;
            Vector3 forward = cam.forward;
            forward.y = 0;

            _moveVel += (InputState.movementDirection.y * forward + InputState.movementDirection.x * right) * (Time.fixedDeltaTime * moveSpeed);
            // _moveVel.y += gravity * Time.fixedDeltaTime;
        }

        transform.position += _moveVel * Time.deltaTime; // move
        _moveVel *= smoof; // decelerate
    }

    private void Update()
    {
        if (Application.isPlaying == false)
            return;

        if (InputFocus)
        {
            _yaw += Input.GetAxis("Mouse X") * lookSensitivity; // Right/Left camera movement
            _pitch -= Input.GetAxis("Mouse Y") * lookSensitivity; // Up/Down camera movement
            _pitch = Mathf.Clamp(_pitch, -90, 90);

            transform.localRotation = Quaternion.Euler(0, _yaw, 0f);
            cam.localRotation = Quaternion.Euler(_pitch, 0, 0);


            // leave focus mode stuff
            if (Input.GetKeyDown(KeyCode.Escape))
                InputFocus = false;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            InputFocus = true;
        }
    }
}