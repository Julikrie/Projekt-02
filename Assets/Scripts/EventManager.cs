using System.Collections;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    // Singleton to have everywhere the possibility to freeze the playtime
    public static EventManager Instance { get; private set; }
    
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

    // Freezes time for duration
    private IEnumerator FreezeTimeCoroutine(float duration)
    {
        float gameTime = Time.timeScale;
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = gameTime;
    }

    // Method to activate the FreezeTimeCoroutine
    public void FreezeTime(float duration)
    {
        StartCoroutine(FreezeTimeCoroutine(duration));
    }
}
