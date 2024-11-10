using UnityEngine;
using System.Collections.Generic;
using TMPro; // TextMeshPro is a better alternative to the default Unity Text component

public class IngameConsole : MonoBehaviour
{
    public static IngameConsole Instance {get; private set; } // make this class a singleton so I can access it from anywhere

    [SerializeField] private TextMeshProUGUI console_text;
    [SerializeField] private int max_messages = 50;

    private Queue<string> messages = new Queue<string>();

    private void Awake() // awake is called before start
    {
        // singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Optional: keep the console across scenes
        }
        else
        {
            Destroy(gameObject);  // Ensure only one instance exists
        }
    }

    public void LogMessage(string message)
    {
        if (messages.Count >= max_messages)
        {
            messages.Dequeue();
        }

        messages.Enqueue(message);
        UpdateConsole();
    }

    private void UpdateConsole()
    {
        console_text.text = string.Join("\n", messages);
    }

    public void ToggleConsole()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
