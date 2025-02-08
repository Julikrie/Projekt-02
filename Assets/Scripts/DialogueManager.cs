using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI DialogueText;
    public GameObject DialogueWindow;
    public string[] DialogueLines;

    private int _currentLine;
    [SerializeField]
    private bool _isChatting;
    private bool _inRange = false;

    void Start()
    {
        _isChatting = false;
        DialogueWindow.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
        }

        if (_isChatting && Input.GetKeyDown(KeyCode.X))
        {
            NextLine();
        }
    }

    private void StartDialogue()
    {
        _isChatting = true;
        DialogueWindow.SetActive(true);
        _currentLine = 0;
        DialogueText.text = DialogueLines[_currentLine];
    }

    private void NextLine()
    {
        _currentLine++;

        if (_currentLine < DialogueLines.Length)
        {
            DialogueText.text = DialogueLines[_currentLine];
        }
        else
        {
            DialogueWindow.SetActive(false);
            _isChatting = false;
        }
    }
}
