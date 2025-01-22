using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject Player;

    private Vector3 _saveSpot;
    private Rigidbody2D _rb;

    private void Start()
    {
        if (Player != null)
        {
            _rb = Player.GetComponent<Rigidbody2D>();
            _saveSpot = Player.transform.position;
        }
    }

    private void Update()
    {
        HandleSaveSpot();
    }

    private void HandleSaveSpot()
    {
        if (_rb.velocity.y == 0)
        {
            _saveSpot = Player.transform.position;
            _rb.velocity = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Player.transform.position = _saveSpot;
            _rb.velocity = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Danger"))
        {
            Player.transform.position = _saveSpot;
            _rb.velocity = Vector2.zero;
        }
    }
}
