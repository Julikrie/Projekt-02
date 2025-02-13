using System.Collections;
using UnityEngine;
using Cinemachine;


// Player States
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
    public AudioClip[] FootStepSound;      
    public AudioClip CollectSound;
    public AudioClip JumpSound;
    public AudioClip TrampolineSound;
    public AudioClip DashSound;
    public AudioClip DamageSound;
    public float ShakeIntensity;
    public float ShakeTrampoline;
    public float ShakeDash;
    public float CornerForce;
    public float CornerCorrectionSide;
    public float CornerCorrectionUp;
    public Vector2 CharacterHead;
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
    public TrailRenderer DashTrail;
    public SpriteRenderer[] SpriteRenderer;

    private AudioSource _audioSource;
    private bool _unlockedDashing = false;
    private bool _unlockedSwinging = false;
    private bool _canTrampoline;
    private bool _isFacingRight;

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
    public bool _isDashing;
    [SerializeField]
    private float _dashTime;
    [SerializeField]
    private float _dashCooldown;
    [SerializeField]
    private float _dashRange;
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
    private HingeJoint2D _hingeJoint;
    private Animator _animator;

    #endregion Components Player

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _audioSource = GetComponent<AudioSource>();
        _currentState = MovementState.Idling;
        _isFacingRight = true;
        _canDash = true;
        _saveSpot = transform.position;
        DashIndicator.SetActive(false);
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

    // Handles the Idle State
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

        if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash && _unlockedDashing)
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }
    }

    // Handles the Movement State
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

        if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash && _unlockedDashing)
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }

        if (_isOnWall && _rb.velocity.y < 0)
        {
            _currentState = MovementState.WallSliding;
        }
    }

    // Handles the Jump State and Phase
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
                JumpDust.Play();
                _impulseSource.GenerateImpulse(ShakeIntensity);
            }
            else
            {
                _currentState = MovementState.Idling;
                JumpDust.Play();
                _impulseSource.GenerateImpulse(ShakeIntensity);
            }
            _jumpCounter = 0;
        }

        if (_isOnWall && !_isGrounded)
        {
            _currentState = MovementState.WallSliding;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash && _unlockedDashing)
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }
    }

    // Handles the Jump off
    private void ExecuteJump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
        JumpDust.Play();
        _audioSource.PlayOneShot(JumpSound, 0.4f);

        if (_jumpCounter < 1)
        {
            _canTrampoline = true;
        }

        _impulseSource.GenerateImpulse(ShakeIntensity);

        if (_isGrounded && _coyoteTimer > 0 && _jumpBufferTimer > 0)
        {
            _rb.velocity = new Vector2(_movementX * _speed, _jumpForce);
        }
        _jumpCounter++;
    }

    // Handles the Wall Slide State
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
                JumpDust.Play();
            }
            else
            {
                _currentState = MovementState.Idling;
                JumpDust.Play();
            }
            _jumpCounter = 0;
        }

        if (_isOnWall && Mathf.Abs(_rb.velocity.x) >= 0f && Input.GetKeyDown(KeyCode.Space))
        {
            _currentState = MovementState.WallJumping;
            WallSlideDust.Stop();
            ExecuteWallJump();

        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && _canDash && _unlockedDashing)
        {
            StartCoroutine(ExecuteDash());
            _currentState = MovementState.Dashing;
        }
    }

    // Handles the Wall Jump State
    private void HandleWallJump()
    {
        if (_isGrounded && _rb.velocity.y <= 0f)
        {
            if (Mathf.Abs(_movementX) > 0.01f)
            {
                _currentState = MovementState.Moving;
                JumpDust.Play();
                _impulseSource.GenerateImpulse(ShakeIntensity);
                _jumpCounter = 0;
            }
            else
            {
                _currentState = MovementState.Idling;
                JumpDust.Play();
                _impulseSource.GenerateImpulse(ShakeIntensity);
                _jumpCounter = 0;
            }
        }

        if (_isOnWall && !_isGrounded)
        {
            _currentState = MovementState.WallSliding;
        }
    }

    // Looks if the player is on a Wall and pushes him off (Jump) the Wall in the other direction
    private void ExecuteWallJump()
    {
        if (_isOnWall && !_isGrounded)
        {
            float direction = _isFacingRight ? -1 : 1;
            _rb.velocity = new Vector2(direction * _wallJumpForce.x, _wallJumpForce.y);
            transform.position += new Vector3(direction * 0.2f, 0f, 0f);
            _impulseSource.GenerateImpulse(ShakeIntensity);
            JumpDust.Play();
            _audioSource.PlayOneShot(JumpSound, 0.4f);
            _isOnWall = false;
        }
    }

    // Handles the Dash State
    private void HandleDash()
    {
        if (!_isDashing && _isGrounded && _rb.velocity.y <= 0f)
        {
            if (Mathf.Abs(_movementX) > 0.01f)
            {
                _currentState = MovementState.Moving;
                JumpDust.Play();
            }
            else
            {
                _currentState = MovementState.Idling;
                JumpDust.Play();
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

    // Starts the Dash and manages the time and cooldown
    private IEnumerator ExecuteDash()
    {
        if (_unlockedDashing)
        {
            _isDashing = true;
            _canDash = false;
            _audioSource.PlayOneShot(DashSound, 0.5f);
            DashIndicator.SetActive(false);
            DashTrail.emitting = true;
            _impulseSource.GenerateImpulseWithForce(ShakeDash);

            float originalGravity = _rb.gravityScale;
            _rb.gravityScale = 0f;

            Vector2 dashDirection = Vector2.zero;

            if (dashDirection == Vector2.zero)
            {
                dashDirection = _isFacingRight ? Vector2.right : Vector2.left;
            }

            _rb.velocity = dashDirection * (_dashRange / _dashTime);
            EventManager.Instance.FreezeTime(0.02f);

            yield return new WaitForSecondsRealtime(_dashTime);
            _rb.gravityScale = originalGravity;
            _isDashing = false;
            DashTrail.emitting = false;

            yield return new WaitForSecondsRealtime(_dashCooldown);
            _canDash = true;
            DashIndicator.SetActive(true);
        }
    }

    // When pressing E spawns a trampoline beneath the player
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Gives the Player a bounce upwards when colliding with the Trampoline
        if (collision.gameObject.CompareTag("Trampoline"))
        {
            _rb.velocity = new Vector2(_movementX, 0);
            _rb.AddForce(Vector2.up * _trampolineForce, ForceMode2D.Impulse);
            _impulseSource.GenerateImpulseWithForce(ShakeTrampoline);
            _audioSource.PlayOneShot(TrampolineSound, 0.3f);
        }

        // Attaches the player to the Vines
        if (collision.gameObject.CompareTag("SwingObject"))
        {
            ExecuteAttachToSwing(collision.gameObject);
        }

        // Starts the Death, Teleport to save area coroutines when colliding with objects tagged as "Danger"
        if (collision.gameObject.CompareTag("Danger"))
        {
            StartCoroutine(DissolvePlayer(0.11f));
            StartCoroutine(TeleportToSaveSpot(0.11f));
            _audioSource.PlayOneShot(DamageSound, 0.4f);
            RespawnParticle.Play();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // When the player collides with a Dash Resetter (green circle), his gravity gets hard set to 6, so that the player does not stop in the dash coroutine, while his gravity is zero and floats afterwards and resets the dash cooldown
        if (other.gameObject.CompareTag("DashResetter"))
        {
            _audioSource.PlayOneShot(CollectSound, 0.35f);
            StopCoroutine(ExecuteDash());
            _rb.gravityScale = 6f;
            _isDashing = false;
            _canDash = true;
            DashIndicator.SetActive(true);
        }

        // After pickup the player can Dash
        if (other.gameObject.CompareTag("DashItem"))
        {
            _audioSource.PlayOneShot(CollectSound, 0.35f);
            DashIndicator.SetActive(true);
            _unlockedDashing = true;
            Destroy(other.gameObject);
        }

        // Afterp pickup the player can Swing
        if (other.gameObject.CompareTag("SwingItem"))
        {
            _audioSource.PlayOneShot(CollectSound, 0.35f);
            _unlockedSwinging = true;
            Destroy(other.gameObject);
        }
    }
    
    // Raycast in formation of a triangle are looking for corners and push the player in the direction of the triggered check
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

        if (isLeftCorner && !isRightCorner && !_isOnWall && !_isGrounded && _currentState != MovementState.WallSliding && _rb.velocity.y < 0)
        {
            RedirectAroundCorner();
            Debug.Log("Corner Correction left");
        }
        else if (isRightCorner && !isLeftCorner && !_isOnWall && !_isGrounded && _currentState != MovementState.WallSliding && _rb.velocity.y < 0)
        {
            RedirectAroundCorner();
            Debug.Log("Corner Correction right");
        }
    }

    // Pushes the player around the corner
    private void RedirectAroundCorner()
    {
        float pushDirection = _isFacingRight ? 1f : -1f;
        Vector2 currentPosition = _rb.position;
        Vector2 targetPosition = currentPosition + new Vector2(pushDirection * _offSetUnderCeiling, 0);
        _rb.position = targetPosition;
        _rb.velocity = new Vector2(pushDirection, CornerForce);
    }

    // Checks if the player is standing on ground and manages the coyote time
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

    // Checks if the player is standing next to a wall and the direciton of the wall
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

    // Flips the sprite but not while the player is swinging on a Vine
    private void FlipSprite()
    {
        if (_currentState == MovementState.Swinging)
        {
            return;
        }

        if (_rb.velocity.x < -0.05f && _isFacingRight)
        {
            Flip();
        }
        else if (_rb.velocity.x > 0.05f && !_isFacingRight)
        {
            Flip();
        }
    }
    
    // Flips the player with locoalscale 1;-1
    private void Flip()
    {
        _isFacingRight = !_isFacingRight;
        Vector3 flipScale = transform.localScale;
        flipScale.x *= -1;
        transform.localScale = flipScale;

        if(_isGrounded)
        {
            JumpDust.Play();
        }
    }

    // Handles the Swing State and the left and right swinging
    private void HandleSwing()
    {
        _swingAttachCooldown = 0.2f;

        if (_unlockedSwinging)
        {
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
    }


    // Adds a HingeJoint to the player so that he can attach to the vine and add force when jumping against it
    private void ExecuteAttachToSwing(GameObject swingableObject)
    {
        if (_unlockedSwinging)
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
    }

    // Detaches the player from the Vine and destroyes the HingeJoint, also it ignores the collider of the vine for some time, so that the player does not reattach immediately
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

                    StartCoroutine(ReenableCollision(playerCollider, swingCollider, 1.2f));
                }
            }

            Vector2 baseJumpVelocity = new Vector2(_movementX * _speed * 1.2f, 12f);
            float swingJumpFactor = 1.4f;
            Vector2 detachVelocity = baseJumpVelocity + swingVelocity * swingJumpFactor;
            _rb.isKinematic = false;
            _rb.freezeRotation = true;
            _rb.velocity = detachVelocity;
            JumpDust.Play();
            _audioSource.PlayOneShot(JumpSound, 0.4f);
        }
    }

    // Ignores the Collision of PlayerCollider and SwingCollider (Player and Vines)
    private IEnumerator ReenableCollision(Collider2D playerCollider, Collider2D swingCollider, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (playerCollider != null && swingCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, swingCollider, false);
        }
    }

    // Moves the player a little bit to the opposite direction he faced while dying, so that the player does not fall after respawn
    private void HandleSaveSpot()
    {
        if (_isGrounded && !_isOnWall && !_isInForbiddenArea)
        {
            if (_isFacingRight)
            {
                _saveSpot = transform.position - new Vector3(0.5f, 0f, 0f);
            }
            else
            {
                _saveSpot = transform.position + new Vector3(0.5f, 0f, 0f);
            }
        }
    }

    // Teleports the player to his last save spot after some delay
    private IEnumerator TeleportToSaveSpot(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        transform.position = _saveSpot;
        _rb.velocity = Vector2.zero;
    }

    // Used in the Animation Running as Event to play footstep sounds
    private void PlayFootsteps()
    {
        if (FootStepSound.Length > 0)
        {
            int footstepIndex = Random.Range(0, FootStepSound.Length);
            _audioSource.PlayOneShot(FootStepSound[footstepIndex], 0.15f);
        }
    }

    // Dissolves the player when dying and activates him again 
    private IEnumerator DissolvePlayer(float delay)
    {
        foreach (SpriteRenderer bodyparts in SpriteRenderer)
        {
            bodyparts.enabled = false;
        }

        yield return new WaitForSecondsRealtime(delay);

        foreach (SpriteRenderer bodyparts in SpriteRenderer)
        {
            bodyparts.enabled = true;
        }
    }

    // Handles the Animation States with the help of the Movement States
    private void HandleAnimation()
    {
        _animator.SetBool("isIdling", _currentState == MovementState.Idling);
        _animator.SetBool("isMoving", _currentState == MovementState.Moving);
        _animator.SetBool("isJumping", _currentState == MovementState.Jumping);
        _animator.SetBool("isWallSliding", _currentState == MovementState.WallSliding || _currentState == MovementState.Swinging);
    }
}

