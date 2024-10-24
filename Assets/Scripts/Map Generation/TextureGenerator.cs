using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colour_map, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Trilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colour_map);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] height_map)
    {
        int width = height_map.GetLength(0);
        int height = height_map.GetLength(1);
        Color[] colour_map = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colour_map[y * width + x] = Color.Lerp(Color.black, Color.white, height_map[x, y]);
            }
        }

        return TextureFromColourMap(colour_map, width, height);
    }

}
