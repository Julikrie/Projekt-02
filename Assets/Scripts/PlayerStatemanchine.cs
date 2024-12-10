using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementState
{
    Idling,
    Moving,
    Jumping,
    WallJumping,
    WallSliding,
    Dashing
}

public class PlayerStatemanchine : MonoBehaviour
{
    public MovementState _currentState;

    public float Speed;
    public float JumpForce;
    public float JumpStrafe;
    public float SlideSpeed;

    public float JumpCounter;
    public int JumpCounterLimit = 2;

    public Vector2 WallJumpForce;
    public float AirGravityScale;

    public float groundOverlapCheckRadius;
    public float wallOverlapCheckRadius;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public Transform groundCheck;
    public Transform wallCheck;

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
        _jumpBufferTimer = _jumpBufferTime;

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
        if (!_isDashing &&_isGrounded && _rb.velocity.y <= 0f)
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

    private void GroundCheck()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundOverlapCheckRadius, groundLayer);
        
        if (_isGrounded)
        {
            _coyoteTimer = _coyoteTime;
            _jumpBufferTimer = 0;
        }
        else
        {
            _jumpBufferTimer -= Time.deltaTime;
            _coyoteTimer -= Time.deltaTime;
        }
    }

    private void WallCheck()
    {
        Vector2 wallCheckPosition = transform.position + new Vector3(_spriteRenderer.flipX ? -0.5f : 0.5f, 0f, 0f);
        Debug.DrawLine(wallCheckPosition, wallCheckPosition + Vector2.up, Color.red);
        _isOnWall = Physics2D.OverlapCircle(wallCheckPosition, wallOverlapCheckRadius, wallLayer);

        Debug.DrawLine(wallCheckPosition, wallCheckPosition + new Vector2(wallOverlapCheckRadius, 0), Color.green);
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
}

