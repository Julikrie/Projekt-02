using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    private float _freezeTime;
    private CinemachineImpulseSource _impulseSource;

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
    private void Start()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public IEnumerator GameTimeChanger (float duration)
    {
        float gameTime = Time.timeScale;
        Time.timeScale = _freezeTime;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = gameTime;
    }

    public void CameraShake(float shakeIntensity)
    {
        _impulseSource.GenerateImpulseWithForce(shakeIntensity);
    }
}
