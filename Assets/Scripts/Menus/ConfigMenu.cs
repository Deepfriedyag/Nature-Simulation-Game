using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // needed to use UI components for the config menu like checkboxes and sliders

public class ConfigMenu : MonoBehaviour // MonoBehaviour is the base class from which every Unity script derives
{
    [SerializeField] private Slider volume;
    [SerializeField] private Dropdown resolution;
    [SerializeField] private Toggle fullscreen;

    private Resolution[] resolutions;

    private void Start() // reserved Unity method. called once when the script is first loaded. this is where we set up the config menu, populating the dropdowns and setting the initial values
    {
        volume.value = PlayerPrefs.GetFloat("volume", 1f); // PlayerPrefs is used to store player data to a local registry between game sessions
        volume.onValueChanged.AddListener(SetVolume);

        fullscreen.isOn = Screen.fullScreen;
        fullscreen.onValueChanged.AddListener(SetFullscreen);

        resolutions = Screen.resolutions;
        resolution.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolution.AddOptions(options);
        resolution.value = currentResolutionIndex;
        resolution.RefreshShownValue();
        resolution.onValueChanged.AddListener(SetResolution);
    }

    // the methods below are called from interactive UI elements in menus therefore they have to be declared public
    public void SetVolume(float volume) // set volume based on slider
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("volume", volume);
    }

    public void SetFullscreen(bool is_full_screen) // set fullscreen mode
    {
        Screen.fullScreen = is_full_screen;
    }

    public void SetResolution(int resolution_index) // set resolution based on dropdown selection
    {
        Resolution resolution = resolutions[resolution_index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}