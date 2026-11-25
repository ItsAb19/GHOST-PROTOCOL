using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 4f;
    [SerializeField, Min(1f)] private float sprintMultiplier = 1.75f;
    [SerializeField, Min(0f)] private float lookSpeed = 150f;

    private CharacterController controller;
    private Camera playerCamera;
    private float pitch;
    private float verticalSpeed;
    private bool isSprinting;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null || playerCamera.transform == transform)
        {
            Debug.LogError("Place the FPS camera under the Player in the Hierarchy.", this);
            enabled = false;
            return;
        }

        pitch = Mathf.DeltaAngle(0f, playerCamera.transform.localEulerAngles.x);
    }

    private void OnDisable()
    {
        isSprinting = false;
    }

    private void Update()
    {
        Gamepad pad = Gamepad.current;
        Vector2 moveInput = Vector2.zero;

        if (pad != null)
        {
            Look(pad.rightStick.ReadValue());
            moveInput = pad.leftStick.ReadValue();
            if (pad.leftStickButton.wasPressedThisFrame)
            {
                isSprinting = true;
            }
        }

        // The Input System reports zero when the stick is inside its centre dead zone.
        if (moveInput.sqrMagnitude == 0f)
        {
            isSprinting = false;
        }

        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        Move(moveInput, speed);
    }

    private void Look(Vector2 stick)
    {
        float turnStep = lookSpeed * Time.deltaTime;
        transform.Rotate(0f, stick.x * turnStep, 0f);

        pitch = Mathf.Clamp(pitch - stick.y * turnStep, -85f, 85f);
        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move(Vector2 stick, float speed)
    {
        stick = Vector2.ClampMagnitude(stick, 1f);
        Vector3 direction = transform.right * stick.x + transform.forward * stick.y;

        if (controller.isGrounded && verticalSpeed < 0f)
        {
            verticalSpeed = -2f;
        }

        verticalSpeed += Physics.gravity.y * Time.deltaTime;
        Vector3 velocity = direction * speed + Vector3.up * verticalSpeed;
        controller.Move(velocity * Time.deltaTime);
    }
}