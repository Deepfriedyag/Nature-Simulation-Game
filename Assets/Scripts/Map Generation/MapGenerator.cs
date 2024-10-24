using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] // this attribute makes a private variable show up in the unity editor
    private enum DrawMode // different draw modes for the map, useful for debugging
    {
        noise_map,
        colour_map,
        mesh
    }

    [System.Serializable] // structs by default do not show up in the unity editor, but this attribute makes it show up
    private struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }

    private const byte MAP_SIZE = 241; // 241 because 240 (241 - 1) is divisible by a lot of numbers and this will be useful when implementing optimisations later on

    public int seed; // the same seed will generate the same map
    [Range(0, 6)] [SerializeField] private int level_of_detail; // the level of detail of the mesh (the better level of detail, the more vertices the mesh will have)
    [SerializeField] private int octaves; // the number of layers of noise that will be added together
    [SerializeField] private float noise_scale; // the scale of the noise algorithm
    [SerializeField] private float mesh_height_multiplier; // the multiplier of the height of the mesh
    [SerializeField] private float lacunarity; // the rate at which the frequency of the noise increases
    [Range(0, 1)] // this attribute turns persistance into a slider in the unity editor with values clamped between 0 and 1
    [SerializeField] private float persistance; // the rate at which the amplitude of the noise decreases

    [SerializeField] private DrawMode draw_mode; // the draw mode of the map
    [SerializeField] private Vector2 offset; // the offset of the noise map
    public bool auto_update_map; // if the map should update itself automatically when a value is changed
    [SerializeField] private TerrainType[] regions; // the regions of the map (the regions are the different colours/biomes of the map)
    [SerializeField] private AnimationCurve mesh_height_curve; // the curve of the mesh height (I set the curve to be a exponential curve in the unity editor so the terrain looks more natural)

    void Start() // this reserved unity method is called only when the script is first loaded
    {
        GenerateMap();
    }

    public int GenerateRandomSeed() // we return the seed (my SaveGameHandler class accesses this variable) for when we want to generate the same map again after saving the game
    {
        seed = Random.Range(int.MinValue, int.MaxValue);
        return seed;
    }

    public void GenerateMap()
    {
        float[,] noise_map = Noise.GenerateNoiseMap(MAP_SIZE, MAP_SIZE, noise_scale, octaves, persistance, lacunarity, seed, offset);
        Color[] colour_map = new Color[MAP_SIZE * MAP_SIZE];

        for (int y = 0; y < MAP_SIZE; y++)
        {
            for (int x = 0; x < MAP_SIZE; x++)
            {
                float current_height = noise_map[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (current_height <= regions[i].height)
                    {
                        colour_map[y * MAP_SIZE + x] = regions[i].colour;
                        break;
                    }
                }

            }
        }

        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if (draw_mode == DrawMode.noise_map)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noise_map));
        }
        else if (draw_mode == DrawMode.colour_map)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colour_map, MAP_SIZE, MAP_SIZE));
        }
        else if (draw_mode == DrawMode.mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noise_map, mesh_height_multiplier, mesh_height_curve, level_of_detail), TextureGenerator.TextureFromColourMap(colour_map, MAP_SIZE, MAP_SIZE));
        }

    }

    private void OnValidate() // makes you unable to set these values lower than specified. The OnValidate method activates when one of the values are changed
    {
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }

}