using UnityEngine;
using UnityEngine.SceneManagement; // needed to load scenes in Unity (like the main menu or the game world)

public class MenuHandler : MonoBehaviour // inherit from MonoBehaviour, the base class for all unity scripts
{
    [SerializeField] private SaveGameHandler SaveGameHandler; // reference to my class

    // references to the different menu panels in the game
    [SerializeField] private GameObject pause_menu_panel;
    [SerializeField] private GameObject options_menu_panel;
    [SerializeField] private GameObject main_menu_panel;

    private void Start() // reserved Unity method. called once when the script is first loaded
    {
        if (SceneManager.GetActiveScene().name == "Main Menu")
        {
            ShowMainMenu();
        }
        else
        {
            options_menu_panel.SetActive(false);
            pause_menu_panel.SetActive(false);
        }

    }

    private void Update() // reserved Unity method. called every frame
    {
        // check if esc is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowPauseMenu();
        }
    }

    // the methods below are called when certain buttons are pressed in menus therefore they have to be declared public
    public void ShowPauseMenu()
    {
        Time.timeScale = 0; // pause the game
        pause_menu_panel.SetActive(true);
        options_menu_panel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowOptionsMenu()
    {
        pause_menu_panel.SetActive(false);
        options_menu_panel.SetActive(true);
    }

    public void ResumeGame()
    {
        pause_menu_panel.SetActive(false);
        options_menu_panel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1; // resume the game
    }

    // Methods to be called from the main menu
    public void ShowMainMenu()
    {
        main_menu_panel.SetActive(true);
        options_menu_panel.SetActive(false);
    }

    public void SaveAndQuit()
    {
        Time.timeScale = 0;
        SaveGameHandler.SaveGame();
        SceneManager.LoadScene("Main Menu");
    }

    public void NewGame()
    {
        MapGenerator.is_new_game = true; // set the variable in my MapGenerator class to true to generate a random game world
        SceneManager.LoadScene("Game");
        Time.timeScale = 1;
    }

    public void LoadGame()
    {
        MapGenerator.is_new_game = false;
        SceneManager.LoadScene("Game");
        Time.timeScale = 1;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}