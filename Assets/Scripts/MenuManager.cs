using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject MenuBackground;
    public GameObject ControlBackground;
    public GameObject ReturnButton;

    [SerializeField]
    private bool _isPaused = false;
    [SerializeField]
    private bool _inGame;

    private void Start()
    {
        CheckForCurrentScene();

        if (_inGame)
        {
            MenuBackground.SetActive(false);
        }
        else
        {
            MenuBackground.SetActive(true);
        }

        ControlBackground.SetActive(false);
    }

    private void Update()
    {
        StopGame();
    }

    private void StopGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isPaused = !_isPaused;

            if (_isPaused)
            {
                Time.timeScale = 0;
                MenuBackground.SetActive(true);
            }
            else
            {
                Time.timeScale = 1;
                MenuBackground.SetActive(false);
            }
        }
    }

    private void CheckForCurrentScene()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 1 || currentScene == 2 || currentScene == 3)
        {
            _inGame = true;
        }
        else
        {
            _inGame = false;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Forest 01");
    }

    public void ShowControls()
    {
        MenuBackground.SetActive(false);
        ControlBackground.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        MenuBackground.SetActive(true);
        ControlBackground.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
