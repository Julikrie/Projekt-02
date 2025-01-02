using UnityEngine;

public class ShowControlls : MonoBehaviour
{
    public GameObject ControllsKey;
    public GameObject ControllPanel;

    void Start()
    {
        ControllPanel.SetActive(false);
        ControllsKey.SetActive(true);
    }

    void Update()
    {
        ToggleControllPanel();
    }

    private void ToggleControllPanel()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ControllPanel.SetActive(true);
            ControllsKey.SetActive(false);
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            ControllPanel.SetActive(false);
            ControllsKey.SetActive(true);
        }
    }
}
