using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigMenu : MonoBehaviour
{
    // UI Components
    public Slider volume;
    public Dropdown resolution;
    public Toggle fullscreen;

    // Available Resolutions
    private Resolution[] resolutions;

    void Start()
    {
        // Set volume slider
        volume.value = PlayerPrefs.GetFloat("volume", 1f); // playerprefs is used to store player data between game sessions
        volume.onValueChanged.AddListener(SetVolume);

        // Set fullscreen toggle
        fullscreen.isOn = Screen.fullScreen;
        fullscreen.onValueChanged.AddListener(SetFullscreen);

        // Populate resolution dropdown
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

    // Set volume based on slider
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("volume", volume);
    }

    // Set fullscreen mode
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // Set resolution based on dropdown selection
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
}
