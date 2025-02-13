using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI DialogueText;
    public TextMeshProUGUI IntroText;
    public GameObject DialogueWindow;
    public GameObject DialogueButton;
    public GameObject ShiftButtonIndicator;
    public string[] DialogueLines;

    private int _currentLine;
    private bool _isChatting = false;
    private bool _inRange = false;
    private SpriteRenderer[] _spriteRenderer;

    void Start()
    {
        CheckForCurrentScene();
        _spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
        DialogueWindow.SetActive(false);
        DialogueButton.SetActive(false);
    }

    void Update()
    {
        // Gets next line when pressing F
        if (_isChatting && Input.GetKeyDown(KeyCode.F))
        {
            NextLine();
        }

        // Starts the dialogue window, when pressing F
        if (_inRange && !_isChatting && Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
            DialogueButton.SetActive(false);
        }

        // Activates the Shift Button on line 2 in Underground section
        if (ShiftButtonIndicator != null)
        {
            if (_currentLine == 2)
            {
                ShiftButtonIndicator.SetActive(true);
            }
            else
            {
                ShiftButtonIndicator.SetActive(false);
            }
        }

        // Destroys the intro text from the Timeline
        Destroy(IntroText, 4.1f);
    }

    // Starts the dialogue
    private void StartDialogue()
    {
        _isChatting = true;
        DialogueWindow.SetActive(true);
        _currentLine = 0;
        DialogueText.text = DialogueLines[_currentLine];
    }

    // Goes to the next line and after last dialogue line closes the window
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

    // If player is in Collider of the Elder, show elder sprite and dialogue button
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _inRange = true;
            DialogueButton.SetActive(true);
            
            foreach (SpriteRenderer sprite in _spriteRenderer)
            {
                sprite.enabled = true;
            }
        }
    }

    // When leaving Collider, let the elder dissapear
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _inRange = false;
            DialogueButton.SetActive(false);
            
            foreach (SpriteRenderer sprite in _spriteRenderer)
            {
                sprite.enabled = false;
            }
        }
    }

    // If the player is in Forest 01, the elder is already active for the intro dialogue, else not active
    private void CheckForCurrentScene()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;

        if (currentScene == 1)
        {
            foreach (SpriteRenderer sprite in _spriteRenderer)
            {
                sprite.enabled = true;
            }
        }
        else if (currentScene == 2 || currentScene == 3)
        {
            foreach (SpriteRenderer sprite in _spriteRenderer)
            {
                sprite.enabled = false;
            }
        }
    }
}
