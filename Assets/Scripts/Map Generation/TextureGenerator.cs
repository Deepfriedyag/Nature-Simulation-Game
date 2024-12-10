using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColourMap(Color[] colour_map, int width, int height) // takes an array of colours (colour_map) and maps it onto a Texture2D (used to represent textures in Unity)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Trilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colour_map);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] height_map) // converts a 2D float array (height_map) representing height values (normalized to the range [0, 1]) into a grayscale colour map.
    {
        int width = height_map.GetLength(0);
        int height = height_map.GetLength(1);
        Color[] colour_map = new Color[width * height];

        for (int y = 0; y < height; y++) // iterate over rows
        {
            for (int x = 0; x < width; x++) // iterate over columns
            {
                colour_map[y * width + x] = Color.Lerp(Color.black, Color.white, height_map[x, y]); // set the colour of the pixel at (x, y) to a grayscale colour based on the height value
            }
        }

        return TextureFromColourMap(colour_map, width, height); // return the final texture
    }
}