using System;
using System.Collections.Generic;
using UnityEngine;

// handles saving and loading game state in a Unity game
public class SaveGameHandler : MonoBehaviour
{
    [SerializeField] private Transform spawned_object_parent; // the parent object for all spawned objects in the game, used for organisation
    [SerializeField] private List<string> animal_tags; // used for identifying animal objects
    [SerializeField] private List<string> vegetation_tags; // used for identifying vegetation objects

    // Represents the complete game state data structure for saving/loading. "[System.Serializable]" attribute allows ".json" conversion
    [System.Serializable]
    public class GameData
    {
        public int world_seed; // seed used to generate the game world, ensuring consistent regeneration

        // player's position and rotation at the time of saving
        public Vector3 player_position;
        public Vector3 player_rotation;

        // lists to store detailed information about animals and vegetation
        public List<AnimalData> animal_data = new List<AnimalData>();
        public List<VegetationData> vegetation_data = new List<VegetationData>();
    }

    // detailed data structure representing an animal's state and properties. most of these values are from my AnimalAI class
    [System.Serializable]
    public class AnimalData
    {
        public string tag; // identifies the specific animal type/prefab
        public string ai_state;
        public float health;
        public float hunger;
        public float age;
        public float mating_cooldown;
        public Vector3 position;
        public Vector3 rotation;
    }

    // detailed data structure representing vegetation's position and orientation
    [System.Serializable]
    public class VegetationData
    {
        public string tag; // identifies the specific vegetation type/prefab
        public Vector3 position;
        public Vector3 rotation;
    }

    // collects and compiles all current game state information, then returns a GameData object containing comprehensive game state details
    public GameData GatherGameData()
    {
        GameData game_data = new GameData();
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera");

        // capture world generation seed and player location details
        game_data.world_seed = MapGenerator.world_seed;
        game_data.player_position = player.transform.position;
        game_data.player_rotation = player.transform.eulerAngles;

        // collect data for all animals in the game world
        game_data.animal_data = new List<AnimalData>();
        foreach (string animal_tag in animal_tags)
        {
            // find all game objects with the current animal tag
            GameObject[] animals = GameObject.FindGameObjectsWithTag(animal_tag);
            foreach (GameObject animal in animals)
            {
                AnimalAI animalAI = animal.GetComponent<AnimalAI>();
                if (animalAI != null)
                {
                    // create detailed animal data record
                    AnimalData animal_data = new AnimalData
                    {
                        tag = animal.tag,
                        ai_state = animalAI.CurrentState,
                        health = animalAI.CurrentHealth,
                        hunger = animalAI.CurrentHunger,
                        age = animalAI.CurrentAge,
                        mating_cooldown = animalAI.MatingCooldownTimer,
                        position = animal.transform.position,
                        rotation = animal.transform.eulerAngles
                    };

                    game_data.animal_data.Add(animal_data);
                }
            }
        }

        // collect data for all vegetation in the game world
        game_data.vegetation_data = new List<VegetationData>();
        foreach (string veg_tag in vegetation_tags)
        {
            // find all game objects with vegetation tags
            GameObject[] vegetation = GameObject.FindGameObjectsWithTag(veg_tag);
            foreach (GameObject veg in vegetation)
            {
                // create detailed vegetation data record
                VegetationData vegetation_data = new VegetationData
                {
                    tag = veg.tag,
                    position = veg.transform.position,
                    rotation = veg.transform.eulerAngles
                };

                game_data.vegetation_data.Add(vegetation_data);
            }
        }

        return game_data;
    }

    // saves the current game state to a JSON file
    public void SaveGame()
    {
        // collect current game state
        GameData game_data = GatherGameData();

        // determine save file path in persistent data directory. Application.persistentDataPath is a platform-independent path to the game's save data directory
        string game_location_path = Application.persistentDataPath + "/Saves.json";

        // convert game data to JSON
        string save_file = JsonUtility.ToJson(game_data, true);

        // write JSON to file
        System.IO.File.WriteAllText(game_location_path, save_file);
        Debug.Log("Info - Game saved to " + game_location_path);
    }

    // loads the save game from the JSON file we get using SaveGame()
    public void LoadGame()
    {
        try
        {
            // read save file from persistent data directory
            string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/Saves.json");
            GameData game_data = JsonUtility.FromJson<GameData>(json);

            // restore player state
            GameObject player = GameObject.FindGameObjectWithTag("MainCamera");
            MapGenerator.world_seed = game_data.world_seed;
            player.transform.position = game_data.player_position;
            player.transform.eulerAngles = game_data.player_rotation;

            // restore animals' states and instantiate animals
            foreach (AnimalData animal_data in game_data.animal_data)
            {
                // load animal prefabs based on saved tags
                GameObject prefab = Resources.Load<GameObject>(animal_data.tag);
                if (prefab != null)
                {
                    // instantiate (spawn) animals at saved positions and rotations
                    GameObject animal = Instantiate(prefab, animal_data.position, Quaternion.Euler(animal_data.rotation), spawned_object_parent);

                    AnimalAI animalAI = animal.GetComponent<AnimalAI>();
                    if (animalAI != null)
                    {
                        // restore detailed animal properties
                        RestoreState(animalAI, animal_data.ai_state);
                        animalAI.CurrentHealth = animal_data.health;
                        animalAI.CurrentHunger = animal_data.hunger;
                        animalAI.CurrentAge = animal_data.age;
                        animalAI.MatingCooldownTimer = animal_data.mating_cooldown;
                    }
                }
                else
                {
                    Debug.LogWarning($"No prefab found for animal tag: {animal_data.tag}");
                }
            }

            // restore vegetation states and instantiate vegetation
            foreach (VegetationData veg_data in game_data.vegetation_data)
            {
                // Load vegetation prefab based on saved tag
                GameObject prefab = Resources.Load<GameObject>(veg_data.tag);
                if (prefab != null)
                {
                    Instantiate(prefab, veg_data.position, Quaternion.Euler(veg_data.rotation), spawned_object_parent);
                }
                else
                {
                    Debug.LogWarning($"No prefab found for vegetation tag: {veg_data.tag}");
                }
            }

            Debug.Log("Info - Save loaded from " + Application.persistentDataPath + "/Saves.json"); // log the successful load
        }
        catch
        {
            Debug.Log("Error - No save file found, cannot load game");
        }
    }

    // restores an animal's AI state using reflection to access private fields
    private void RestoreState(AnimalAI animalAI, string stateName)
    {
        // use reflection to access the private state field
        var type = typeof(AnimalAI);
        var state_field = type.GetField("currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (state_field != null)
        {
            // parse the state name to the correct enum type
            var state_enum_type = state_field.FieldType;
            var state_value = Enum.Parse(state_enum_type, stateName);

            // set the private state field
            state_field.SetValue(animalAI, state_value);
        }
        else
        {
            Debug.LogWarning($"Failed to restore state: {stateName}");
        }
    }
}