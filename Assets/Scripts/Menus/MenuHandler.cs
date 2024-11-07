using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandler : MonoBehaviour // inherit from MonoBehaviour, the base class for all unity scripts
{
    [SerializeField] private SaveGameHandler SaveGameHandler; // reference to my class

    // references to each menu panel - I have to set these myself manually in the unity editor because these are "game objects" instead of scripts and unity can't serialize them
    public GameObject pause_menu_panel;
    public GameObject options_menu_panel;
    public GameObject main_menu_panel;

    void Start() // called once when the script is first loaded in the game
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

    void Update() // called every frame
    {
        // check if esc is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowPauseMenu();
        }
    }

    // the methods below are called when certain buttons are pressed in menus
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
        MapGenerator.new_game = true; // set the variable in my class to true to generate a random game world
        SceneManager.LoadScene("Game");
        Time.timeScale = 1;
    }

    public void LoadGame()
    {
        MapGenerator.new_game = false;
        SceneManager.LoadScene("Game");
        Time.timeScale = 1;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}