using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Transform cam;

    [Header("Movement Settings")]
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Jump & Gravity")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.2f; 
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    void Update()
    {
        // -ground
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // inputo
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // camera
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * vertical + camRight * horizontal;

        if (moveDir.magnitude >= 0.1f)
        {
            //rot camera
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), turnSmoothTime);
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        // --jump
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //debug-
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);
    }
}
