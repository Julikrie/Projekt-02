using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI DialogueText;
    public GameObject DialogueWindow;
    public GameObject ChatButton;
    public string[] DialogueLines;

    private int _currentLine;
    [SerializeField]
    private bool _isChatting;
    private bool _inRange = false;

    void Start()
    {
        _isChatting = false;
        DialogueWindow.SetActive(false);
        ChatButton.SetActive(false);
    }

    void Update()
    {
        if (_inRange && Input.GetKeyDown(KeyCode.F))
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _inRange = true;
            ChatButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _inRange = false;
            ChatButton.SetActive(false);
        }
    }
}
