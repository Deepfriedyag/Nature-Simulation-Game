using System.Collections.Generic; // needed to use List and HashSet
using UnityEngine;

public class PlaceVegetation : MonoBehaviour // MonoBehaviour is the base class for all Unity scripts
{
    [SerializeField] private int vegetation_count = 2000;
    [SerializeField] private float height_offset = 0.5f;
    [SerializeField] private float min_height = 2.5f;
    [SerializeField] private float max_height = 5.1f;
    [SerializeField] private List<GameObject> vegetation_prefabs; // contains the vegetation prefabs (tree, grass etc.)
    [SerializeField] private MeshFilter terrain_mesh_filter; // reference to the MeshFilter component of the terrain mesh in the game world
    [SerializeField] private Transform spawned_object_parent; // reference to the parent object to hold all the spawned gameobjects

    private Mesh terrain_mesh;

    public void PlaceObjects() // Places the vegetation objects in the game world. this method is called from my MapGenerator class in the world generation phase
    {
        terrain_mesh = terrain_mesh_filter.mesh;
        Vector3[] vertices = terrain_mesh.vertices;
        Vector3[] normals = terrain_mesh.normals; // normals are used to orient the spawned objects correctly

        HashSet<int> selected_indices = new HashSet<int>(); // we check this hashset to prevent spawning on the same vertex

        for (int i = 0; i < vegetation_count; i++)
        {
            int random_index;

            do
            {
                random_index = Random.Range(0, vertices.Length);
            }
            while (selected_indices.Contains(random_index)); // keep generating random indexes until we find one that hasn't been selected yet

            selected_indices.Add(random_index);
            Vector3 position = vertices[random_index];
            Vector3 normal = normals[random_index];
            Vector3 world_position = terrain_mesh_filter.transform.TransformPoint(position); // convert the local position to world position

            if (world_position.y >= min_height && world_position.y <= max_height)
            {
                int random_prefab_index = Random.Range(0, vegetation_prefabs.Count);
                GameObject tree = Instantiate(vegetation_prefabs[random_prefab_index], world_position + normal * height_offset, Quaternion.identity, spawned_object_parent); // spawn a vegetation prefab at the <world_position> as a child object to <spawned_object_parent>
                tree.transform.up = normal;
            }
        }
    }
}