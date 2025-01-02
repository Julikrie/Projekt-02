using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spike : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Player;
    public Vector2 _saveLocationRoom;


    [SerializeField]

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Player"))
        {
            Player.transform.position = new Vector2(_saveLocationRoom.x, _saveLocationRoom.y);
        }
    }
}
