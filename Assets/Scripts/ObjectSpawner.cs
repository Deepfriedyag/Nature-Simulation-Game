using UnityEngine;
using UnityEngine.AI; // required for using Unity's built-in navigation system to detect where the objects can be spawned

public class ObjectSpawner : MonoBehaviour // MonoBehaviour is the base class from which every Unity script derives
{
    [SerializeField] private GameObject[] spawnable_objects;
    [SerializeField] private Transform player_spawned_objects_parent;
    [SerializeField] private float height_offset = -0.5f;
    [SerializeField] private float max_navmesh_distance = 100.0f;

    private GameObject selected_object;

    private void Update() // reserved Unity method. called once per frame
    {
        if (Input.GetMouseButtonDown(0) && selected_object != null && Time.timeScale != 0) // check if the left mouse button is pressed and an object is selected from the menu
        {
            SpawnObject();
        }
    }

    public void SelectObject(int index) // used to detect which object is selected from the menu. has to be declared public because this is called from a button in the menu
    {
        if (index >= 0 && index < spawnable_objects.Length)
        {
            selected_object = spawnable_objects[index];
            IngameConsole.Instance.LogMessage($"Selected object: {selected_object.name}");
        }
        else
        {
            Debug.LogError("Index out of range or invalid."); // log an error message to Unity's console
        }
    }

    private void SpawnObject() // spawns the selected object at the center of the screen on a suitable position on the NavMesh
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        if (NavMesh.SamplePosition(ray.origin + ray.direction * 10f, out NavMeshHit nav_hit, max_navmesh_distance, NavMesh.AllAreas))
        {
            Vector3 spawn_position = nav_hit.position + Vector3.up * height_offset;
            Instantiate(selected_object, spawn_position, Quaternion.identity, player_spawned_objects_parent);
            IngameConsole.Instance.LogMessage($"Spawned {selected_object.name}");
        }
    }
}