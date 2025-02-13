using UnityEngine;

public class AudioManager : MonoBehaviour

{
    private static AudioManager instance;

    // Does not destroy the Audio Manager on scene change, so that the music does not start from the beginning
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
