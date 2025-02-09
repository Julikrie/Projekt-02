using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject MenuBackground;
    public GameObject ControlBackground;
    public GameObject ReturnButton;

    private void Start()
    {
        ControlBackground.SetActive(false);
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
