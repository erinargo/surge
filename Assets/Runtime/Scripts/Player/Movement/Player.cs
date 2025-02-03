using System;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField] private PlayerCam playerCam;
    [SerializeField] private Transform cameraTarget;

    [SerializeField] private CharacterController characterController;

    private Vector3 _lookInputVector;

    void Start() {
        playerCam.SetTransform(cameraTarget);
    }

    void HandleCameraInput() {
        float mouseUp = Input.GetAxisRaw("Mouse Y");
        float mouseRight = Input.GetAxisRaw("Mouse X");
        float scrollInput = -Input.GetAxis("Mouse ScrollWheel");
        
        
        _lookInputVector = new Vector3(mouseRight, mouseUp, 0f);
        
        playerCam.UpdateWithInput(Time.deltaTime, scrollInput, _lookInputVector);
    }

    void HandleCharacterInputs() {
        // Later, add customization options for character inputs; 
        PlayerInputs inputs = new PlayerInputs();
        inputs.Forward = Input.GetAxisRaw("Vertical");
        inputs.Right = Input.GetAxisRaw("Horizontal");
        inputs.Jump = Input.GetKeyDown(KeyCode.Space);

        Debug.Log(Input.GetKeyDown(KeyCode.Space));
        Debug.Log(inputs.Jump);

        inputs.CamRotation = playerCam.transform.rotation;
        characterController.SetInputs(ref inputs);
    }

    void Update() {
        HandleCharacterInputs();
    }

    void LateUpdate() {
        HandleCameraInput(); 
    }
}
