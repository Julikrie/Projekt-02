using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableWall : MonoBehaviour
{
    public float CollisionForce;
    public float IgnoreCollisionDuration = 2f;
    public float ShakeIntensity;

    private Rigidbody2D[] _rb;
    private PolygonCollider2D[] _polygonCollider;
    private BoxCollider2D _boxCollider;
    private Collider2D _playerCollider;
    private PlayerStateMachine _playerStateMachine;
    private CinemachineImpulseSource _impulseSource;

    private void Start()
    {
        _rb = GetComponentsInChildren<Rigidbody2D>();
        _polygonCollider = GetComponentsInChildren<PolygonCollider2D>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _playerStateMachine = FindObjectOfType<PlayerStateMachine>();
        _playerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && _playerStateMachine._isDashing)
        {
            BreakWall();
            _impulseSource.GenerateImpulse(ShakeIntensity);
            EventManager.Instance.SlowTime(0.02f);

            _boxCollider.enabled = false;
        }
    }

    private void BreakWall()
    {
        foreach (Rigidbody2D rigidbody in _rb)
        {
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            rigidbody.AddForce(Vector2.right * CollisionForce, ForceMode2D.Impulse);
        }

        foreach (PolygonCollider2D collider in _polygonCollider)
        {
            Physics2D.IgnoreCollision(_playerCollider, collider, true);
        }
    }
}


