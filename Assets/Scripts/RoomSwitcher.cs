using System.Collections;
using UnityEngine;

public class RoomSwitcher : MonoBehaviour
{
    public GameObject CameraTarget;
    public GameObject DeactivatedRoomTrigger;
    public Vector3 TargetNewPosition;
    public float DeactivateTime;

    private void Start()
    {
        CameraTarget.transform.position = new Vector3(25f, 0f, 0f);
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
        DeactivatedRoomTrigger.SetActive(false);

        yield return new WaitForSeconds(DeactivateTime);

        DeactivatedRoomTrigger.SetActive(true);
    }
}

