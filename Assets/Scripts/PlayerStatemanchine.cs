using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementState
{
    Idle,
    Move,
    Jump,
    WallJump,
    WallSlide,
    Dash
}

public class PlayerStatemanchine : MonoBehaviour
{
    public float Speed;
    public float JumpForce;
    public float SlideSpeed;

    public float groundOverlapCheckRadius;
    public float wallOverlapCheckRadius;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public Transform groundCheck;
    public Transform wallCheck;

    [SerializeField]
    private bool _isJumping;
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
    public MovementState _currentState;
    private Vector2 _movement;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentState = MovementState.Idle;
    }

    void Update()
    {
        _movement.x = Input.GetAxis("Horizontal");

        GroundCheck();
        WallCheck();
        FlipSprite();

        switch (_currentState)
        {
            case MovementState.Idle:
                ExecuteIdle();
                break;
            case MovementState.Move:
                ExecuteMove();
                break;
            case MovementState.Jump:
                ExecuteJump();
                break;
            case MovementState.WallJump:
                break;
            case MovementState.WallSlide:
                ExecuteWallSlide();
                break;
            case MovementState.Dash:
                break;
        }
    }

    private void ExecuteIdle()
    {
        if (_isGrounded && Mathf.Abs(_movement.x) > 0.01f)
        {
            _currentState = MovementState.Move;
        }

        if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            _currentState = MovementState.Jump;
            PerformJump();
        }
    }

    private void ExecuteMove()
    {
        _rb.velocity = new Vector2(_movement.x * Speed, _rb.velocity.y);
        if (_isGrounded && _movement.x < 0.01f)
        {
            _currentState = MovementState.Idle;
        }

        if (_isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            _currentState = MovementState.Jump;
            PerformJump();

        }
    }

    private void ExecuteJump()
    {
        _isJumping = true;
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
            _isJumping = false;
            _currentState = Mathf.Abs(_movement.x) > 0.01f ? MovementState.Move : MovementState.Idle;
        }

        if (_isOnWall && !_isGrounded)
        {
            _isJumping = false;
            _currentState = MovementState.WallSlide;
        }
    }

    private void PerformJump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, JumpForce);
    }

    private void ExecuteWallSlide()
    {
            Debug.Log("Ich WallSlide");
            _rb.velocity = new Vector2(_rb.velocity.x, -SlideSpeed);

        if (_isGrounded)
        {
            _currentState = MovementState.Move;
        }
    }

    private void GroundCheck()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundOverlapCheckRadius, groundLayer);
    }

    void WallCheck()
    {
        Vector2 wallCheckPosition = transform.position + new Vector3(_spriteRenderer.flipX ? -0.5f : 0.5f, 0f, 0f);
        Debug.DrawLine(wallCheckPosition, wallCheckPosition + Vector2.up, Color.red);
        _isOnWall = Physics2D.OverlapCircle(wallCheckPosition, wallOverlapCheckRadius, wallLayer);

        Debug.DrawLine(wallCheckPosition, wallCheckPosition + new Vector2(wallOverlapCheckRadius, 0), Color.green);
    }

    private void FlipSprite()
    {
        if (_movement.x < 0f && !_spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = true;
        }
        else if (_movement.x > 0f && _spriteRenderer.flipX)
        {
            _spriteRenderer.flipX = false;
        }
    }
}

