using UnityEngine;

public class MapDisplay : MonoBehaviour // MonoBehaviour is the base class from which every Unity script derives
{

    [SerializeField] private Renderer texture_render; // renderer component to display the texture
    [SerializeField] private MeshFilter mesh_filter; // meshFilter component to hold the mesh data
    [SerializeField] private MeshRenderer mesh_renderer; // meshRenderer component to render the mesh with a material

    // Method to draw the map as a texture
    public void DrawTexture(Texture2D texture)
    {
        texture_render.sharedMaterial.mainTexture = texture;
        texture_render.transform.localScale = new Vector3(texture.width, 1, texture.height); // adjust the scale of the texture to match its dimensions
    }

    // Method to draw the map as a mesh
    public void DrawMesh(MeshData meshData, Texture2D texture) // one of the input parameters is my MeshData class (see in MeshGenerator.cs)
    {
        mesh_filter.sharedMesh = meshData.CreateMesh();
        mesh_renderer.sharedMaterial.mainTexture = texture;
    }
}
