using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float jumpForce;
    public bool isFalling;
    public ParticleSystem jumpDust;

    public int jumpCounter;
    private int maxJumpCounter = 2;

    [Header("FLIP SPRITE")]
    private SpriteRenderer spriteRenderer;

    [Header("AUDIO")]
    public AudioClip jumpAudio;
    private AudioSource audioSource;

    [Header("GROUND CHECK")]
    public float overlapCheckRadius;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public bool isGrounded = true;

    [Header("WALL CHECK")]
    public LayerMask wallLayer;
    public Transform[] wallCheck;
    public bool isWalled = false;

    [Header("WALL SLIDE")]
    public float wallSlideSpeed; 

    [Header("WALL HANG")]
    public float wallHangTimer;

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

    [Header("WALL JUMP")]
    public float wallPushOff = 20f;

    [Header("TRAMPOLIN")]
    public float trampolinForce;
    public float trampolinTimer;
    public float trampolinCooldown;
    public GameObject trampolinPrefab; 

    [Header("DASH")]
    public float dashTime;
    public float dashCooldown;
    public float dashRange;
    public float dashRangeMin;
    public float dashRangeMax;
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

        // Grounded Check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, overlapCheckRadius, groundLayer);

        OnWall();
        WallHang();
        WallJump();
        SpawnTrampolin();
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
            // Clamp Gravity to max Gravity Scale
            rb.gravityScale = Mathf.Min(rb.gravityScale + fallGravity * Time.deltaTime, maxGravityScale);
        }

        if (isDashing)
        {
            rb.gravityScale = 0f;
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

        // Double Jump add Jump to Jump Counter
        if (Input.GetKey(KeyCode.Space) && isJumping && jumpTimeCounter > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpTimeCounter -= Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.Space) || jumpTimeCounter <= 0)
        {
            isJumping = false;
        }

        // Increase Gravity Scale when falling 
        if (rb.velocity.y < 0f && !isFalling)
        {
            isFalling = true;
            rb.gravityScale += fallGravity * Time.deltaTime;
        }
        else if (isGrounded)
        {
            isFalling = false;
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

    // Flip Sprite when Player is moving right/left
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
        // Increase Jump Counter for double Jump count
        jumpCounter++;
        jumpTimeCounter = jumpTime;
        jumpDust.Play();
        audioSource.PlayOneShot(jumpAudio);
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        // Set Dash Gravity to 0
        float dashGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashInputX = Input.GetAxis("Horizontal");
        float dashInputY = Input.GetAxis("Vertical");

        Vector2 dashDirection = new Vector2(dashInputX, dashInputY).normalized;

        // If no Input turn Player Sprite in direction
        if (dashDirection == Vector2.zero)
        {
            dashDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        // Clamps DashRange.y between min and max value
        dashDirection = new Vector2(dashDirection.x, Mathf.Clamp(dashDirection.y, dashRangeMin, dashRangeMax));

        // Dash range
        rb.velocity = dashDirection * dashRange;

        trailRenderer.enabled = true;

        yield return new WaitForSeconds(dashTime);

        // While dashing set Gravity to 0 and reset dash bool to false;
        rb.gravityScale = dashGravity;
        isDashing = false;

        trailRenderer.enabled = false;

        // Start dash cooldown, after cooldown canDash is resetted to true and avaiable
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // When player collides with DashResetter his canDash gets resetted
    private void OnTriggerEnter2D(Collider2D other) 
    { 
        if (other.gameObject.CompareTag("DashResetter"))
        {
            Destroy(other.gameObject);
            canDash = true;
        }

        if (other.gameObject.CompareTag("Destroyable") && isDashing)
        {
            Destroy(other.gameObject);
        }
    }

// Checks if the Player is next to a wall layer with two Wall Checks
private void OnWall()
    {
        isWalled = false;

        foreach (Transform wall in wallCheck)
        {
            if (Physics2D.OverlapCircle(wall.position, overlapCheckRadius, wallLayer))
            {
                isWalled = true;
                break;
            }
        }
    }

    private void WallHang()
    {
        if (isWalled && wallHangTimer > 0 && Input.GetKey(KeyCode.Q))
        {
            wallHangTimer -= Time.deltaTime;

            float colorChangeOverTime = 1 - (wallHangTimer / 2f);
            spriteRenderer.color = Color.Lerp(Color.white, Color.blue, colorChangeOverTime);

            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
        else if (isWalled && (wallHangTimer <= 0 || !Input.GetKey(KeyCode.E)))
        {
            rb.gravityScale = 1f;
            rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
            spriteRenderer.color = Color.white;
        }

        if (isGrounded)
        {
            spriteRenderer.color = Color.white;
            wallHangTimer = 2f;
        }
    }

    private void WallJump()
    {
        if (isWalled && !isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            float wallDirection = (spriteRenderer.flipX) ? 1f : -1f; 

            rb.velocity = new Vector2(wallDirection * wallPushOff, jumpForce);

            jumpDust.Play();

            rb.gravityScale = 1f;

            jumpCounter = 0;
        }
    }

    private void SpawnTrampolin()
    {
        Vector2 trampolinOffset = new Vector2(0f, -1f);

        if (rb.velocity.y > 0 && trampolinTimer <= 0 && Input.GetKey(KeyCode.E))
        {
            GameObject trampolinSpawn = Instantiate(trampolinPrefab, (Vector2)transform.position + trampolinOffset, Quaternion.identity);

            trampolinTimer = trampolinCooldown;

            Destroy(trampolinSpawn, 1.25f);
        }

        trampolinTimer -= Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trampolin"))
        {
            if (collision.contacts[0].normal.y > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, trampolinForce);
                jumpDust.Play();
            }
        }

        if (collision.gameObject.CompareTag("Destroyable") && isDashing)
        {
            Destroy(collision.gameObject);
        }
    }
}



