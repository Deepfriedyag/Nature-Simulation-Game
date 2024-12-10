using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int map_width, int map_height, float scale, int octaves, float persistance, float lacunarity, int seed, Vector2 offset) // generates a 2D noise map using the Perlin noise algorithm. most of its parameters are defined in the MapGenerator class, where this class gets called from.
    {
        float[,] noise_map = new float[map_width, map_height];
        float max_noise_height = float.MinValue;
        float min_noise_height = float.MaxValue;
        float half_width = map_width / 2f; // calculate half dimensions for centering the noise
        float half_height = map_width / 2f;

        System.Random rng = new System.Random(seed); // initialize random number generator with the given seed
        Vector2[] octave_offsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++) // generate random offsets for each octave
        {
            float offset_x = rng.Next(-100000, 100000) + offset.x;
            float offset_y = rng.Next(-100000, 100000) + offset.y;
            octave_offsets[i] = new Vector2(offset_x, offset_y);
        }

        if (scale <= 0) // ensures scale is not zero or negative as it would cause a division by zero error or if it was negative, it would cause the noise to be inverted
        {
            scale = 0.001f;
        }

        for (int y = 0; y < map_height; y++) // loop through each point in the noise map, calculate the noise value and store it
        {
            for (int x = 0; x < map_width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noise_height = 0;

                for (int i = 0; i < octaves; i++) // loop through each octave to calculate the noise height
                {
                    float sample_x = (x - half_width) / scale * frequency + octave_offsets[i].x;
                    float sample_y = (y - half_height) / scale * frequency + octave_offsets[i].y;

                    float perlin_value = Mathf.PerlinNoise(sample_x, sample_y) * 2 - 1; // get Perlin noise value and adjust it to range [-1, 1] so the noise height sometimes decrease. this contributes to getting more interesting noise/terrain)
                    noise_height += perlin_value * amplitude;

                    // adjust amplitude and frequency for next octave
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // update max and min noise heights
                if (noise_height > max_noise_height)
                {
                    max_noise_height = noise_height;
                }
                else if (noise_height < min_noise_height)
                {
                    min_noise_height = noise_height;
                }

                noise_map[x, y] = noise_height; // store the noise height in the noise map
            }
        }

        for (int y = 0; y < map_height; y++) // normalise the noise map values to range [0, 1] using the min and max noise heights
        {
            for (int x = 0; x < map_width; x++)
            {
                noise_map[x, y] = Mathf.InverseLerp(min_noise_height, max_noise_height, noise_map[x, y]);
            }
        }

        return noise_map;

    }
}
