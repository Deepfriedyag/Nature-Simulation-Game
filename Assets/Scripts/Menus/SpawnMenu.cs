using UnityEngine;

public class SpawnMenu : MonoBehaviour
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
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        is_ui_open = !is_ui_open;

        if (is_ui_open)
        {
            OpenMenu();
        }
        else
        {
            CloseMenu();
        }
    }

    public void OpenMenu()
    {
        Time.timeScale = 0; // pause the game
        spawn_ui.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseMenu()
    {
        Time.timeScale = 1; // unpause the game
        spawn_ui.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ADD SPEEDING UP GAMETIME HERE []

}
