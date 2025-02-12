using System.Collections;
using UnityEngine;

public class RoomChanger : MonoBehaviour
{
    public GameObject CameraTarget;
    public GameObject Player;
    public Vector3 TargetNewPosition;
    public float CloseTime = 1f;

    [SerializeField]
    private Vector3 _moveCharacter;
    private BoxCollider2D _collider;


    private void Start()
    {
        _collider = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CameraTarget.transform.position += TargetNewPosition;

            Player.transform.position += _moveCharacter;

            StartCoroutine(ClosingRoom());
        }
    }

    private IEnumerator ClosingRoom()
    {
        yield return new WaitForSeconds(CloseTime);

        _collider.isTrigger = false;
    }
}


