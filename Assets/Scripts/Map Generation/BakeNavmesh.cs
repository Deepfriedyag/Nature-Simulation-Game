using UnityEngine;
using Unity.AI.Navigation;
using System.CodeDom.Compiler;

// this class bakes the navmesh (required for AI navigation) at runtime. Normally this is not needed but since the game world is procedurally generated, we need this
public class BakeNavmesh : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navmesh_surface;
    [SerializeField] private bool generate_navmesh; // I can set this to true or false in the unity editor

    private void Start() // called once when the script is first loaded
    {
        if (generate_navmesh == true)
        {
            Invoke(nameof(GenerateNavmesh), 0.1f); // call the GenerateNavmesh method after a short delay to ensure the terrain is fully generated first
            Debug.Log("info - navmesh generated successfully");
        }
    }

    public void GenerateNavmesh()
    {
        if (navmesh_surface == null)
        {
            navmesh_surface = GetComponent<NavMeshSurface>();

            if (navmesh_surface == null)
            {
                Debug.LogError("error - no NavMeshSurface component found");
                return;
            }
        }

        navmesh_surface.RemoveData();
        navmesh_surface.BuildNavMesh();

        Debug.Log("info - navmesh baked successfully");
    }
}