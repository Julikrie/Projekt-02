using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NewPlayer : MonoBehaviour
{
    public float speed = 10f;
    public float jumpForce = 12f;
    public float fallSpeed = -4f;
    public float wallJumpDirection = 1f;
    public float wallJumpDelay;

    public float groundOverlapCheckRadius;
    public float wallOverlapCheckRadius;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public Transform groundCheck;
    public Transform wallCheck;

    public Vector2 wallJumpForce;
    public float slideSpeed;
    public float airGravityScale = 6f;

    public bool isGrounded = true;
    public bool isOnWall = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;

    private float coyoteTime = 0.2f;
    private float coyoteTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = rb.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Move();
        Jump();
        FlipSprite();
        GroundCheck();
        WallCheck();
        WallHang();

        if (isOnWall && !isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            WallJump();
        }
    }

    void FixedUpdate()
    {
        ClampFallSpeed();
    }

    void Move()
    {
        movement.x = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(movement.x * speed, rb.velocity.y);
    }

    void Jump()
    {
        if (!isGrounded)
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            rb.gravityScale = 6;
        }

        if (coyoteTimer > 0 && Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            coyoteTimer = 0f;
        }
    }

    void ClampFallSpeed()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, fallSpeed));
        }
    }

    void GroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundOverlapCheckRadius, groundLayer);
    }

    void WallCheck()
    {
        Vector2 wallCheckPosition = transform.position + new Vector3(spriteRenderer.flipX ? -0.5f : 0.5f, 0f, 0f);  // Adjust the X offset to your character's width

        Debug.DrawLine(wallCheckPosition, wallCheckPosition + Vector2.up, Color.red);

        isOnWall = Physics2D.OverlapCircle(wallCheckPosition, wallOverlapCheckRadius, wallLayer);

        if (isOnWall)
        {
            Debug.DrawLine(wallCheckPosition, wallCheckPosition + new Vector2(wallOverlapCheckRadius, 0), Color.green);
        }
    }

    void WallHang()
    {
        if (isOnWall)
        {
            rb.velocity = new Vector2(rb.velocity.x, -slideSpeed);
        }
    }

void WallJump()
{
    if (isOnWall && !isGrounded)
    {
        float direction = spriteRenderer.flipX ? -1f : 1f; // Determine the direction to jump based on sprite flip

        // Apply a wall jump force in the correct direction
        rb.velocity = new Vector2(direction * wallJumpForce.x, wallJumpForce.y); 

        // Reset the coyote timer and apply the air gravity scale to prevent floating
        coyoteTimer = 0f;
        rb.gravityScale = airGravityScale;
    }
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
}
