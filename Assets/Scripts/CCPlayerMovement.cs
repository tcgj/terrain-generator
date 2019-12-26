using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CCPlayerMovement : MonoBehaviour {

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpHeight = 2f;

    [Header("Gravity Settings")]
    public float gravity = Physics.gravity.y;
    public Transform groundChecker;
    public float checkerRadius;
    public LayerMask groundLayer;

    CharacterController controller;
    Vector3 moveDirection;
    Vector3 velocity;
    bool isGrounded;

    void Start() {
        controller = GetComponent<CharacterController>();
    }

    void FixedUpdate() {
        isGrounded = Physics.CheckSphere(groundChecker.position, checkerRadius, groundLayer);

        if (isGrounded && velocity.y < 0) {
            velocity.y = -0.5f;
        }

        // Lateral movement
        Vector3 translation = transform.right * moveDirection.x + transform.forward * moveDirection.z;
        controller.Move(translation * moveSpeed * Time.deltaTime);

        // Free fall
        velocity.y += gravity * Time.deltaTime;
        Vector3 displacement = 0.5f * velocity * Time.deltaTime;
        controller.Move(displacement);
    }

    public void OnMove(InputAction.CallbackContext context) {
        Vector2 lateral = context.ReadValue<Vector2>();

        moveDirection.x = lateral.x;
        moveDirection.z = lateral.y;
    }

    public void OnJump(InputAction.CallbackContext context) {
        if (context.performed && isGrounded) {
            velocity.y += Mathf.Sqrt(-2f * jumpHeight * gravity);
        }
    }
}
