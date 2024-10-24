using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer texture_render;
    public MeshFilter mesh_filter;
    public MeshRenderer mesh_renderer;
    
    public void DrawTexture(Texture2D texture)
    {
        texture_render.sharedMaterial.mainTexture = texture;
        texture_render.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData MeshData, Texture2D texture)
    {
        mesh_filter.sharedMesh = MeshData.CreateMesh();
        mesh_renderer.sharedMaterial.mainTexture = texture;
    }

}
