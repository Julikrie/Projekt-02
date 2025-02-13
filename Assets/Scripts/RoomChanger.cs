using System.Collections;
using UnityEngine;

public class RoomChanger : MonoBehaviour
{
    public GameObject CameraTarget;
    public GameObject Player;
    public float CloseTime = 1f;
    public Vector3 TargetNewPosition;

    private Vector3 _moveCharacter;
    private BoxCollider2D _collider;

    private void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    // Moves the CameraTarget to the next room and pushes the player a little bit in the new room direction
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CameraTarget.transform.position += TargetNewPosition;
            Player.transform.position += _moveCharacter;
            StartCoroutine(ClosingRoom());
        }
    }

    // Closes the room, so that the player does not get stuck in the collider and can't go back
    private IEnumerator ClosingRoom()
    {
        yield return new WaitForSeconds(CloseTime);
        _collider.isTrigger = false;
    }
}


