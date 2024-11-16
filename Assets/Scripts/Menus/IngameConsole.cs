using UnityEngine;
using System.Collections.Generic;
using TMPro; // TextMeshPro is a better alternative to the default Unity Text component

public class IngameConsole : MonoBehaviour
{
    public static IngameConsole Instance {get; private set; } // make this class a singleton so I can access it from anywhere in the code. instance is a property that can be accessed from other classes

    [SerializeField] private TextMeshProUGUI console_text; // this is the text component that will display the console messages
    [SerializeField] private int max_messages = 50;

    private Queue<string> messages = new Queue<string>(); // using a circular queue for storing and limiting the number of messages by dequeuing the oldest message

    private void Awake() // awake is called before start
    {
        // singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // keep the console across scenes (such as the main menu and the game map/terrain)
        }
        else
        {
            Destroy(gameObject);  // ensure only one instance exists
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
