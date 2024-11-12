using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DroppingPlatforms : MonoBehaviour
{
    public float dropDelay;

    private Rigidbody2D rb;
    public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DroppingPlatform());
            Destroy(gameObject, 1.25f);
        }
    }

    private IEnumerator DroppingPlatform()
    {
        yield return new WaitForSeconds(dropDelay);
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
