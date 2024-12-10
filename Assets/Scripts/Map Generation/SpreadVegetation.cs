using System.Collections.Generic; // needed to use List and HashSet
using UnityEngine;

public class SpreadVegetation : MonoBehaviour
{
    [SerializeField] private int max_vegetation_count = 5000;
    [SerializeField] private float spread_check_interval = 10f;
    [SerializeField] private float height_offset = 0.5f;
    [SerializeField] private float min_height = 2.5f;
    [SerializeField] private float max_height = 5.1f;
    [SerializeField] private List<GameObject> vegetation_prefabs; // list of the vegetation prefabs (tree, grass etc.)
    [SerializeField] private MeshFilter terrain_mesh_filter; // reference to the MeshFilter of the terrain mesh in the game world
    [SerializeField] private Transform spawned_object_parent; // parent object to hold all the spawned gameobjects

    private Mesh terrain_mesh;

    void Start() // reserved Unity method. called once when the script is first loaded
    {
        InvokeRepeating(nameof(Spread), spread_check_interval, spread_check_interval * Time.timeScale); // call the Spread method every <spread_check_interval> * <Time.timeScale> seconds
    }

    public void Spread() // places vegetation onto the terrain
    {
        terrain_mesh = terrain_mesh_filter.mesh;
        Vector3[] vertices = terrain_mesh.vertices;
        Vector3[] normals = terrain_mesh.normals;

        HashSet<int> selected_indices = new HashSet<int>(); // check this hashset to prevent spawning on the same vertex

        for (int i = 0; i < vegetation_prefabs.Count; i++)
        {
            string prefab_tag = vegetation_prefabs[i].tag;

            if (GameObject.FindGameObjectsWithTag(prefab_tag).Length < max_vegetation_count)
            {
                int random_index;

                do
                {
                    random_index = Random.Range(0, vertices.Length);
                }
                while (selected_indices.Contains(random_index)); // keep generating random indices until we find one that hasn't been selected yet

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
}