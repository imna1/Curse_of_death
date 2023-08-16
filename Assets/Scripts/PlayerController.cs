using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCutMtp;
    [SerializeField] private float jumpBufferTime;
    [SerializeField] private float jumpCoyoteTime;
    [SerializeField] private float gravityScaleMtp;
    private float gravityScale;
    [SerializeField] private float lastGroundedTime;
    [SerializeField] private float lastJumpTime;
    [SerializeField] private bool isJumping;
    private bool isJumpButtonRealesed;

    private float input;
    void Start()
    {
        gravityScale = rb.gravityScale;
    }

    private void Update()
    {
        input = Input.GetAxisRaw("Horizontal");
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            lastGroundedTime = jumpCoyoteTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            lastJumpTime = jumpBufferTime;

        if(lastGroundedTime > 0 && lastJumpTime > 0 && !isJumping)
            Jump();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if(rb.velocity.y > 0 && isJumping)
                rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMtp), ForceMode2D.Impulse);
            lastJumpTime = 0;
            isJumpButtonRealesed = true;
        }

        if(rb.velocity.y < 0 && isJumping)
        {
            isJumping = false;
        }

        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
    }
    void FixedUpdate()
    {
        float speedDif = input * maxSpeed - rb.velocity.x;
        speedDif *= Mathf.Abs(input) > 0.5 ? acceleration : deceleration;
        rb.AddForce(speedDif * Vector2.right);
        if(rb.velocity.y > 0)
        {
            rb.gravityScale = gravityScale;
        }
        else
        {
            rb.gravityScale = gravityScale * gravityScaleMtp;
        }
    }
    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastGroundedTime = 0;
        lastJumpTime = 0;
        isJumping = true;
        isJumpButtonRealesed = false;
    }
}
