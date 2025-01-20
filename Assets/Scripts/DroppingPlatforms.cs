using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DroppingPlatforms : MonoBehaviour
{
    public GameObject PlatformCrumble;
    public float ShakeForce = 1f;
    public float DropDelay;
    public float DestroyTime;

    private Rigidbody2D _rb;
    private CinemachineImpulseSource _impulseSource;

    public void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();

        PlatformCrumble.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DroppingPlatform());

            _impulseSource.GenerateImpulseWithForce(ShakeForce);
            PlatformCrumble.SetActive(true);

            Destroy(gameObject, DestroyTime);
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
}
