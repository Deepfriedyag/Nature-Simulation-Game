using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int map_width, int map_height, float scale, int octaves, float persistance, float lacunarity, int seed, Vector2 offset)
    {
        float[,] noise_map = new float[map_width, map_height];
        float max_noise_height = float.MinValue;
        float min_noise_height = float.MaxValue;
        float half_width = map_width / 2f;
        float half_height = map_width / 2f;

        System.Random rng = new System.Random(seed);
        Vector2[] octave_offsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offset_x = rng.Next(-100000, 100000) + offset.x;
            float offset_y = rng.Next(-100000, 100000) + offset.y;
            octave_offsets[i] = new Vector2(offset_x, offset_y);
        }

        if (scale <= 0)
        {
            scale = 0.001f;
        }

        for (int y = 0; y < map_height; y++)
        {
            for (int x = 0; x < map_width; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noise_height = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sample_x = (x - half_width) / scale * frequency + octave_offsets[i].x;
                    float sample_y = (y - half_height) / scale * frequency + octave_offsets[i].y;

                    float perlin_value = Mathf.PerlinNoise(sample_x, sample_y) * 2 - 1; //(The * 2 - 1 makes it so we can get negative perlin values [in the range -1 to 1] so the noise height sometimes decrease. This contributes to getting more interesting noise/terrain)
                    noise_height += perlin_value * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noise_height > max_noise_height)
                {
                    max_noise_height = noise_height;
                }
                else if (noise_height < min_noise_height)
                {
                    min_noise_height = noise_height;
                }

                noise_map[x, y] = noise_height;
            }
        }

        for (int y = 0; y < map_height; y++)
        {
            for (int x = 0; x < map_width; x++)
            {
                noise_map[x, y] = Mathf.InverseLerp(min_noise_height, max_noise_height, noise_map[x, y]);
            }
        }

        return noise_map;

    }
}
