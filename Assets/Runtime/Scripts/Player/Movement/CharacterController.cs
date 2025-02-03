using KinematicCharacterController;
using Unity.Mathematics;
using UnityEngine;

public struct PlayerInputs {
    public float Forward;
    public float Right;
    public bool Jump;
    public bool Sprint;
    public bool Crouch;
    
    public Quaternion CamRotation;
}

public class CharacterController : MonoBehaviour, ICharacterController {
    [SerializeField] private KinematicCharacterMotor motor;

    [Space]
    [SerializeField] private float maxStableMovementSpeed = 10f;
    [SerializeField] private float maxStableSprintSpeed = 15f;
    [SerializeField] private float maxStableCrouchSpeed = 5f;
    [SerializeField] private float stableMovementSharpness = 15f; 
    [SerializeField] private float orientationSharpness = 10f;
    
    [Space] 
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private int _jumpLimit = 1;

    [Space] 
    // The one thing Sonic '06 Devs could never do
    [SerializeField] private Vector3 gravity = new Vector3(0f, -30f, 0f); 

    private Vector3 _moveInputVector, _lookInputVector;
    private bool _jumped;
    private int _timesJumped = 0;
    private bool _resetJumps;
    
    public float _characterSpeed;

    void Start() {
        motor.CharacterController = this;
    }

    public void SetInputs(ref PlayerInputs inputs) {
        if (_timesJumped >= _jumpLimit || motor.GroundingStatus.IsStableOnGround) _resetJumps = true;
        
        if ((inputs.Jump || _jumped) && (motor.GroundingStatus.IsStableOnGround || _timesJumped < _jumpLimit)) 
            _jumped = true;

        if (inputs.Sprint || inputs.Crouch) {
            _characterSpeed = inputs.Sprint ? maxStableSprintSpeed : maxStableCrouchSpeed; // prefer sprint over crouching for now, can change to sprint modifier later instead i.e if crouching && sprinting move faster than crouching but slower than walking
        } else _characterSpeed = maxStableMovementSpeed;

        Vector3 moveInputVector = 
            Vector3.ClampMagnitude(new Vector3(inputs.Right, 0f, inputs.Forward), 1f);
        Vector3 camPlanarDirection = 
            Vector3.ProjectOnPlane(inputs.CamRotation * Vector3.forward, motor.CharacterUp).normalized;

        if (camPlanarDirection.sqrMagnitude == 0f)
            camPlanarDirection = Vector3.ProjectOnPlane(inputs.CamRotation * Vector3.up, motor.CharacterUp).normalized;

        Quaternion camPlanarRotation = Quaternion.LookRotation(camPlanarDirection, motor.CharacterUp);

        // change these to change how the game feels to play 
        _moveInputVector = camPlanarRotation * moveInputVector;
        _lookInputVector = _moveInputVector.normalized;
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
        if (_lookInputVector.sqrMagnitude <= 0f || orientationSharpness <= 0f) return;

        Vector3 smoothedLookInputDirection = 
            Vector3.Slerp(
                motor.CharacterForward, 
                _lookInputVector,
                1 - Mathf.Exp(-orientationSharpness * deltaTime)
            ).normalized;
        
        currentRotation = quaternion.LookRotation(smoothedLookInputDirection, motor.CharacterUp);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
        if (motor.GroundingStatus.IsStableOnGround) { // The Gravity of the Situation is Palpable.
            float currentVelocityMagnitude = currentVelocity.magnitude;
            Vector3 groundNormal = motor.GroundingStatus.GroundNormal;

            currentVelocity = 
                motor.GetDirectionTangentToSurface(currentVelocity, groundNormal) * currentVelocityMagnitude;

            Vector3 inputRight = Vector3.Cross(_moveInputVector, motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(groundNormal, inputRight).normalized;

            Vector3 targetMovementVelocity = reorientedInput * _characterSpeed;
            currentVelocity = 
                Vector3.Lerp(
                    currentVelocity, 
                    targetMovementVelocity, 
                    1f - Mathf.Exp(-stableMovementSharpness * deltaTime));

            if (_resetJumps) {
                _timesJumped = 0;
                _resetJumps = false;
            }
        } else currentVelocity += gravity * deltaTime;
        
        if (_jumped) {
            currentVelocity += (motor.CharacterUp * jumpSpeed) - Vector3.Project(currentVelocity, motor.CharacterUp);
            _jumped = false;
            if (_timesJumped < _jumpLimit) _timesJumped++;
            
            motor.ForceUnground();
        }
    }

    public void BeforeCharacterUpdate(float deltaTime) {
    }

    public void PostGroundingUpdate(float deltaTime) {
    }

    public void AfterCharacterUpdate(float deltaTime) {
    }

    public bool IsColliderValidForCollisions(Collider coll) {
        return true;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
        ref HitStabilityReport hitStabilityReport) {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
        Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider) {
    }
}
