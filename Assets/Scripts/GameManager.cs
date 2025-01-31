using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void RespawnObject(GameObject targetObject, float respawnTime)
    {
        StartCoroutine(RespawningObject(targetObject, respawnTime));
    }

    private IEnumerator RespawningObject(GameObject targetObject, float respawnTime)
    {
        targetObject.SetActive(false); 

        yield return new WaitForSeconds(respawnTime);

        targetObject.SetActive(true);
    }
}
