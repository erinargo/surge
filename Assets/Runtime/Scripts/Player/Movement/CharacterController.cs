using KinematicCharacterController;
using Unity.Mathematics;
using UnityEngine;

public struct PlayerInputs {
    public float Forward;
    public float Right;
    public bool Jump;
    
    public Quaternion CamRotation;
}

public class CharacterController : MonoBehaviour, ICharacterController {
    [SerializeField] private KinematicCharacterMotor motor;
    
    [Space]
    [SerializeField] private float maxStableMovementSpeed = 10f;
    [SerializeField] private float stableMovementSharpness = 15f; 
    [SerializeField] private float orientationSharpness = 10f;
    [SerializeField] private float jumpSpeed = 10f;
    [SerializeField] private int _jumpLimit = 1;

    [Space] 
    // The one thing Sonic '06 Devs could never do
    [SerializeField] private Vector3 gravity = new Vector3(0f, -30f, 0f); 

    private Vector3 _moveInputVector, _lookInputVector;
    private bool _jumped;
    private int _timesJumped = 0;
    private bool _resetJumps;

    void Start() {
        motor.CharacterController = this;
    }

    public void SetInputs(ref PlayerInputs inputs) {
        if (_timesJumped >= _jumpLimit || motor.GroundingStatus.IsStableOnGround) _resetJumps = true;
        
        if ((inputs.Jump || _jumped) && (motor.GroundingStatus.IsStableOnGround || _timesJumped < _jumpLimit)) {
            _jumped = true; // May be buggy, do testing
        }

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

            Vector3 targetMovementVelocity = reorientedInput * maxStableMovementSpeed; // adjust later for sprinting, char speeds
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
