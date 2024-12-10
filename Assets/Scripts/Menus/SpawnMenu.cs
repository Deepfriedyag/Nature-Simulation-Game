using UnityEngine;

public class SpawnMenu : MonoBehaviour // MonoBehaviour is the base class from which every Unity script derives
{
    [SerializeField] private GameObject spawn_UI;

    private bool is_UI_open = false;

    private void Start() // reserved Unity method. called once when the script is first loaded
    {
        spawn_UI.SetActive(false);
    }

    private void Update() // reserved Unity method. called every frame
    {
        // Toggle the UI on and off with the E key
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu() // toggle the UI on and off
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

    private void OpenMenu() // open the UI
    {
        Time.timeScale = 0; // pause the game
        spawn_UI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseMenu() // close the UI and set the game speed back to normal (unpause)
    {
        spawn_UI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
            IngameConsole.Instance.LogMessage("Game speed set to 1x");
        }
    }

    public void SetGameTime05X() // set the game speed to 0.5x. has to be declared public so it can be called from a button
    {
        Time.timeScale = 0.5f;
        CloseMenu();
        IngameConsole.Instance.LogMessage("Game speed set to 0.5x");
    }

    public void SetGameTime1X() // set the game speed to 1x (normal speed). has to be declared public so it can be called from a button
    {
        Time.timeScale = 1;
        CloseMenu();
        IngameConsole.Instance.LogMessage("Game speed set to 1x");
    }

    public void SetGameTime2X() // set the game speed to 2x. has to be declared public so it can be called from a button
    {
        Time.timeScale = 2;
        CloseMenu();
        IngameConsole.Instance.LogMessage("Game speed set to 2x");
    }
}