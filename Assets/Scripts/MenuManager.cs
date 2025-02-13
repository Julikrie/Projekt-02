using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject MenuBackground;
    public GameObject ControlBackground;
    public GameObject ReturnButton;

    private bool _isPaused = false;
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

    // In Game 

    // Stops game when pressing ESC and opening the Pause Menu
    private void StopGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _inGame)
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
                ControlBackground.SetActive(false);
            }
        }
    }

    // Checks if the player is in Scene 1-3, and sets _ingame true. This seperates the in game menu and main menu
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

    // Goes to Main Menu and sets playtime normal
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }

    // Main Menu

    // Goes to the Start of the Game
    public void StartGame()
    {
        SceneManager.LoadScene("Forest 01");
        Time.timeScale = 1f;
    }

    // Switches to Controls overlay in Main Menu
    public void ShowControls()
    {
        MenuBackground.SetActive(false);
        ControlBackground.SetActive(true);
    }

    // Returns from Controls to Main Menu overlay
    public void ReturnToMenu()
    {
        MenuBackground.SetActive(true);
        ControlBackground.SetActive(false);
    }

    // Closes the game
    public void QuitGame()
    {
        Application.Quit();
    }
}
