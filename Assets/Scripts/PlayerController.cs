using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CircleCollider2D playerCollider;
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCutMtp;
    [SerializeField] private float jumpBufferTime;
    [SerializeField] private float jumpCoyoteTime;
    [SerializeField] private float gravityScaleMtp;
    [SerializeField] private float platformInvisibilityTime;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldownTime;
    [SerializeField] private float dashMinSpeedVertical;
    [SerializeField] private float dashMaxSpeedVertical;
    [SerializeField] private float dashMinSpeedHorizontal;
    [SerializeField] private float dashMaxSpeedHorizontal;
    [SerializeField] private float maxx;
    private GameObject currentPlatform;
    private float gravityScale;
    private float lastGroundedTime;
    private float lastJumpTime;
    private float dashCooldown;
    private bool isJumping;
    private bool blockMove;
    private bool usedDash;

    private float input;
    void Start()
    {
        gravityScale = rb.gravityScale;
    }

    private void Update()
    {

        //нажатия с клавиш
        input = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            lastJumpTime = jumpBufferTime;

        if(lastGroundedTime > 0 && lastJumpTime > 0 && !isJumping)
            Jump();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            if(rb.velocity.y > 0 && isJumping)
                rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMtp), ForceMode2D.Impulse);
            lastJumpTime = 0;
            //isJumpButtonRealesed = true;
        }

        if (Input.GetKey(KeyCode.S))
        {
            if (currentPlatform != null)
                StartCoroutine(DisableCollision());
        }

        if (Input.GetKey(KeyCode.Q))
        {
            if(dashCooldown < 0 && !usedDash)
            {
                StartCoroutine(VerticalDash());
                dashCooldown = dashCooldownTime;
            }
        }
        if (Input.GetKey(KeyCode.E))
        {
            if (dashCooldown < 0 && !usedDash)
            {
                StartCoroutine(HorizontalDash());
                dashCooldown = dashCooldownTime;
            }
        }
        //просто проверки
        if (Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            lastGroundedTime = jumpCoyoteTime;
            usedDash = false;
        }

        if (rb.velocity.y < 0 && isJumping)
            isJumping = false;

        //таймеры
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
        dashCooldown -= Time.deltaTime;
    }
    void FixedUpdate()
    {
        if (!blockMove)
        {
            float speedDif = input * maxSpeed - rb.velocity.x;
            speedDif *= Mathf.Abs(input) > 0.5 ? acceleration : deceleration;
            rb.AddForce(speedDif * Vector2.right);
            if (rb.velocity.y < maxFallSpeed)
                rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
        }
        
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
        //isJumpButtonRealesed = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            currentPlatform = collision.gameObject;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Platform")
        {
            currentPlatform = null;
        }
    }
    private IEnumerator DisableCollision()
    {
        TilemapCollider2D platformCollider = currentPlatform.GetComponent<TilemapCollider2D>();
        Physics2D.IgnoreCollision(playerCollider, platformCollider);
        yield return new WaitForSeconds(platformInvisibilityTime);
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }
    private IEnumerator HorizontalDash()
    {
        usedDash = true;
        blockMove = true;
        float dashspeed = 0;
        if (rb.velocity.y > 0)
        {
            dashspeed = Mathf.Lerp(dashMinSpeedHorizontal, dashMaxSpeedHorizontal, Mathf.Clamp01(rb.velocity.y / (jumpForce - 2)));
        }
        else
        {
            dashspeed = Mathf.Lerp(dashMinSpeedHorizontal, dashMaxSpeedHorizontal, Mathf.Clamp01(rb.velocity.y / (maxFallSpeed + 3)));
        }
        dashspeed = rb.velocity.x > 0 ? -dashspeed : dashspeed;
        rb.velocity = new Vector2(dashspeed, 0);
        yield return new WaitForSeconds(dashTime);
        usedDash = true;
        blockMove = false;
        Debug.Log(dashspeed);
    }
    private IEnumerator VerticalDash()
    {
        usedDash = true;
        blockMove = true;
        float dashspeed = Mathf.Lerp(dashMaxSpeedVertical, dashMinSpeedVertical, Mathf.Clamp01(Mathf.Abs(rb.velocity.x) / maxSpeed));
        dashspeed = rb.velocity.y > 0 ? -dashspeed : dashspeed;
        rb.velocity = new Vector2(0, dashspeed);
        yield return new WaitForSeconds(dashTime);
        usedDash = true;
        blockMove = false;
        Debug.Log(dashspeed);
    }
}
