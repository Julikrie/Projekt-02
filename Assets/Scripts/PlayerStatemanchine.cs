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

    [SerializeField]
    private bool _isOnWall;
    [SerializeField]
    private bool _isGrounded;
    [SerializeField]
    private float _coyoteTime;
    [SerializeField]
    private float _coyoteTimer;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _movement;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentState = MovementState.Idling;
    }

    void Update()
    {
        _movement.x = Input.GetAxis("Horizontal");

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
    }

    private void ManageMove()
    {
        _rb.velocity = new Vector2(_movement.x * Speed, _rb.velocity.y);

        FlipSprite();

        if (JumpCounter <= JumpCounterLimit && Mathf.Abs(_movement.x) < 0.1f)
        {
            _currentState = MovementState.Idling;
            _rb.velocity = Vector2.zero;
        }

        if (JumpCounter < JumpCounterLimit && Input.GetKeyDown(KeyCode.Space))
        {
            _currentState = MovementState.Jumping;
            ExecuteJump();
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

        /*
        if (!_isGrounded)
        {
            _coyoteTimer -= Time.deltaTime;
        }

        if (_isGrounded && _coyoteTimer <= 0f)
        {
            _coyoteTimer = _coyoteTime;
        }
        */

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
    }

    private void ExecuteJump()
    {
        _rb.velocity = new Vector2(_movement.x * Speed, JumpForce);
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

    private void GroundCheck()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundOverlapCheckRadius, groundLayer);
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

