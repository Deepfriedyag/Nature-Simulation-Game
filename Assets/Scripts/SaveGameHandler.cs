using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameHandler : MonoBehaviour
{
    [SerializeField] private Transform spawned_object_parent;
    [SerializeField] private List<string> animal_tags;  // List of tags for all animal types
    [SerializeField] private List<string> vegetation_tags;  // List of tags for all vegetation types

    [System.Serializable]
    public class GameData
    {
        public int world_seed;
        public Vector3 player_position;
        public Vector3 player_rotation;

        public List<AnimalData> animal_data = new List<AnimalData>();
        public List<VegetationData> vegetation_data = new List<VegetationData>();
    }

    [System.Serializable]
    public class AnimalData
    {
        public string tag;  // Tag for identifying the animal type
        public string ai_state;
        public float health;
        public float hunger;
        public Vector3 position;
        public Vector3 rotation;
    }

    [System.Serializable]
    public class VegetationData
    {
        public string tag;  // Tag for identifying the vegetation type
        public Vector3 position;
        public Vector3 rotation;
    }

    public GameData GatherGameData()
    {
        GameData game_data = new GameData();
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera");

        game_data.world_seed = MapGenerator.world_seed;
        game_data.player_position = player.transform.position;
        game_data.player_rotation = player.transform.eulerAngles;

        // Collect animal data
        game_data.animal_data = new List<AnimalData>();
        foreach (string animal_tag in animal_tags) // Loop through all animal tags
        {
            GameObject[] animals = GameObject.FindGameObjectsWithTag(animal_tag);
            foreach (GameObject animal in animals)
            {
                AnimalAI animalAI = animal.GetComponent<AnimalAI>();
                if (animalAI != null)
                {
                    AnimalData animal_data = new AnimalData();
                    animal_data.tag = animal.tag;  // Use tag for prefab identification
                    animal_data.ai_state = animalAI.CurrentState;  // Access public property
                    animal_data.health = animalAI.CurrentHealth;  // Access health property
                    animal_data.hunger = animalAI.CurrentHunger;  // Access hunger property
                    animal_data.position = animal.transform.position;
                    animal_data.rotation = animal.transform.eulerAngles;

                    game_data.animal_data.Add(animal_data);
                }
            }
        }

        // Collect vegetation data
        game_data.vegetation_data = new List<VegetationData>();
        foreach (string veg_tag in vegetation_tags)  // Loop through all vegetation tags
        {
            GameObject[] vegetation = GameObject.FindGameObjectsWithTag(veg_tag);
            foreach (GameObject veg in vegetation)
            {
                VegetationData vegetation_data = new VegetationData();
                vegetation_data.tag = veg.tag;  // Use tag for prefab identification
                vegetation_data.position = veg.transform.position;
                vegetation_data.rotation = veg.transform.eulerAngles;

                game_data.vegetation_data.Add(vegetation_data);
            }
        }

        return game_data;
    }

    public void SaveGame()
    {
        GameData game_data = GatherGameData();
        string game_location_path = Application.persistentDataPath + "/Saves.json";
        string save_file = JsonUtility.ToJson(game_data, true);
        System.IO.File.WriteAllText(game_location_path, save_file);
        Debug.Log("Info - Game saved to " + game_location_path);
    }

    public void LoadGame()
    {
        try
        {
            string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/Saves.json");
            GameData game_data = JsonUtility.FromJson<GameData>(json);

            GameObject player = GameObject.FindGameObjectWithTag("MainCamera");
            MapGenerator.world_seed = game_data.world_seed;
            player.transform.position = game_data.player_position;
            player.transform.eulerAngles = game_data.player_rotation;

            // Load animal data
            foreach (AnimalData animal_data in game_data.animal_data)
            {
                GameObject prefab = Resources.Load<GameObject>(animal_data.tag); // Load prefab based on tag
                if (prefab != null)
                {
                    GameObject animal = Instantiate(prefab, animal_data.position, Quaternion.Euler(animal_data.rotation), spawned_object_parent);
                    AnimalAI animalAI = animal.GetComponent<AnimalAI>();
                    if (animalAI != null)
                    {
                        RestoreState(animalAI, animal_data.ai_state);  // Restore the state directly
                        animalAI.CurrentHealth = animal_data.health;  // Set health
                        animalAI.CurrentHunger = animal_data.hunger;  // Set hunger
                    }
                }
                else
                {
                    Debug.LogWarning($"No prefab found for animal tag: {animal_data.tag}");
                }
            }

            // Load vegetation data
            foreach (VegetationData veg_data in game_data.vegetation_data)
            {
                GameObject prefab = Resources.Load<GameObject>(veg_data.tag); // Load prefab based on tag
                if (prefab != null)
                {
                    Instantiate(prefab, veg_data.position, Quaternion.Euler(veg_data.rotation), spawned_object_parent);
                }
                else
                {
                    Debug.LogWarning($"No prefab found for vegetation tag: {veg_data.tag}");
                }
            }

            Debug.Log("Info - Save loaded from " + Application.persistentDataPath + "/Saves.json");
        }
        catch
        {
            Debug.Log("Error - No save file found, cannot load game");
        }
    }

    private void RestoreState(AnimalAI animalAI, string stateName)
    {
        var type = typeof(AnimalAI);
        var stateField = type.GetField("currentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (stateField != null)
        {
            var stateEnumType = stateField.FieldType;
            var stateValue = Enum.Parse(stateEnumType, stateName);
            stateField.SetValue(animalAI, stateValue);
        }
        else
        {
            Debug.LogWarning($"Failed to restore state: {stateName}");
        }
    }
}
