using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.Tilemaps;

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
    private SpriteRenderer _spriteRenderer;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        DialogueWindow.SetActive(false);
        DialogueButton.SetActive(false);
        _spriteRenderer.enabled = false;
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
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _inRange = true;
            DialogueButton.SetActive(true);
            _spriteRenderer.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _inRange = false;
            DialogueButton.SetActive(false);
            _spriteRenderer.enabled = false;
        }
    }

}
