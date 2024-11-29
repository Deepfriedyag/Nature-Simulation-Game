using UnityEngine;
using UnityEngine.AI;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] spawnable_objects;
    [SerializeField] private Transform player_spawned_objects_parent;
    private GameObject selected_object;
    [SerializeField] private float height_offset = -0.5f;
    [SerializeField] private float max_navmesh_distance = 100.0f; // Max horizontal distance to search for NavMesh

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && selected_object != null && Time.timeScale != 0)
        {
            SpawnObject();
        }
    }

    public void SelectObject(int index)
    {
        if (index >= 0 && index < spawnable_objects.Length)
        {
            selected_object = spawnable_objects[index];
            IngameConsole.Instance.LogMessage($"Selected object: {selected_object.name}");
        }
        else
        {
            Debug.Log("Error - Index out of range or invalid.");
        }
    }

    public void SpawnObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

        // Only sample a position on the NavMesh
        if (NavMesh.SamplePosition(ray.origin + ray.direction * 10f, out NavMeshHit nav_hit, max_navmesh_distance, NavMesh.AllAreas))
        {
            Vector3 spawn_position = nav_hit.position + Vector3.up * height_offset;
            GameObject spawned_object = Instantiate(selected_object, spawn_position, Quaternion.identity, player_spawned_objects_parent);
            IngameConsole.Instance.LogMessage($"Spawned {selected_object.name}");
        }

    }

}
