using UnityEngine;

public class RespawnObject : MonoBehaviour
{
    public float RespawnTime;

    // When Colliding with a "Player"-Tagged Object respawn after set RespawnTime
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            RespawnManager.Instance.RespawnObject(gameObject, RespawnTime);
        }
    }
}
