using System.Collections;
using UnityEngine;

public enum MovementState
{
    Idling,
    Moving,
    Jumping,
    WallJumping,
    WallSliding,
    Dashing,
    Swinging
}

public class PlayerStatemanchine : MonoBehaviour
{
    public MovementState _currentState;

    public float Speed;
    public float JumpForce;
    public float JumpStrafe;
    public float SlideSpeed;

    public float detachJumpForce;
    public float swingForce;

    public float TrampolineForce;
    public GameObject TrampolinePrefab;

    public float JumpCounter;
    public int JumpCounterLimit = 2;

    public Vector2 WallJumpForce;
    public float AirGravityScale;

    public float groundOverlapCheckRadius;
    public float wallOverlapCheckRadius;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    public Transform wallCheck;
    public Transform groundCheck;

    public float dashTime;
    public float dashCooldown;
    public float dashRange;
    [SerializeField]
    private bool _isDashing;
    [SerializeField]
    private bool _canDash;

    [SerializeField]
    private bool _isOnWall;
    [SerializeField]
    private bool _isGrounded;

    [SerializeField]
    private float _coyoteTime = 0.2f;
    [SerializeField]
    private float _coyoteTimer;

    [SerializeField]
    private float _jumpBufferTime = 0.25f;
    [SerializeField]
    private float _jumpBufferTimer;

    [SerializeField]
    private bool _isSwinging;

    public float rayLength;
    public float cornerPushForce = 2f;
    public float offSetUnderCeiling;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private TrailRenderer _trailRenderer;
    private Vector2 _movement;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _currentState = MovementState.Idling;
    }

    void Update()
    {
        _movement.x = Input.GetAxis("Horizontal");
        _movement.y = Input.GetAxis("Vertical");

        GroundCheck();
        WallCheck();
        SpawnTrampoline();
        CornerCorrection();

        switch (_currentState)
        {
            case MovementState.Idling:
                ManageIdle();
                break;
            case MovementState.Moving:
                ManageMove();
                break;
            case MovementState.Jumping:
                ManageJump();
                break;
            case MovementState.WallSliding:
                ManageWallSlide();
                break;
            case MovementState.Dashing:
                ManageDash();
                break;
            case MovementState.Swinging:
                ManageSwing();
                break;
        }
    }

    private void ManageIdle()
    {
        if (_isGrounded && Mathf.Abs(_movement.x) > 0.1f)
        {
            _currentState = MovementState.Moving;
        }

        if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            _currentState = MovementState.Jumping;
            ExecuteJump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }
    }

    private void ManageMove()
    {
        _rb.velocity = new Vector2(_movement.x * Speed, _rb.velocity.y);

        FlipSprite();

        if (JumpCounter < JumpCounterLimit && Mathf.Abs(_movement.x) < 0.1f)
        {
            _currentState = MovementState.Idling;
            _rb.velocity = Vector2.zero;
        }

        if (JumpCounter < JumpCounterLimit && Input.GetKeyDown(KeyCode.Space))
        {
            _currentState = MovementState.Jumping;
            ExecuteJump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }
    }

    private void ManageJump()
    {
        // Improves the controll while jumping
        _rb.AddForce(new Vector2(_movement.x * JumpStrafe, 0), ForceMode2D.Force);

        if (JumpCounter < JumpCounterLimit && Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteJump();
        }

        if (_isGrounded && _rb.velocity.y <= 0f)
        {
            if (Mathf.Abs(_movement.x) > 0.01f)
            {
                _currentState = MovementState.Moving;
            }
            else
            {
                _currentState = MovementState.Idling;
            }
            JumpCounter = 0;
        }

        if (_isOnWall && !_isGrounded)
        {
            _currentState = MovementState.WallSliding;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }
    }

    private void ExecuteJump()
    {
        _rb.velocity = new Vector2(_movement.x * Speed, JumpForce);

        if (_isGrounded && _coyoteTimer > 0 && _jumpBufferTimer > 0)
        {
            _rb.velocity = new Vector2(_movement.x * Speed, JumpForce);
        }

        JumpCounter++;
    }

    private void ManageWallSlide()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, -SlideSpeed);

        if (_isGrounded)
        {
            _currentState = MovementState.Moving;
            JumpCounter = 0;
        }

        if (_isOnWall && Mathf.Abs(_rb.velocity.x) >= 0f && Input.GetKeyDown(KeyCode.Space))
        {

            _currentState = MovementState.Jumping;
            ExecuteWallJump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }
    }

    private void ExecuteWallJump()
    {
        if (_isOnWall && !_isGrounded)
        {
            float direction = _spriteRenderer.flipX ? 1f : -1f;

            _spriteRenderer.flipX = !_spriteRenderer.flipX;

            _rb.velocity = new Vector2(direction * WallJumpForce.x, WallJumpForce.y);

            _rb.gravityScale = AirGravityScale;

            // Adds a small buffer to the Character, so that the WallCheck isn't too fast and switches instantly back to WallSlide State
            transform.position += new Vector3(direction * 0.1f, 0f, 0f);

            _isOnWall = false;
        }
    }

    private void ManageDash()
    {
        if (!_isDashing && _isGrounded && _rb.velocity.y <= 0f)
        {
            if (Mathf.Abs(_movement.x) > 0.01f)
            {
                _currentState = MovementState.Moving;
            }
            else
            {
                _currentState = MovementState.Idling;
            }
            JumpCounter = 0;
        }

        if (!_isDashing && !_isGrounded)
        {
            _currentState = MovementState.Jumping;
        }

        if (!_isDashing && _isOnWall && !_isGrounded)
        {
            _currentState = MovementState.WallSliding;
        }
    }

    private IEnumerator ExecuteDash()
    {
        _isDashing = true;
        _canDash = false;

        float originalGravity = 6f;
        _rb.gravityScale = 0f;

        Vector2 dashDirection = new Vector2(_movement.x, _movement.y).normalized;

        if (dashDirection == Vector2.zero)
        {
            dashDirection = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        _rb.velocity = dashDirection * dashRange;

        _trailRenderer.enabled = true;

        yield return new WaitForSeconds(dashTime);

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = originalGravity;

        _isDashing = false;
        _trailRenderer.enabled = false;

        yield return new WaitForSeconds(dashCooldown);
        _canDash = true;
    }

    private void ManageSwing()
    {
        Rigidbody2D ropeRb = transform.parent.GetComponent<Rigidbody2D>();

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            ropeRb.AddTorque(swingForce * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            ropeRb.AddTorque(-swingForce * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteDetachFromRope();
        }
    }

    private void SpawnTrampoline()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3 spawnPosition = transform.position + new Vector3(0f, -1f, 0f);

            GameObject spawnedTrampoline = Instantiate(TrampolinePrefab, spawnPosition, Quaternion.identity);

            Destroy(spawnedTrampoline, 0.5f);
        }
    }

    // Spawn Trampoline under Player and give him a bounce
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trampoline"))
        {
            _rb.velocity = new Vector2(_movement.x, 0);
            _rb.AddForce(Vector2.up * TrampolineForce, ForceMode2D.Impulse);
        }

        if (collision.gameObject.CompareTag("SwingObject"))
        {
            ExecuteAttachToRope(collision.gameObject);
        }
    }

    // Dash through destroyable objects without getting stuck
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Changed the Collision Rate to 0.001
        if (other.gameObject.CompareTag("Destroyable") && _isDashing)
        {
            Destroy(other.transform.parent.gameObject);
        }
    }

    private void GroundCheck()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundOverlapCheckRadius, groundLayer);

        if (_isGrounded)
        {
            _coyoteTimer = _coyoteTime;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }
    }
    private void CornerCorrection()
    {
        Vector2 characterTop = (Vector2)transform.position + new Vector2(0f, 0.6f);

        bool rightWallHit = Physics2D.Raycast(characterTop + new Vector2(0.4f, 0f), Vector2.right, rayLength, wallLayer);
        bool rightCeilingHit = Physics2D.Raycast(characterTop + new Vector2(0.5f, 0f), Vector2.up, rayLength, wallLayer);
        bool isRightCorner = rightWallHit && rightCeilingHit;

        bool leftWallHit = Physics2D.Raycast(characterTop + new Vector2(-0.4f, 0f), Vector2.left, rayLength, wallLayer);
        bool leftCeilingHit = Physics2D.Raycast(characterTop + new Vector2(-0.5f, 0f), Vector2.up, rayLength, wallLayer);
        bool isLeftCorner = leftWallHit && leftCeilingHit;

        Debug.DrawRay(characterTop + new Vector2(0.4f, 0f), Vector2.right * rayLength, Color.red);
        Debug.DrawRay(characterTop + new Vector2(0.5f, 0f), Vector2.up * rayLength, Color.red);
        Debug.DrawRay(characterTop + new Vector2(-0.4f, 0f), Vector2.left * rayLength, Color.blue);
        Debug.DrawRay(characterTop + new Vector2(-0.5f, 0f), Vector2.up * rayLength, Color.blue);

        if (isLeftCorner && !isRightCorner && !_isOnWall)
        {
            _spriteRenderer.flipX = false;
            RedirectAroundCorner();
        }
        else if (isRightCorner && !isLeftCorner && !_isOnWall)
        {
            _spriteRenderer.flipX = true;
            RedirectAroundCorner();
        }
    }

    private void RedirectAroundCorner()
    {
        float pushDirection = _spriteRenderer.flipX ? -1f : 1f;

        Vector2 currentPosition = _rb.position;
        Vector2 targetPosition = currentPosition + new Vector2(pushDirection * offSetUnderCeiling, 0);

        _rb.position = targetPosition;

        _rb.velocity = new Vector2(pushDirection * cornerPushForce, JumpForce);
    }

    private void WallCheck()
    {
        Vector2 wallCheckPosition = transform.position + new Vector3(_spriteRenderer.flipX ? -0.5f : 0.5f, 0f, 0f);

        _isOnWall = Physics2D.OverlapCircle(wallCheckPosition, wallOverlapCheckRadius, wallLayer);

        Debug.DrawLine(wallCheckPosition, wallCheckPosition + Vector2.up, Color.red);
        Debug.DrawLine(wallCheckPosition, wallCheckPosition + new Vector2(wallOverlapCheckRadius, 0), Color.magenta);
    }

    private void FlipSprite()
    {
        if (_movement.x < -0.1f && !_spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = true;
        }
        else if (_movement.x > 0.1f && _spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = false;
        }
    }

    private void ExecuteAttachToRope(GameObject swingableObject)
    {
        if (_currentState != MovementState.Swinging)
        {
            _currentState = MovementState.Swinging;

            Rigidbody2D swingableObjectRb = swingableObject.GetComponent<Rigidbody2D>();

            Vector2 playerVelocity = _rb.velocity;
            
            swingableObjectRb.AddForceAtPosition(playerVelocity * _rb.mass, transform.position, ForceMode2D.Impulse);

            transform.parent = swingableObject.transform;

            _rb.isKinematic = true;
        }
    }

    private void ExecuteDetachFromRope()
    {
        if (_currentState == MovementState.Swinging)
        {
            _currentState = MovementState.Jumping;

            transform.parent = null;

            _rb.isKinematic = false;

            Rigidbody2D ropeRb = GetComponentInParent<Rigidbody2D>();
            
            if (ropeRb != null)
            {
                _rb.velocity = ropeRb.velocity + new Vector2(0f, detachJumpForce);
            }
            else
            {
                _rb.velocity += new Vector2(0f, detachJumpForce);
            }
        }
    }
}

