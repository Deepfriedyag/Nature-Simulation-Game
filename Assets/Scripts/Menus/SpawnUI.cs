using UnityEngine;

public class SpawnUI : MonoBehaviour
{
    [SerializeField] private GameObject spawn_ui;
    private bool is_ui_open = false;

    private void Start() // Start is called before the first frame update
    {
        spawn_ui.SetActive(false); // hide the UI initially
    }

    private void Update() // Update is called once per frame
    {
        // Toggle the UI on and off with the E key
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleUI();
        }
    }

    public void ToggleUI()
    {
        is_ui_open = !is_ui_open;

        if (is_ui_open)
        {
            OpenUI();
        }
        else
        {
            CloseUI();
        }
    }

    public void OpenUI()
    {
        Time.timeScale = 0; // pause the game
        spawn_ui.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseUI()
    {
        Time.timeScale = 1; // unpause the game
        spawn_ui.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}
