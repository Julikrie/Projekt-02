using System.Collections;
using UnityEngine;
using Cinemachine;

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
    public float CornerForce;
    public float CornerCorrectionSide;
    public float CornerCorrectionUp;
    public Vector2 CharacterHead;

    [SerializeField]
    private bool _canTrampoline;

    [SerializeField]
    private float _freezeTime;

    public float ShakeForce;
    public float ShakeForceDestroyable;
    public GameObject DashIndicator;
    public GameObject TrampolinePrefab;
    public LayerMask GroundLayer;
    public LayerMask WallLayer;
    public LayerMask ForbiddenLayer;
    public LayerMask TrampolineLayer;
    public Transform GroundCheckTarget;
    public Transform WallCheckTarget;
    public ParticleSystem JumpDust;
    public ParticleSystem WallSlideDust;
    public ParticleSystem RespawnParticle;

    public bool _isFacingRight;

    [Header("PLAYER STATE")]
    [SerializeField]
    private MovementState _currentState;

    #region Movement

    [Header("MOVE")]
    [SerializeField]
    private float _speed;
    private float _movementX;
    private float _movementY;

    [Header("JUMP")]
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private int _jumpCounterLimit = 2;
    [SerializeField]
    private float _jumpCounter;
    private float _airGravityScale;
    [SerializeField]
    private float _coyoteTime = 0.2f;
    [SerializeField]
    private float _coyoteTimer;
    //private float _jumpBufferTime = 0.25f;
    [SerializeField]
    private float _jumpBufferTimer;

    [Header("WALL JUMP")]
    [SerializeField]
    private Vector2 _wallJumpForce;

    [Header("WALL SLIDE")]
    [SerializeField]
    private float _slideSpeed;

    [Header("DASH")]
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

    [Header("SWING ON OBJECT")]
    [SerializeField]
    private float _detachJumpForce;
    private float _swingAttachCooldown = 0f;
    [SerializeField]
    private float _swingForce;
    [SerializeField]
    private bool _isSwinging;

    [Header("TRAMPOLINE")]
    [SerializeField]
    private float _trampolineForce;

    #endregion Movement

    #region Collision Checks

    [Header("GROUND- AND WALLCHECK")]
    [SerializeField]
    private float _groundCheckRayLength = 0.015f;
    [SerializeField]
    private float _wallCheckRayLength = 0.015f;
    [SerializeField]
    private bool _isOnWall;
    [SerializeField]
    private bool _isGrounded;

    [Header("CORNER CORRECTION")]
    [SerializeField]
    private float _cornerCheckRayLength;
    [SerializeField]
    private float _offSetUnderCeiling;

    [Header("FORBIDDEN SAVE SPOT AREAS")]
    [SerializeField] 
    private float _forbiddenAreaRayLength;
    [SerializeField]
    private Transform _forbiddenAreaTarget;

    #endregion Collision Checks

    #region Saving

    [Header("SAVE MECHANIC")]
    private Vector3 _saveSpot;
    [SerializeField]
    private bool _isInForbiddenArea;

    #endregion Saving

    #region Game Juice

    [Header("SCREEN SHAKE")]
    private CinemachineImpulseSource _impulseSource;

    #endregion Game Juice

    #region Components Player

    [Header("COMPONENTS")]
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private TrailRenderer _trailRenderer;
    private HingeJoint2D _hingeJoint;
    private Animator _animator;

    #endregion Components Player

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _currentState = MovementState.Idling;

        _isFacingRight = true;

        _canDash = true;

        _saveSpot = transform.position;

        _jumpCounter = 0f;
    }

    private void Update()
    {
        _movementX = Input.GetAxis("Horizontal");
        _movementY = Input.GetAxis("Vertical");

        if (_swingAttachCooldown > 0f)
        {
            _swingAttachCooldown -= Time.deltaTime;
        }

        FlipSprite();
        GroundCheck();
        WallCheck();
        ForbiddenSaveAreas();
        SpawnTrampoline();
        CornerCorrection();
        HandleSaveSpot();
        HandleAnimation();

        switch (_currentState)
        {
            case MovementState.Idling:
                HandleIdle();
                break;
            case MovementState.Moving:
                HandleMove();
                break;
            case MovementState.Jumping:
                HandleJump();
                break;
            case MovementState.WallJumping:
                HandleWallJump();
                break;
            case MovementState.WallSliding:
                HandleWallSlide();
                break;
            case MovementState.Dashing:
                HandleDash();
                break;
            case MovementState.Swinging:
                HandleSwing();
                break;
        }
    }

    private void HandleIdle()
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

    private void HandleMove()
    {
        _rb.velocity = new Vector2(_movementX * _speed, _rb.velocity.y);

        if (Mathf.Abs(_movementX) < 0.1f)
        {
            _currentState = MovementState.Idling;
        }

        if (_jumpCounter < _jumpCounterLimit && Input.GetKeyDown(KeyCode.Space))
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

    private void HandleJump()
    {
        _rb.velocity = new Vector2(_movementX * _speed, _rb.velocity.y);

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

        if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash)
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }
    }

    private void ExecuteJump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);

        JumpDust.Play();
        _canTrampoline = true;


        _impulseSource.GenerateImpulse(ShakeForce);

        if (_isGrounded && _coyoteTimer > 0 && _jumpBufferTimer > 0)
        {
            _rb.velocity = new Vector2(_movementX * _speed, _jumpForce);
        }
        _jumpCounter++;
    }

    private void HandleWallSlide()
    {
        if (_isOnWall && !_isGrounded)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, -_slideSpeed);

            WallSlideDust.Play();
        }
        else
        {
            WallSlideDust.Stop();
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

        if (_isOnWall && Mathf.Abs(_rb.velocity.x) >= 0f && Input.GetKeyDown(KeyCode.Space))
        {
            _currentState = MovementState.WallJumping;
            WallSlideDust.Stop();
            ExecuteWallJump();

        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash)
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }
    }

    private void HandleWallJump()
    {
        if (_isGrounded && _rb.velocity.y <= 0f)
        {
            if (Mathf.Abs(_movementX) > 0.01f)
            {
                _currentState = MovementState.Moving;
                _jumpCounter = 0;
            }
            else
            {
                _currentState = MovementState.Idling;
                _jumpCounter = 0;
            }
        }

        if (_isOnWall && !_isGrounded)
        {
            _currentState = MovementState.WallSliding;
        }
    }

    private void ExecuteWallJump()
    {
        if (_isOnWall && !_isGrounded)
        {
            float direction = _isFacingRight ? -1 : 1;

            _rb.velocity = new Vector2(direction * _wallJumpForce.x, _wallJumpForce.y);

            transform.position += new Vector3(direction * 0.2f, 0f, 0f);

            _impulseSource.GenerateImpulseWithForce(ShakeForce);

            JumpDust.Play();

            _isOnWall = false;
        }
    }

    private void HandleDash()
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

        DashIndicator.SetActive(false);

        float originalGravity = _rb.gravityScale;
        _rb.gravityScale = 0f;

        Vector2 dashDirection = Vector2.zero;

        if (dashDirection == Vector2.zero)
        {
            dashDirection = _isFacingRight ? Vector2.right : Vector2.left;
        }

        _rb.velocity = dashDirection * (_dashRange / _dashTime);

        // _trailRenderer.enabled = true;

        yield return new WaitForSeconds(_dashTime);

        //_rb.velocity = Vector2.zero;
        _rb.gravityScale = originalGravity;

        _isDashing = false;
        //_trailRenderer.enabled = false;

        yield return new WaitForSeconds(_dashCooldown);

        _canDash = true;
        DashIndicator.SetActive(true);
    }

    private void SpawnTrampoline()
    {
        if (Input.GetKeyDown(KeyCode.E) && !_isGrounded && _canTrampoline)
        {
            Vector3 spawnPosition = transform.position + new Vector3(0f, -0.5f, 0f);

            GameObject spawnedTrampoline = Instantiate(TrampolinePrefab, spawnPosition, Quaternion.identity);

            _canTrampoline = false;

            _jumpCounterLimit = 2;

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

        if (collision.collider.CompareTag("Danger"))
        {
            // StartCoroutine(FreezeTimeOnCollision(0.1f));
            // RespawnParticle.Play();
            // Invoke("TeleportToSaveSpot", 0.11f);
            TeleportToSaveSpot();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Destroyable") && _isDashing)
        {
            Destroy(other.transform.parent.gameObject);
            _impulseSource.GenerateImpulseWithForce(ShakeForceDestroyable);
            StartCoroutine(FreezeTimeOnCollision(0.05f));
        }

        if (other.gameObject.CompareTag("DashResetter"))
        {
            StopCoroutine(ExecuteDash());

            _isDashing = false;
            _canDash = true;

            other.gameObject.SetActive(false);
            DashIndicator.SetActive(true);
        }
    }

    private IEnumerator FreezeTimeOnCollision(float freezeDuration)
    {
        float gameTime = Time.timeScale;
        Time.timeScale = _freezeTime;

        yield return new WaitForSecondsRealtime(freezeDuration);

        Time.timeScale = gameTime;
    }
    
    private void CornerCorrection()
    {
        Vector2 characterTop = (Vector2)transform.position + CharacterHead;

        bool rightWallHit = Physics2D.Raycast(characterTop + new Vector2(CornerCorrectionSide - 0.1f, 0f), Vector2.right, _cornerCheckRayLength, WallLayer);
        bool rightCeilingHit = Physics2D.Raycast(characterTop + new Vector2(CornerCorrectionUp - 0.1f, 0f), Vector2.up, _cornerCheckRayLength, WallLayer);
        bool isRightCorner = rightWallHit && rightCeilingHit;

        bool leftWallHit = Physics2D.Raycast(characterTop + new Vector2(-CornerCorrectionSide, 0f), Vector2.left, _cornerCheckRayLength, WallLayer);
        bool leftCeilingHit = Physics2D.Raycast(characterTop + new Vector2(-CornerCorrectionUp, 0f), Vector2.up, _cornerCheckRayLength, WallLayer);
        bool isLeftCorner = leftWallHit && leftCeilingHit;

        Debug.DrawRay(characterTop + new Vector2(CornerCorrectionSide -0.1f, 0f), Vector2.right * _cornerCheckRayLength, Color.red);
        Debug.DrawRay(characterTop + new Vector2(CornerCorrectionUp -0.1f, 0f), Vector2.up * _cornerCheckRayLength, Color.red);
        Debug.DrawRay(characterTop + new Vector2(-CornerCorrectionSide, 0f), Vector2.left * _cornerCheckRayLength, Color.blue);
        Debug.DrawRay(characterTop + new Vector2(-CornerCorrectionUp, 0f), Vector2.up * _cornerCheckRayLength, Color.blue);

        if (isLeftCorner && !isRightCorner && !_isOnWall && !_isGrounded && _currentState != MovementState.WallSliding)
        {
            RedirectAroundCorner();
        }
        else if (isRightCorner && !isLeftCorner && !_isOnWall && !_isGrounded && _currentState != MovementState.WallSliding)
        {
            RedirectAroundCorner();
        }
    }
    private void RedirectAroundCorner()
    {
        float pushDirection = _isFacingRight ? 1f : -1f;

        Vector2 currentPosition = _rb.position;
        Vector2 targetPosition = currentPosition + new Vector2(pushDirection * _offSetUnderCeiling, 0);

        _rb.position = targetPosition;

        _rb.velocity = new Vector2(pushDirection, CornerForce);
    }

    private void GroundCheck()
    {
        _isGrounded = Physics2D.Raycast(GroundCheckTarget.position, Vector2.down, _groundCheckRayLength) 
            && !Physics2D.Raycast(GroundCheckTarget.position, Vector2.down, _groundCheckRayLength, TrampolineLayer);

        Debug.DrawRay(GroundCheckTarget.position, Vector2.down * _groundCheckRayLength, Color.blue);

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
        Vector2 direction = transform.localScale.x < 0 ? Vector2.left : Vector2.right;

        _isOnWall = Physics2D.Raycast(WallCheckTarget.position, direction, _wallCheckRayLength, WallLayer);

        Debug.DrawRay(WallCheckTarget.position, direction * _wallCheckRayLength, Color.red);

    }

    // Areas where the player is not allowed to respawn - could be game breaking
    private void ForbiddenSaveAreas()
    {
        _isInForbiddenArea = Physics2D.Raycast(_forbiddenAreaTarget.position, Vector2.down, _forbiddenAreaRayLength, ForbiddenLayer);

        Debug.DrawRay(_forbiddenAreaTarget.position, Vector2.down *_forbiddenAreaRayLength, Color.cyan);
    }

    private void FlipSprite()
    {
        if (_rb.velocity.x < -0.05f && _isFacingRight)
        {
            Flip();
        }
        else if (_rb.velocity.x > 0.05f && !_isFacingRight)
        {
            Flip();
        }
    }
    
    private void Flip()
    {
        _isFacingRight = !_isFacingRight;

        Vector3 flipScale = transform.localScale;
        flipScale.x *= -1;
        transform.localScale = flipScale;
    }

    private void HandleSwing()
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

    private void HandleSaveSpot()
    {
        if (_isGrounded && !_isOnWall && !_isInForbiddenArea)
        {
            _saveSpot = transform.position - new Vector3(0.5f, 0f, 0f);
        }
    }

    private void TeleportToSaveSpot()
    {
        transform.position = _saveSpot;
        _rb.velocity = Vector2.zero;
    }

    private void HandleAnimation()
    {
        _animator.SetBool("isIdling", _currentState == MovementState.Idling);
        _animator.SetBool("isRunning", _currentState == MovementState.Moving);
        _animator.SetBool("isJumping", _currentState == MovementState.Jumping);
        _animator.SetBool("isWallSliding", _currentState == MovementState.WallSliding);
    }
}

