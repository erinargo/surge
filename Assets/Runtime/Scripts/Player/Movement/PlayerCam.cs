using System;
using UnityEngine;

public class PlayerCam : MonoBehaviour {
    [SerializeField] private float defaultDistance = 6f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 10f;
    [Space]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float zoomSharpness = 10f;
    [Space]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float rotationSharpness = 10000f;
    [SerializeField] private float followSharpness = 10000f;
    [Space]
    [SerializeField] private float defaultVerticalAngle = 20f;
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;
    
    private Transform _cameraTarget;
    private Vector3 _currentFollowPosition, _currentDirection;
    private float _targetVerticalAngle;

    private float _currentDistance, _targetDistance;

    void Awake() {
        _currentDistance = defaultDistance;
        _targetDistance = _currentDistance;
        _targetVerticalAngle = 0f;
        _currentDirection = Vector3.forward;
    }

    public void SetTransform(Transform t) {
        _cameraTarget = t;

        _currentFollowPosition = t.position;
        _currentDirection = t.forward;
    }

    void OnValidate() {
        defaultDistance = Mathf.Clamp(defaultDistance, minDistance, maxDistance);
        defaultVerticalAngle = Mathf.Clamp(defaultVerticalAngle, minVerticalAngle, maxVerticalAngle);
    }

    void HandleRotationInput(float deltaTime, Vector3 rotationInput, out Quaternion targetRotation) {
        Quaternion rotationFromInput = Quaternion.Euler(_cameraTarget.up * (rotationInput.x * rotationSpeed));
        _currentDirection = rotationFromInput * _currentDirection;

        Quaternion rotation = Quaternion.LookRotation(_currentDirection, _cameraTarget.up);

        _targetVerticalAngle -= (rotationInput.y * rotationSpeed);
        _targetVerticalAngle = Mathf.Clamp(_targetVerticalAngle, minVerticalAngle, maxVerticalAngle);

        Quaternion verticalRotation = Quaternion.Euler(_targetVerticalAngle, 0, 0);

        targetRotation = Quaternion.Slerp(transform.rotation, 
            rotation * verticalRotation, 
            rotationSharpness * deltaTime
        );

        transform.rotation = targetRotation;
    }

    void HandlePosition(float deltaTime, float zoomInput, Quaternion targetRotation) { // Zoom
        _targetDistance += zoomInput * zoomSpeed;
        _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);

        _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, 
            _cameraTarget.position, 
            1f - Mathf.Exp(-followSharpness * deltaTime)
        );

        Vector3 targetPosition = 
            _currentFollowPosition - ((targetRotation * Vector3.forward) * _currentDistance);

        _currentDistance = Mathf.Lerp(
            _currentDistance, 
            _targetDistance, 
            1 - Mathf.Exp(-zoomSharpness * deltaTime)
        );

        transform.position = targetPosition;
    }

    public void UpdateWithInput(float deltaTime, float zoomInput, Vector3 rotationInput) {
        if (_cameraTarget) {
            HandleRotationInput(deltaTime, rotationInput, out Quaternion targetRotation);
            HandlePosition(deltaTime, zoomInput, targetRotation);
        }
    }
}
