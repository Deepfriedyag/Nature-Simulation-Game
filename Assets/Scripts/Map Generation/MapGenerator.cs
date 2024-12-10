using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private const byte MAP_SIZE = 241; // 241 because 240 (241 - 1) is divisible by a lot of numbers and this will be useful when implementing optimisations later on (e.g chunks)

    public static int world_seed; // the same seed value will generate the same map. This variable is static so it can be accessed from other classes without needing a reference to the class
    public static bool is_new_game; // if the world will be generated with a new (random) seed
    public bool is_auto_update_map; // if the map should update itself automatically when a value is changed. useful for debugging and testing the terrain generation

    [Range(0, 6)] [SerializeField] private int level_of_detail; // the better the level of detail, the more vertices the mesh will have
    [SerializeField] private int octaves; // the number of layers of noise that will be added together
    [SerializeField] private float noise_scale; // the scale of the noise algorithm
    [SerializeField] private float mesh_height_multiplier;
    [SerializeField] private float lacunarity; // the rate at which the frequency of the noise increases
    [Range(0, 1)] // this attribute turns persistance into a slider in the unity editor with values clamped between 0 and 1
    [SerializeField] private float persistance; // the rate at which the amplitude of the noise decreases

    [SerializeField] private DrawMode draw_mode; // the draw mode of the map, useful for debugging
    [SerializeField] private Vector2 offset; // offset of the noise map
    [SerializeField] private TerrainType[] regions; // the regions are the different colours/biomes of the map
    [SerializeField] private AnimationCurve mesh_height_curve; // set to be a exponential curve in the unity editor so the terrain looks more natural

    [SerializeField] private PlaceVegetation PlaceVegetation; // reference to my class
    [SerializeField] private SaveGameHandler SaveGameHandler; // reference to my class

    [SerializeField] // this attribute makes a private variable show up in the unity editor, in this case the enum DrawMode
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

    private void Start() // reserved Unity method. called once when the script is first loaded. generates a random or saved map based on is_new_game boolean
    {
        if (is_new_game == true)
        {
            GenerateRandomSeed();
            GenerateMap();
            Debug.Log("Info - Generated world with the seed: " + world_seed); // Debug.Log is a method that prints a message to Unity's built in dev console for debugging purposes
            PlaceVegetation.PlaceObjects();
            Debug.Log("Info - Placed vegetation");
            IngameConsole.Instance.LogMessage("Generated new world with the seed: " + world_seed);
        }
        else
        {
            SaveGameHandler.LoadGame(); // my class and its method for loading the saved game state
            GenerateMap();
            IngameConsole.Instance.LogMessage("Loaded saved game");
        }
    }

    private int GenerateRandomSeed() // generates a random seed value to be used in GenerateMap() as a parameter for Noise.GenerateNoiseMap(). otherwise the seed is set to 0.
    {
        world_seed = Random.Range(int.MinValue, int.MaxValue);
        return world_seed; // we return the seed (my SaveGameHandler class accesses this variable) for when we want to generate the same map again after saving the game
    }

    public void GenerateMap() // generates the terrain based on the parameters set in the Unity editor and displays it accordingly using my MapDisplay class
    {
        float[,] noise_map = Noise.GenerateNoiseMap(MAP_SIZE, MAP_SIZE, noise_scale, octaves, persistance, lacunarity, world_seed, offset);
        Color[] colour_map = new Color[MAP_SIZE * MAP_SIZE];

        for (int y = 0; y < MAP_SIZE; y++) // loop through each point in the noise map
        {
            for (int x = 0; x < MAP_SIZE; x++)
            {
                float current_height = noise_map[x, y];

                for (int i = 0; i < regions.Length; i++) // determine the colour based on the height value by comparing it to the height thresholds in the regions array
                {
                    if (current_height <= regions[i].height)
                    {
                        colour_map[y * MAP_SIZE + x] = regions[i].colour;
                        break;
                    }
                }

            }
        }

        MapDisplay display = FindAnyObjectByType<MapDisplay>(); // find the MapDisplay object in the scene to display the generated map
        // check the selected draw mode and display the map accordingly. in the game world, the mesh mode is used to display the terrain in 3D. the other modes are for debugging and 2D flat surfaces
        if (draw_mode == DrawMode.noise_map) // draw the noise map texture
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noise_map)); 
        }
        else if (draw_mode == DrawMode.colour_map) // draw the colour map texture
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colour_map, MAP_SIZE, MAP_SIZE));
        }
        else if (draw_mode == DrawMode.mesh) // generate the terrain mesh and draw it with the colour map texture
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noise_map, mesh_height_multiplier, mesh_height_curve, level_of_detail), TextureGenerator.TextureFromColourMap(colour_map, MAP_SIZE, MAP_SIZE));
        }

    }

    private void OnValidate() // reserved Unity method. activates when one of the values in this class are changed. makes it unable to set values lower than specified as it would break the map generation
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