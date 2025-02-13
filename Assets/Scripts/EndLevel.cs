using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevel : MonoBehaviour
{
    public GameObject EndingBackground;

    private void Start()
    {
        EndingBackground.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(StartEnding());
        }
    }
    
    // Starts the ending screen, when player collides with collider and switches automatically to Main Menu after 3 seconds
    private IEnumerator StartEnding()
    {
        EndingBackground.SetActive(true);

        yield return new WaitForSecondsRealtime(3f);
        EndingBackground.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
}
