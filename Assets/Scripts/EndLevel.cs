using System.Collections;
using System.Collections.Generic;
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

    private IEnumerator StartEnding()
    {
        EndingBackground.SetActive(true);

        yield return new WaitForSeconds(3f);

        EndingBackground.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
}
