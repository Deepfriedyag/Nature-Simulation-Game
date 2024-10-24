using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    private void LateUpdate() // LateUpdate is called after Update each frame. This is to ensure that the object has already been spawned in the scene before running this script
    {
        Vector3 camera_forward = Camera.main.transform.forward;
        Quaternion rotation = Quaternion.Euler(0f, Quaternion.LookRotation(camera_forward).eulerAngles.y, 0f);
        transform.rotation = rotation;
    }

}