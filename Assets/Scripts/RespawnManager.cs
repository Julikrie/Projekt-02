using System.Collections;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    // Singleton to use as Instance in other scripts
    public static RespawnManager Instance { get; private set; }

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

    // Starts the RespawningObject methode
    public void RespawnObject(GameObject targetObject, float respawnTime)
    {
        StartCoroutine(RespawningObject(targetObject, respawnTime));
    }

    // Respawns Objects (Dash Resetter)
    private IEnumerator RespawningObject(GameObject targetObject, float respawnTime)
    {
        targetObject.SetActive(false); 

        yield return new WaitForSeconds(respawnTime);
        targetObject.SetActive(true);
    }
}
