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

    public float overlapCheckRadius;
    public LayerMask groundLayer;
    public Transform groundCheck;

    public LayerMask wallLayer;
    public Transform[] wallCheck;

    public float wallJumpForce = 40f;
    public float wallHangTimer;
    public float wallSlideSpeed;
    public float airGravityScale = 6f;

    public bool isGrounded = true;
    public bool isOnWall = false;
    private bool isAtJumpApex = false;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;

    private float coyoteTime = 0.2f;
    private float coyoteTimer = 0f;

    // Apex timing
    private float apexTimeCounter = 0f;
    public float apexDuration; // Duration to apply reduced gravity at the apex
    public float reducedGravityScale; // Reduced gravity scale during the apex


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
        // WallHang();
        // WallJump();
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

            // Check if we are near the apex (when velocity is near zero)
            if (rb.velocity.y > 0f && Mathf.Abs(rb.velocity.y) < 0.1f)
            {
                isAtJumpApex = true;
                apexTimeCounter = apexDuration; // Reset the apex timer
            }

            // If we're at the apex, apply reduced gravity for a short time
            if (isAtJumpApex)
            {
                if (apexTimeCounter > 0f)
                {
                    rb.gravityScale = reducedGravityScale;
                    apexTimeCounter -= Time.deltaTime;
                }
                else
                {
                    rb.gravityScale = airGravityScale; // Reset gravity after apex duration
                    isAtJumpApex = false;
                }
            }
            else
            {
                rb.gravityScale = airGravityScale;
            }
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
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, overlapCheckRadius, groundLayer);
    }

    void WallCheck()
    {
        float wallJumpDirection = 1f;
        isOnWall = false;

        foreach (Transform wall in wallCheck)
        {
            if (Physics2D.OverlapCircle(wall.position, overlapCheckRadius, wallLayer))
            {
                isOnWall = true;

                // Determine jump direction based on sprite's facing direction
                wallJumpDirection = spriteRenderer.flipX ? 1f : -1f; // If flipped, jump right; else, jump left
                break;
            }
        }
    }

    void FlipSprite()
    {
        if (movement.x < 0f)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }


}
