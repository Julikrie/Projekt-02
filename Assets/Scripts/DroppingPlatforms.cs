using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DroppingPlatforms : MonoBehaviour
{
    public GameObject PlatformCrumble;
    public float ShakeForce = 1f;
    public float DropDelay;
    public float RespawnTime;

    private Rigidbody2D _rb;
    private Vector3 _originalPosition;
    private CinemachineImpulseSource _impulseSource;

    public void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _originalPosition = transform.position;
        _impulseSource = GetComponent<CinemachineImpulseSource>();

        PlatformCrumble.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DroppingPlatform());
            StartCoroutine(RespawnPlatform());

            _impulseSource.GenerateImpulseWithForce(ShakeForce);

            PlatformCrumble.SetActive(true);
        }
        else
        {
            PlatformCrumble.SetActive(false);
        }
    }

    private IEnumerator DroppingPlatform()
    {
        yield return new WaitForSeconds(DropDelay);

        _rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private IEnumerator RespawnPlatform()
    {
        yield return new WaitForSeconds(RespawnTime);

        transform.position = _originalPosition;

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.velocity = Vector3.zero;
    }
}
