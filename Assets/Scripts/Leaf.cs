using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf : MonoBehaviour
{
    public float ForcePower;
    public GameObject Player;
   // public GameObject LeafPrefab;

    private PlayerStateMachine _playerStateMachine;
    private Rigidbody2D _playerRigidbody;

    private void Start()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }

        _playerStateMachine = Player.GetComponent<PlayerStateMachine>();
        _playerRigidbody = Player.GetComponent<Rigidbody2D>();

       // InvokeRepeating("SpawnLeaves", 1f, 2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, 0);
            _playerRigidbody.AddForce(Vector2.up * ForcePower, ForceMode2D.Impulse);

            _playerStateMachine.JumpDust.Play();
        }
    }

    /*
    private void SpawnLeaves()
    {
        Instantiate(LeafPrefab, new Vector3(49f, -5.6f, 0f), Quaternion.identity);
    }
    */
}