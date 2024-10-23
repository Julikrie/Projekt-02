using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float jumpForce;

    public int jumpCounter;
    private int maxJumpCounter = 2;

    [Header("FLIP SPRITE")]
    private SpriteRenderer spriteRenderer;

    [Header ("AUDIO")]
    public AudioClip jumpAudio;
    private AudioSource audioSource;

    [Header("GROUND CHECK")]
    public float groundCheckRadius;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public bool isGrounded = true;

    [Header ("GRAVITY SCALE TEST")]
    [SerializeField] private float fallGravity;
    [SerializeField] private float maxGravityScale;

    [Header("JUMP BUFFER")]
    [SerializeField] private float jumpBufferTime;
    private float jumpBufferCounter;

    [Header ("COYOTE TIME")]
    [SerializeField] private float coyoteTime; 
    private float coyoteTimeCounter;

    [Header("JUMP LOAD")]
    [SerializeField] private float jumpTime;
    private float jumpTimeCounter;
    public bool isJumping;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = rb.GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        movement.x = Input.GetAxis("Horizontal");

        // Grounded 
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        FlipSprite();

        // Checking Grounded & Coyote Time & Jump Counter
        if (isGrounded)
        {
            jumpCounter = 0;
            coyoteTimeCounter = coyoteTime;
            rb.gravityScale = 1f;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            rb.gravityScale = Mathf.Min(rb.gravityScale + fallGravity * Time.deltaTime, maxGravityScale);
        }

        // When Jump start Counter
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        // Decrease Buffer Counter Time
        jumpBufferCounter -= Time.deltaTime;

        // Jump & Coyote Time & Jump Buffer
        if (jumpBufferCounter > 0)
        {
            // Jump if Grounded or Coyote Time avaiable
            if (isGrounded || coyoteTimeCounter > 0)
            {
                Jump();
                jumpBufferCounter = 0f;
            }
            // Double Jump possible if Counts avaiable 
            else if (!isGrounded && jumpCounter < maxJumpCounter)
            {
                Jump();
                jumpBufferCounter = 0f; 
            }
        }

        // Jumping when holding Space
        if (Input.GetKey(KeyCode.Space) && isJumping && jumpTimeCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpTimeCounter -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space) || jumpTimeCounter <= 0)
        {
            isJumping = false;
        }

        // Faster fall
        if (rb.velocity.y < 0f)
        {
            rb.gravityScale += fallGravity * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(movement.x * speed, rb.velocity.y);
    }

    void FlipSprite()
    {
        // Flip Sprite in the moving direction
        if (movement.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (movement.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        isJumping = true;
        jumpCounter++;
        jumpTimeCounter = jumpTime;
        audioSource.PlayOneShot(jumpAudio);
    }
}

