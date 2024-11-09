using System.Collections.Generic; // import the System.Collections.Generic namespace to use List and HashSet
using UnityEngine;

// this class reuses some of the code from my PlaceVegetation class to spread vegetation in the game world (spawns in new trees, grass etc. as time goes on)
public class SpreadVegetation : MonoBehaviour
{
    public int max_vegetation_count = 5000;
    public float spread_check_interval = 10.0f;
    public float height_offset = 0.5f; // offset to prevent objects from being buried in the terrain
    public float min_height = 2.5f;
    public float max_height = 5f;

    public List<GameObject> vegetation_prefabs; // list of the vegetation prefabs (tree, grass etc.) to spawn in the world
    public MeshFilter terrain_mesh_filter; // reference to the MeshFilter of the terrain mesh in the game world
    public Transform spawned_object_parent; // parent object to hold all the spawned gameobjects

    private Mesh terrain_mesh;

    void Start() // called once when the script is first loaded
    {
        InvokeRepeating(nameof(Spread), spread_check_interval, spread_check_interval); // call the Spread method every <spread_check_interval> seconds
    }

    public void Spread()
    {
        terrain_mesh = terrain_mesh_filter.mesh;
        Vector3[] vertices = terrain_mesh.vertices;
        Vector3[] normals = terrain_mesh.normals; // normals are used to orient the spawned objects correctly

        HashSet<int> selected_indices = new HashSet<int>(); // we check this hashset to prevent spawning on the same vertex

        for (int i = 0; i < vegetation_prefabs.Count; i++) // loop through all the vegetation prefabs
        {
            string prefab_tag = vegetation_prefabs[i].tag;

            if (GameObject.FindGameObjectsWithTag(prefab_tag).Length < max_vegetation_count) // keep spawning vegetation until we reach the max count
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
                    GameObject tree = Instantiate(vegetation_prefabs[random_prefab_index], world_position + normal * height_offset, Quaternion.identity, spawned_object_parent); // spawn a vegetation prefab at the world position as a child object
                    tree.transform.up = normal;
                }

            }

        }

    }

}
