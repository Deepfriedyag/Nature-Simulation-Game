using UnityEngine;

public class CameraMovement : MonoBehaviour // MonoBehaviour is the base class from which every Unity script derives
{
    [SerializeField] private float sensitivity = 10f;
    [SerializeField] private float slow_speed = 1f;
    [SerializeField] private float normal_speed = 5f;
    [SerializeField] private float fast_speed = 50f;

    private float vertical_rotation = 0f; // Track the vertical rotation of the camera
    private float current_speed;

    private void Start() // reserved Unity method. called when the script is first loaded
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() // reserved Unity method. called every frame
    {
        MoveCamera();
        RotateCamera();
    }

    private void MoveCamera() // moves the camera with different speeds based on the player input
    {
        Vector3 player_input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (Input.GetKey(KeyCode.LeftShift))
        {
            current_speed = fast_speed;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            current_speed = slow_speed;
        }
        else
        {
            current_speed = normal_speed;
        }

        transform.Translate(translation: current_speed * Time.deltaTime * player_input); // multiply by the time since the last frame (Time.deltaTime) to make the movement framerate independent

    }

    private void RotateCamera() // rotates the camera based on the mouse input
    {
        Vector2 mouse_input = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        // adjust horizontal rotation
        transform.Rotate(Vector3.up, mouse_input.x * sensitivity * Time.deltaTime);

        // adjust vertical rotation and clamp it
        vertical_rotation -= mouse_input.y * sensitivity * Time.deltaTime;
        vertical_rotation = Mathf.Clamp(vertical_rotation, -90f, 90f); // Prevent looking directly up/down

        // apply vertical rotation while maintaining horizontal rotation
        Quaternion targetRotation = Quaternion.Euler(vertical_rotation, transform.rotation.eulerAngles.y, 0);
        transform.rotation = targetRotation;
    }
}