using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public float jumpCount;
    public float maxJumps = 1f;

    public float slideSpeed;
    public bool isSliding;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public bool isGrounded = false;

    public LayerMask wallLayer;
    public Transform wallCheck;
    public float wallCheckRadius = 0.1f;
    public bool isWalled = false;

    public Vector2 wallJumpForce;
    public float wallJumpDirection;
    public bool wallJumping;

    private Vector2 movement;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    public float dashTime;
    public float dashCooldown;
    public float dashRange;
    public float dashRangeMin;
    public float dashRangeMax;
    private TrailRenderer trailRenderer;

    [SerializeField] private bool isDashing;
    [SerializeField] private bool canDash = true;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        trailRenderer = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.F))
        {
            rb.AddForce(new Vector2(-1000f, 10f), ForceMode2D.Impulse);
        }


        Move();
        Jump();
        WallSlide();
        WallJump();

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }

        FlipSprite();

        GroundCheck();
        WallCheck();    
    }

    private void FixedUpdate()
    {

    }

    void Move()
    {
        if (!isWalled && !wallJumping || !wallJumping)  
        {
            rb.velocity = new Vector2(movement.x * speed, rb.velocity.y);
        }
    }

    void Jump()
    {
        if (isGrounded)
        {
            jumpCount = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
        }
    }

    void WallSlide()
    {
        if (isWalled && !isGrounded)
        {
            isSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, -slideSpeed);
        }
        else
        {
            isSliding = false;
        }
    }
    
    void WallJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isWalled)
        {
            wallJumpDirection = spriteRenderer.flipX ? 1 : -1;

            Vector2 jumpForce = new Vector2(wallJumpDirection * wallJumpForce.x, wallJumpForce.y);

            rb.AddForce(jumpForce, ForceMode2D.Impulse);

            wallJumping = true;

            Debug.Log("Wall Jump triggered. Direction: " + wallJumpDirection + " Force: " + jumpForce);
            Debug.Log("Velocity after force: " + rb.velocity);
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        Vector2 dashDirection = new Vector2(movement.x, movement.y).normalized;
        if (dashDirection == Vector2.zero)
        {
            dashDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        rb.velocity = dashDirection * dashRange;

        trailRenderer.enabled = true;

        yield return new WaitForSeconds(dashTime);

        rb.velocity = Vector2.zero;
        rb.gravityScale = originalGravity;

        isDashing = false;
        trailRenderer.enabled = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void FlipSprite()
    {
        if (movement.x < 0f && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
        }
        else if (movement.x > 0f && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false;
        }
    }

    void GroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            wallJumping = false;
        }
    }

    void WallCheck()
    {
        Vector2 wallCheckPosition = transform.position + new Vector3(spriteRenderer.flipX ? -0.5f : 0.5f, 0f, 0f);

        Debug.DrawLine(wallCheckPosition, wallCheckPosition + Vector2.up, Color.red);

        isWalled = Physics2D.OverlapCircle(wallCheckPosition, wallCheckRadius, wallLayer);

        if (isWalled)
        {
            Debug.DrawLine(wallCheckPosition, wallCheckPosition + new Vector2(wallCheckRadius, 0), Color.green);
        }
    }
}

