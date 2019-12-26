using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour {

    [Range(1, 100)]
    public float mouseSensitivity = 100f;
    public float maxCameraHeightOffset = 4;
    public Transform playerBody;

    float lateralRotation;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        playerBody.Rotate(Vector3.up * lateralRotation);
    }

    public void OnLook(InputAction.CallbackContext context) {
        Vector2 lookPositionOnScreen = context.ReadValue<Vector2>();

        lateralRotation = lookPositionOnScreen.x * mouseSensitivity * Time.deltaTime;
    }
}
