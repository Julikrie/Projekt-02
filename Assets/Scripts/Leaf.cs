using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : MonoBehaviour
{
    public GameObject Player;
    public float ForcePower;

    private Rigidbody2D _playerRigidbody;

    private void Start()
    {
        _playerRigidbody = Player.GetComponent<Rigidbody2D>();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerRigidbody.AddForce(Vector2.up * ForcePower, ForceMode2D.Impulse);
        }
    }
}
