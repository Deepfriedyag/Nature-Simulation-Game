using UnityEngine;

public class SpriteBillboard : MonoBehaviour // monobehaviour is the base class from which every Unity script derives
{
    private void Start() // reserved Unity method. called when the script is first loaded
    {
        InvokeRepeating(nameof(RepositionSprite), 0.05f, 0.05f); // call RepositionSprite every <0.05> seconds instead of every frame to avoid performance issues
    }

    private void RepositionSprite() // repositions the sprite to face the camera
    {
        Vector3 camera_forward = Camera.main.transform.forward;
        Quaternion rotation = Quaternion.Euler(0f, Quaternion.LookRotation(camera_forward).eulerAngles.y, 0f);
        transform.rotation = rotation;
    }
}