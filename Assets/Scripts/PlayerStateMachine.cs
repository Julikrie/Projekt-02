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

public class PlayerStateMachine : MonoBehaviour
{
    [SerializeField]
    private MovementState _currentState;

    public ParticleSystem JumpDust;
    public GameObject TrampolinePrefab;

    public LayerMask groundLayer;
    public LayerMask wallLayer;

    public Transform groundCheck;
    public Transform wallCheck;

    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private float _jumpStrafe;
    [SerializeField]
    private float _slideSpeed;

    [SerializeField]
    private float _detachJumpForce;
    [SerializeField]
    private float _swingForce;

    [SerializeField]
    private float _trampolineForce;

    [SerializeField]
    private float _jumpCounter;
    private int _jumpCounterLimit = 2;

    [SerializeField]
    private Vector2 _wallJumpForce;
    [SerializeField]
    private float _airGravityScale;

    [SerializeField]
    private float _groundOverlapCheckRadius;
    [SerializeField]
    private float _wallOverlapCheckRadius;

    [SerializeField]
    private float _dashTime;
    [SerializeField]
    private float _dashCooldown;
    [SerializeField]
    private float _dashRange;
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

    [SerializeField]
    private float _rayLength;
    [SerializeField]
    private float _cornerPushForce;
    [SerializeField]
    private float _offSetUnderCeiling;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private TrailRenderer _trailRenderer;
    private float _movementX;
    private float _movementY;

    private HingeJoint2D _hingeJoint;
    private float _swingAttachCooldown = 0f;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _currentState = MovementState.Idling;
    }

    void Update()
    {
        _movementX = Input.GetAxis("Horizontal");
        _movementY = Input.GetAxis("Vertical");

        if (_swingAttachCooldown > 0f)
        {
            _swingAttachCooldown -= Time.deltaTime;
        }

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
        if (Mathf.Abs(_movementX) > 0.1f)
        {
            _currentState = MovementState.Moving;
        }
        else
        {
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }

        if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            _currentState = MovementState.Jumping;
            ExecuteJump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash)
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }
    }

    private void ManageMove()
    {
        _rb.velocity = new Vector2(_movementX * _speed, _rb.velocity.y);

        FlipSprite();

        if (Mathf.Abs(_movementX) < 0.1f)
        {
            _currentState = MovementState.Idling;
        }

        if (_jumpCounter < _jumpCounterLimit && Input.GetKeyDown(KeyCode.Space))
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
        _rb.AddForce(new Vector2(_movementX * _jumpStrafe, 0), ForceMode2D.Force);

        if (_jumpCounter < _jumpCounterLimit && Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteJump();
        }

        if (_isGrounded && _rb.velocity.y <= 0f)
        {
            if (Mathf.Abs(_movementX) > 0.01f)
            {
                _currentState = MovementState.Moving;
            }
            else
            {
                _currentState = MovementState.Idling;
            }
            _jumpCounter = 0;
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
        _rb.velocity = new Vector2(_movementX * _speed, _jumpForce);

        JumpDust.Play();

        if (_isGrounded && _coyoteTimer > 0 && _jumpBufferTimer > 0)
        {
            _rb.velocity = new Vector2(_movementX * _speed, _jumpForce);
        }

        _jumpCounter++;
    }

    private void ManageWallSlide()
    {
        if (_isOnWall)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, -_slideSpeed);
        }

        if (_isGrounded)
        {
            _currentState = MovementState.Moving;
            _jumpCounter = 0;
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

            _rb.velocity = new Vector2(direction * _wallJumpForce.x, _wallJumpForce.y);

            _rb.gravityScale = _airGravityScale;

            transform.position += new Vector3(direction * 0.1f, 0f, 0f);

            JumpDust.Play();

            _isOnWall = false;
        }
    }

    private void ManageDash()
    {
        if (!_isDashing && _isGrounded && _rb.velocity.y <= 0f)
        {
            if (Mathf.Abs(_movementX) > 0.01f)
            {
                _currentState = MovementState.Moving;
            }
            else
            {
                _currentState = MovementState.Idling;
            }
            _jumpCounter = 0;
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

        Vector2 dashDirection = new Vector2(_movementX, _movementY).normalized;

        if (dashDirection == Vector2.zero)
        {
            dashDirection = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }

        _rb.velocity = dashDirection * _dashRange / _dashTime;

        _trailRenderer.enabled = true;

        yield return new WaitForSeconds(_dashTime);

        _rb.velocity = Vector2.zero;
        _rb.gravityScale = originalGravity;

        _isDashing = false;
        _trailRenderer.enabled = false;

        yield return new WaitForSeconds(_dashCooldown);
        _canDash = true;
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
            _rb.velocity = new Vector2(_movementX, 0);
            _rb.AddForce(Vector2.up * _trampolineForce, ForceMode2D.Impulse);
        }

        if (collision.gameObject.CompareTag("SwingObject"))
        {
            ExecuteAttachToSwing(collision.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Destroyable") && _isDashing)
        {
            Destroy(other.transform.parent.gameObject);
        }
    }

    private void CornerCorrection()
    {
        Vector2 characterTop = (Vector2)transform.position + new Vector2(0f, 0.6f);

        bool rightWallHit = Physics2D.Raycast(characterTop + new Vector2(0.4f, 0f), Vector2.right, _rayLength, wallLayer);
        bool rightCeilingHit = Physics2D.Raycast(characterTop + new Vector2(0.5f, 0f), Vector2.up, _rayLength, wallLayer);
        bool isRightCorner = rightWallHit && rightCeilingHit;

        bool leftWallHit = Physics2D.Raycast(characterTop + new Vector2(-0.4f, 0f), Vector2.left, _rayLength, wallLayer);
        bool leftCeilingHit = Physics2D.Raycast(characterTop + new Vector2(-0.5f, 0f), Vector2.up, _rayLength, wallLayer);
        bool isLeftCorner = leftWallHit && leftCeilingHit;

        Debug.DrawRay(characterTop + new Vector2(0.4f, 0f), Vector2.right * _rayLength, Color.red);
        Debug.DrawRay(characterTop + new Vector2(0.5f, 0f), Vector2.up * _rayLength, Color.red);
        Debug.DrawRay(characterTop + new Vector2(-0.4f, 0f), Vector2.left * _rayLength, Color.blue);
        Debug.DrawRay(characterTop + new Vector2(-0.5f, 0f), Vector2.up * _rayLength, Color.blue);

        if (isLeftCorner && !isRightCorner && !_isOnWall && !_isGrounded)
        {
            _spriteRenderer.flipX = false;
            RedirectAroundCorner();
        }
        else if (isRightCorner && !isLeftCorner && !_isOnWall && !_isGrounded)
        {
            _spriteRenderer.flipX = true;
            RedirectAroundCorner();
        }
    }

    private void RedirectAroundCorner()
    {
        float pushDirection = _spriteRenderer.flipX ? -1f : 1f;

        Vector2 currentPosition = _rb.position;
        Vector2 targetPosition = currentPosition + new Vector2(pushDirection * _offSetUnderCeiling, 0);

        _rb.position = targetPosition;

        _rb.velocity = new Vector2(pushDirection * _cornerPushForce, _jumpForce);
    }

    private void GroundCheck()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, _groundOverlapCheckRadius, groundLayer);

        if (_isGrounded)
        {
            _coyoteTimer = _coyoteTime;
        }
        else
        {
            _coyoteTimer -= Time.deltaTime;
        }
    }

    private void WallCheck()
    {
        Vector2 wallCheckPosition = transform.position + new Vector3(_spriteRenderer.flipX ? -0.5f : 0.5f, 0f, 0f);

        _isOnWall = Physics2D.OverlapCircle(wallCheckPosition, _wallOverlapCheckRadius, wallLayer);

        Debug.DrawLine(wallCheckPosition, wallCheckPosition + Vector2.up, Color.red);
        Debug.DrawLine(wallCheckPosition, wallCheckPosition + new Vector2(_wallOverlapCheckRadius, 0), Color.magenta);
    }

    private void FlipSprite()
    {
        if (_movementX < -0.1f && !_spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = true;
        }
        else if (_movementX > 0.1f && _spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = false;
        }
    }

    private void ManageSwing()
    {
        _swingAttachCooldown = 0.2f;

        if (_hingeJoint == null || _hingeJoint.connectedBody == null)
        {
            return;
        }

        Rigidbody2D swingRb = _hingeJoint.connectedBody;

        if (Input.GetKey(KeyCode.D))
        {
            swingRb.AddTorque(_swingForce);
        }
        if (Input.GetKey(KeyCode.A))
        {
            swingRb.AddTorque(-_swingForce);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ExecuteDetachFromSwing();
        }
    }

    private void ExecuteAttachToSwing(GameObject swingableObject)
    {
        if (_swingAttachCooldown > 0f) return;

        if (_currentState != MovementState.Swinging)
        {
            _currentState = MovementState.Swinging;

            _rb.freezeRotation = true;

            if (_hingeJoint == null)
            {
                _hingeJoint = gameObject.AddComponent<HingeJoint2D>();
            }

            Rigidbody2D swingRb = swingableObject.GetComponent<Rigidbody2D>();

            if (swingRb == null)
            {
                return;
            }

            _hingeJoint.connectedBody = swingRb;

            _hingeJoint.autoConfigureConnectedAnchor = false;

            _hingeJoint.anchor = Vector2.zero;

            Vector2 localGrabPoint = swingRb.transform.InverseTransformPoint(transform.position);
            _hingeJoint.connectedAnchor = localGrabPoint;

            Vector2 playerVelocity = _rb.velocity;
            swingRb.AddForceAtPosition(playerVelocity * _rb.mass * 0.2f, transform.position, ForceMode2D.Impulse);
        }
    }

    private void ExecuteDetachFromSwing()
    {
        if (_currentState == MovementState.Swinging)
        {
            _currentState = MovementState.Jumping;

            Rigidbody2D swingRb = null;

            if (_hingeJoint != null && _hingeJoint.connectedBody != null)
            {
                swingRb = _hingeJoint.connectedBody;
            }

            Vector2 swingVelocity = swingRb.velocity;

            if (_hingeJoint != null)
            {
                Destroy(_hingeJoint);
                _hingeJoint = null;
            }

            if (swingRb != null)
            {
                Collider2D swingCollider = swingRb.GetComponent<Collider2D>();
                Collider2D playerCollider = GetComponent<Collider2D>();

                if (swingCollider != null && playerCollider != null)
                {
                    Physics2D.IgnoreCollision(playerCollider, swingCollider, true);

                    StartCoroutine(ReenableCollision(playerCollider, swingCollider, 0.5f));
                }

            }

            Vector2 baseJumpVelocity = new Vector2(_movementX * _speed * 1.2f, 12f);

            float swingJumpFactor = 1.4f;

            Vector2 detachVelocity = baseJumpVelocity + swingVelocity * swingJumpFactor;

            _rb.isKinematic = false;
            _rb.freezeRotation = true;
            _rb.velocity = detachVelocity;

            JumpDust.Play();
        }
    }

    private IEnumerator ReenableCollision(Collider2D playerCollider, Collider2D swingCollider, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (playerCollider != null && swingCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, swingCollider, false);
        }
    }
}

