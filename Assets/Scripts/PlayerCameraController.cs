using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour {

    [Range(1, 100)]
    public float mouseSensitivity = 100f;
    public float maxCameraHeightOffset = 4;
    public Transform playerBody;

    float lateralOffset;
    float verticalRotation;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        playerBody.Rotate(Vector3.up * lateralOffset);
        transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    public void OnLook(InputAction.CallbackContext context) {
        Vector2 lookPositionOnScreen = context.ReadValue<Vector2>();

        lateralOffset = lookPositionOnScreen.x * mouseSensitivity * Time.deltaTime;
        verticalRotation -= lookPositionOnScreen.y * mouseSensitivity * Time.deltaTime;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
    }
}
