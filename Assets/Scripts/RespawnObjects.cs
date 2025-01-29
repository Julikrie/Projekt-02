using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnObjects : MonoBehaviour
{
    public float RespawnTime;
    private Vector3 _originalPosition;

    private void Start()
    {
        _originalPosition = transform.position;
    }

    private void Update()
    {
        StartCoroutine(RespawnPlatform());
    }

    private IEnumerator RespawnPlatform()
    {
        yield return new WaitForSeconds(RespawnTime);

        transform.position = _originalPosition;

        gameObject.SetActive(true);
    }
}
