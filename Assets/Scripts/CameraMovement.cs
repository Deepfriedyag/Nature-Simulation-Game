using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float sensitivity = 100f;
    public float slow_speed, normal_speed, fast_speed; // we can change these values in the unity inspector
    float current_speed;

    protected void Start() // called when the script is first loaded
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    protected void Update() // called every frame
    {
        MoveCamera();
        RotateCamera();
    }

    protected void MoveCamera()
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

        transform.Translate(translation: current_speed * Time.deltaTime * player_input); // move the camera based on the input multiplied by the speed and the time since the last frame

    }

    protected void RotateCamera() // rotates the camera based on the mouse input
    {
        Vector3 mouse_input = new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0);
        transform.Rotate(50 * sensitivity * Time.deltaTime * mouse_input);
        Vector3 euler_rotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(euler_rotation.x, euler_rotation.y, 0);
    }

}
