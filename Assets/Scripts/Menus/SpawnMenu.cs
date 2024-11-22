using UnityEngine;

public class SpawnMenu : MonoBehaviour
{
    [SerializeField] private GameObject spawn_UI;
    private bool is_UI_open = false;

    private void Start() // Start is called before the first frame update
    {
        spawn_UI.SetActive(false); // hide the UI initially
    }

    private void Update() // Update is called once per frame
    {
        // Toggle the UI on and off with the E key
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        is_UI_open = !is_UI_open;

        if (is_UI_open)
        {
            OpenMenu();
        }
        else
        {
            CloseMenu();
        }
    }

    private void OpenMenu()
    {
        Time.timeScale = 0; // pause the game
        spawn_UI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseMenu()
    {
        spawn_UI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            IngameConsole.Instance.LogMessage("Time set to 1x");
        }
    }

    public void SetGameTime05X()
    {
        Time.timeScale = 0.5f;
        CloseMenu();
        IngameConsole.Instance.LogMessage("Time set to 0.5x");
    }

    public void SetGameTime1X()
    {
        Time.timeScale = 1;
        CloseMenu();
        IngameConsole.Instance.LogMessage("Time set to 1x");
    }

    public void SetGameTime2X()
    {
        Time.timeScale = 2;
        CloseMenu();
        IngameConsole.Instance.LogMessage("Time set to 2x");
    }

}