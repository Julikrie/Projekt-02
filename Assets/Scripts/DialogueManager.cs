using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI DialogueText;
    public GameObject DialogueWindow;
    public GameObject DialogueButton;
    public string[] DialogueLines;

    private int _currentLine;
    [SerializeField]
    private bool _isChatting =false;
    private bool _inRange = false;

    void Start()
    {
        DialogueWindow.SetActive(false);
        DialogueButton.SetActive(false);
    }

    void Update()
    {

        if (_isChatting && Input.GetKeyDown(KeyCode.F))
        {
            NextLine();
        }
        if (_inRange && !_isChatting && Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
            DialogueButton.SetActive(false);
        }

    }
    private void StartDialogue()
    {
        _isChatting = true;
        DialogueWindow.SetActive(true);

        _currentLine = 0;

        DialogueText.text = DialogueLines[_currentLine];

        EventManager.Instance.SlowTime(1f);
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
            gameObject.SetActive(false);

            EventManager.Instance.SlowTime(0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _inRange = true;
            DialogueButton.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _inRange = false;
            DialogueButton.SetActive(false);
        }
    }
}
