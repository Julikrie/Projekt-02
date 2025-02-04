using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BouncyLeaves : MonoBehaviour
{
    public float ForcePower;
    public float MoveDirection;
    public float Speed;
    public GameObject Player;
    public AudioClip BounceSound;

    private PlayerStateMachine _playerStateMachine;
    private Rigidbody2D _playerRigidbody;
    private AudioSource _audioSource;

    private void Start()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }

        _playerStateMachine = Player.GetComponent<PlayerStateMachine>();
        _playerRigidbody = Player.GetComponent<Rigidbody2D>();
        _audioSource = Player.GetComponent<AudioSource>();
    }

    private void Update()
    {
        transform.position += new Vector3(0f, MoveDirection, 0f) * Speed * Time.deltaTime;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _playerRigidbody.velocity = new Vector2(_playerRigidbody.velocity.x, 0);
            _playerRigidbody.AddForce(Vector2.up * ForcePower, ForceMode2D.Impulse);

            _playerStateMachine.JumpDust.Play();
            _audioSource.PlayOneShot(BounceSound, 0.3f);
        }
    }
}