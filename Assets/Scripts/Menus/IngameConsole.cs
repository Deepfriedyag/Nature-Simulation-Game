using UnityEngine;
using TMPro; // TextMeshPro is a better alternative to the default Unity text rendering system
using System.Collections.Generic;

public class IngameConsole : MonoBehaviour // Monobehaviour is the base class from which every Unity script derives
{
    public static IngameConsole Instance {get; private set; } // make this class a singleton so I can access it from anywhere in the same assembly. instance is a property that can be accessed from other classes

    [SerializeField] private TextMeshProUGUI console_text;
    [SerializeField] private int max_messages = 18;

    private Queue<string> messages = new Queue<string>(); // using a circular queue for storing and limiting the number of messages by dequeuing the oldest message

    private void Awake() // reserved Unity method that is called when the script is first loaded and before the Start() method
    {
        // singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // ensure only one instance exists
        }
    }

    public void LogMessage(string message) // log the specified <message> to the console
    {
        if (messages.Count >= max_messages)
        {
            messages.Dequeue();
        }

        messages.Enqueue(message);
        UpdateConsole();
    }

    private void UpdateConsole() // update the console text with the messages
    {
        console_text.text = string.Join("\n", messages);
    }

    private void ToggleConsole() // toggle the console on and off
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
