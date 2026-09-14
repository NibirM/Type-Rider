using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Position")]
    [Tooltip("Offset relative to the car. X = left/right, Y = height, Z = forward/backward.")]
    public Vector3 offset = new Vector3(0f, 5f, -8f);

    [Header("Smoothing")]
    [Tooltip("Lower values make the camera respond faster.")]
    public float positionSmoothTime = 0.08f;

    [Tooltip("Higher values make the camera rotate faster.")]
    public float rotationSmoothSpeed = 8f;

    [Header("Look Settings")]
    [Tooltip("Height above the car that the camera looks toward.")]
    public float lookHeight = 1f;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null)
            return;

        // Camera position follows the car's current rotation.
        Vector3 desiredPosition =
            target.position + target.TransformDirection(offset);

        // Smooth camera movement.
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            positionSmoothTime
        );

        // Look toward the car.
        Vector3 lookTarget = target.position;
        lookTarget.y += lookHeight;

        Vector3 lookDirection = lookTarget - transform.position;

        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(lookDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
        }
    }
}