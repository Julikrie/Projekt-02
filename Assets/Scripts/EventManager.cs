using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
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

    private IEnumerator SlowTimeCoroutine (float duration)
    {
        
        float gameTime = Time.timeScale;
        Time.timeScale = 0;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = gameTime;
    }

    public void SlowTime(float duration)
    {
        StartCoroutine(SlowTimeCoroutine(duration));
    }
}
