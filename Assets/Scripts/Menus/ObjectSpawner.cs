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
        if (selected_object != null && Input.GetMouseButtonDown(0))
        {
            SpawnObject();
        }
    }

    public void SelectObject(int index)
    {
        if (index >= 0 && index < spawnable_objects.Length)
        {
            selected_object = spawnable_objects[index];
            Debug.Log("Info - Selected object: " + selected_object.name);
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
        if (NavMesh.SamplePosition(ray.origin + ray.direction * 10f, out NavMeshHit navHit, max_navmesh_distance, NavMesh.AllAreas))
        {
            Vector3 spawnPosition = navHit.position + Vector3.up * height_offset;
            GameObject spawnedObject = Instantiate(selected_object, spawnPosition, Quaternion.identity, player_spawned_objects_parent);
        }

    }

}
