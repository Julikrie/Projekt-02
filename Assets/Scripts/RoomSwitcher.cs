using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomSwitcher : MonoBehaviour
{
    public GameObject Player;
    public GameObject CameraTarget;
    public GameObject CurrentRoomTrigger;
    public Vector3 TargetNewPosition;
    public float DeactivateTime;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CameraTarget.transform.position += TargetNewPosition;
            StartCoroutine(RoomTriggerCooldown());
        }
    }

    private IEnumerator RoomTriggerCooldown()
    {
        CurrentRoomTrigger.SetActive(false);

        yield return new WaitForSeconds(DeactivateTime);

        CurrentRoomTrigger.SetActive(true);
    }
}

