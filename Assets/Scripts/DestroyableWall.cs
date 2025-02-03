using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableWall : MonoBehaviour
{
    public float CollisionForce;
    public float IgnoreCollisionDuration = 2f;

    private Rigidbody2D[] _rb;
    private PolygonCollider2D[] _collider;
    private PlayerStateMachine _playerStateMachine;
    private Collider2D _playerCollider;


    private void Start()
    {
        _rb = GetComponentsInChildren<Rigidbody2D>();
        _collider = GetComponentsInChildren<PolygonCollider2D>();
        _playerStateMachine = FindObjectOfType<PlayerStateMachine>();
        _playerCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<Collider2D>();

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && _playerStateMachine._isDashing)
        {
            BreakWall();
        }
    }

    private void BreakWall()
    {
        foreach (Rigidbody2D rigidbody in _rb)
        {
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            rigidbody.AddForce(Vector2.right * CollisionForce, ForceMode2D.Impulse);
        }

        StartCoroutine(IgnorePlayerCollisionTemporarily());
        Destroy(gameObject, 2f);
    }

    private IEnumerator IgnorePlayerCollisionTemporarily()
    {
        foreach (PolygonCollider2D collider in _collider)
        {
            Physics2D.IgnoreCollision(_playerCollider, collider, true);
        }

        yield return new WaitForSeconds(IgnoreCollisionDuration);

        foreach (PolygonCollider2D collider in _collider)
        {
            Physics2D.IgnoreCollision(_playerCollider, collider, false);
        }
    }
}

