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

    [Header("AUDIO")]
    public AudioClip jumpAudio;
    private AudioSource audioSource;

    [Header("GROUND CHECK")]
    public float groundCheckRadius;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public bool isGrounded = true;

    [Header("GRAVITY SCALE TEST")]
    [SerializeField] private float fallGravity;
    [SerializeField] private float maxGravityScale;

    [Header("JUMP BUFFER")]
    [SerializeField] private float jumpBufferTime;
    private float jumpBufferCounter;

    [Header("COYOTE TIME")]
    [SerializeField] private float coyoteTime;
    private float coyoteTimeCounter;

    [Header("JUMP LOAD")]
    [SerializeField] private float jumpTime;
    private float jumpTimeCounter;
    public bool isJumping;

    [Header("DASH")]
    public float dashTime;
    public float dashCooldown;
    public float dashRange;
    private TrailRenderer trailRenderer;

    [SerializeField] private bool isDashing;
    [SerializeField] private bool canDash;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = rb.GetComponent<SpriteRenderer>();
        trailRenderer = rb.GetComponent<TrailRenderer>();
        audioSource = GetComponent<AudioSource>();

        trailRenderer.enabled = false;
    }

    void Update()
    {
        movement.x = Input.GetAxis("Horizontal");

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        FlipSprite();

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0)
        {
            if (isGrounded || coyoteTimeCounter > 0)
            {
                Jump();
                jumpBufferCounter = 0f;
            }
            else if (!isGrounded && jumpCounter < maxJumpCounter)
            {
                Jump();
                jumpBufferCounter = 0f;
            }
        }

        if (Input.GetKey(KeyCode.Space) && isJumping && jumpTimeCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpTimeCounter -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space) || jumpTimeCounter <= 0)
        {
            isJumping = false;
        }

        if (rb.velocity.y < 0f)
        {
            rb.gravityScale += fallGravity * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.velocity = new Vector2(movement.x * speed, rb.velocity.y);
        }
    }

    private void FlipSprite()
    {
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

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        float dashGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashInputX = Input.GetAxis("Horizontal");
        float dashInputY = Input.GetAxis("Vertical");

        Vector2 dashDirection = new Vector2(dashInputX, dashInputY).normalized;

        if (dashDirection == Vector2.zero)
        {
            dashDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        rb.velocity = dashDirection * dashRange;

        trailRenderer.enabled = true;

        yield return new WaitForSeconds(dashTime);

        rb.gravityScale = 1f; 
        isDashing = false;

        trailRenderer.enabled = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
