using UnityEngine;
using Unity.AI.Navigation;

// this class bakes the navmesh (required for AI navigation) at runtime. Normally this is not needed but since the game world is procedurally generated, we need this
public class BakeNavmesh : MonoBehaviour // MonoBehaviour is the base class from which every Unity script derives
{
    [SerializeField] private NavMeshSurface navmesh_surface;
    [SerializeField] private bool is_generate_navmesh = true; // a toggle for debugging purposes

    private void Start() // reserved Unity method. called once when the script is first loaded    
    {
        if (is_generate_navmesh == true) Invoke(nameof(GenerateNavmesh), 0.1f); // call the GenerateNavmesh() method after a short delay to ensure the terrain is fully generated first
    }

    public void GenerateNavmesh() // this method bakes the navmesh which is required for AI pathfinding
    {
        if (navmesh_surface == null)
        {
            navmesh_surface = GetComponent<NavMeshSurface>();

            if (navmesh_surface == null)
            {
                Debug.LogError("No NavMeshSurface component found");
                return;
            }
        }

        navmesh_surface.RemoveData();
        navmesh_surface.BuildNavMesh();

        Debug.Log("Info - navmesh baked successfully");
    }
}