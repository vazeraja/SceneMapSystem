using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TNS.InputMiddleware;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private bool focusOnPlay;
    [SerializeField] private new PlayerCamera camera;
    [SerializeField] private new Rigidbody rigidbody;
    [SerializeField] private new CapsuleCollider collider;
    [SerializeField] private InputProvider inputProvider;

    [Title("Settings")]
    [SerializeField] public float rotateSpeed = 90f;
    [SerializeField] public Vector3 gravity = new Vector3(0, -20, 0);
    [SerializeField] public float verticalSnapDown = 0.45f;
    [SerializeField] public float groundDist = 0.01f;
    [SerializeField] public float maxWalkingAngle = 60f;
    [SerializeField] public int maxBounces = 5;
    [Tooltip("Decrease in momentum when walking into objects (such as walls) at an angle as an exponential." +
             "Values between [0, 1] so values smaller than 1 create a positive curve and grater than 1 for a negative curve")]
    [SerializeField] public float anglePower = 0.5f;

    [ShowInInspector, ReadOnly]
    private float _elapsedFalling = 0f;
    [ShowInInspector, ReadOnly]
    private Vector3 _velocity;
    [ShowInInspector, ReadOnly]
    private Vector2 _cameraAngle;

    private const float c_Epsilon = 0.001f;
    private const float c_MaxAngleShoveDegrees = 180.0f - c_BufferAngleShove;
    private const float c_BufferAngleShove = 120.0f;

    private const float c_MinPitch = -90;
    private const float c_MaxPitch = 90;

    private InputState InputState => inputProvider;


    private void Start()
    {
        if (focusOnPlay) InputManager.InputFocus = true;

        camera.SetMainTarget(transform);
        camera.Init();

        inputProvider.OnLeftClickPressed += () => InputManager.InputFocus = true;
    }

    private void Update()
    {
        camera.RotateCamera(InputState.mouseDeltaX, InputState.mouseDeltaY);

        (bool onGround, float groundAngle) = CheckGrounded(out RaycastHit groundHit);
        bool falling = !(onGround && groundAngle <= maxWalkingAngle);

        if (falling)
        {
            _velocity += gravity * Time.deltaTime;
            _elapsedFalling += Time.deltaTime;
        }
        else
        {
            _velocity = Vector3.zero;
            _elapsedFalling = 0;
        }

        transform.position = MovePlayer(_velocity * Time.deltaTime);

        Debug.Log("Grounded: " + onGround);
    }

    private Vector3 MovePlayer(Vector3 movement)
    {
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;

        Vector3 remaining = movement;

        int bounces = 0;

        while (bounces < maxBounces && remaining.magnitude > c_Epsilon)
        {
            float distance = remaining.magnitude;
            if (CastSelf(position, rotation, remaining.normalized, distance, out RaycastHit hit) == false)
            {
                position += remaining;
                break;
            }

            if (hit.distance == 0)
                break;

            float fraction = hit.distance / distance;
            position += remaining * fraction;
            position += hit.normal * c_Epsilon;
            remaining *= (1 - fraction);

            Vector3 planeNormal = hit.normal;

            float angleBetween = Vector3.Angle(hit.normal, remaining) - 90f;
            angleBetween = Mathf.Min(c_MaxAngleShoveDegrees, Mathf.Abs(angleBetween));
            float normalizedAngle = angleBetween / c_MaxAngleShoveDegrees;

            remaining *= Mathf.Pow(1 - normalizedAngle, anglePower) * 0.9f + 0.1f;

            Vector3 projected = Vector3.ProjectOnPlane(remaining, planeNormal).normalized * remaining.magnitude;

            if (projected.magnitude + c_Epsilon < remaining.magnitude)
            {
                remaining = Vector3.ProjectOnPlane(remaining, Vector3.up).normalized * remaining.magnitude;
            }
            else
            {
                remaining = projected;
            }

            bounces++;

            print(bounces);
        }

        return position;
    }

    private (bool, float) CheckGrounded(out RaycastHit groundHit)
    {
        bool onGround = CastSelf(transform.position, transform.rotation, Vector3.down, groundDist, out groundHit);
        float angle = Vector3.Angle(groundHit.normal, Vector3.up);
        return (onGround, angle);
    }

    /// <summary>
    /// Snap the player down if they are within a specific distance of the ground.
    /// </summary>
    public void SnapPlayerDown()
    {
        bool closeToGround = CastSelf(
            transform.position,
            transform.rotation,
            Vector3.down,
            verticalSnapDown,
            out RaycastHit groundHit);

        // If within the threshold distance of the ground
        if (closeToGround && groundHit.distance > 0)
        {
            // Snap the player down the distance they are from the ground
            transform.position += Vector3.down * (groundHit.distance - c_Epsilon * 2);
        }
    }

    public bool CastSelf(Vector3 pos, Quaternion rot, Vector3 dir, float dist, out RaycastHit hit)
    {
        // Get Parameters associated with the KCC
        Vector3 center = rot * collider.center + pos;
        float radius = collider.radius;
        float height = collider.height;

        // Get top and bottom points of collider
        Vector3 bottom = center + rot * Vector3.down * (height / 2 - radius);
        Vector3 top = center + rot * Vector3.up * (height / 2 - radius);

        // Check what objects this collider will hit when cast with this configuration excluding itself
        IEnumerable<RaycastHit> hits = Physics.CapsuleCastAll(
                top, bottom, radius, dir, dist, ~0, QueryTriggerInteraction.Ignore)
            .Where(hit => hit.collider.transform != transform);
        bool didHit = hits.Count() > 0;

        // Find the closest objects hit
        float closestDist = didHit ? Enumerable.Min(hits.Select(hit => hit.distance)) : 0;
        IEnumerable<RaycastHit> closestHit = hits.Where(hit => hit.distance == closestDist);

        // Get the first hit object out of the things the player collides with
        hit = closestHit.FirstOrDefault();

        // Return if any objects were hit
        return didHit;
    }
}