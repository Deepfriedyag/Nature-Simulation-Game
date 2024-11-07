using System.Collections.Generic;
using UnityEngine;

public class SaveGameHandler : MonoBehaviour
{
    [SerializeField] private Dictionary<string, string> vegetation_type_to_prefab_name = new Dictionary<string, string>();
    [SerializeField] private Dictionary<string, GameObject> vegetation_prefabs = new Dictionary<string, GameObject>();
    // DO THE ABOVE FOR ANIMALS LATER []

    [SerializeField] private Transform vegetation_parent;
 
    private void Awake() // awake is a reserved Unity method that is run when the script is first loaded
    {
        // map saved names to actual prefab names
        vegetation_type_to_prefab_name["Grass(Clone)"] = "Grass"; // assumes prefab is named "Grass" in the Resources folder
        vegetation_type_to_prefab_name["Tree(Clone)"] = "Tree";

        // load prefabs from Resources based on mapped names
        foreach (var entry in vegetation_type_to_prefab_name)
        {
            GameObject prefab = Resources.Load<GameObject>(entry.Value);
            if (prefab != null)
            {
                vegetation_prefabs[entry.Key] = prefab;
                Debug.Log($"Loaded prefab for {entry.Key}: {entry.Value}");
            }
            else
            {
                Debug.LogWarning($"Prefab {entry.Value} not found in Resources for vegetation type {entry.Key}");
            }
        }
    }

    [System.Serializable] // this attribute tells Unity to serialize this class. This means that it can be converted to a byte stream and saved to disk
    public class GameData
    {
        public int world_seed;
        public Vector3 player_position;
        public Vector3 player_rotation;

        public List<AnimalData> animal_data;
        public List<VegetationData> vegetation_data;
    }

    [System.Serializable]
    public class AnimalData
    {
        public string type;
        public string ai_state;
        public int health;
        public int thirst;
        public int hunger;
        public Vector3 position;
        public Vector3 rotation;
    }

    [System.Serializable]
    public class VegetationData
    {
        public string vegetation_type;
        public Vector3 vegetation_position;
        public Vector3 vegetation_rotation;
    }

    public GameData GatherGameData()
    {
        GameData game_data = new GameData();
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera"); // find the player object by tag for reference because it is a game object and not a script

        game_data.world_seed = MapGenerator.world_seed;
        game_data.player_position = player.transform.position;
        game_data.player_rotation = player.transform.eulerAngles;

        // collect animal data
        //game_data.animal_data = new list<animaldata>();
        //gameobject[] animals = gameobject.findgameobjectswithtag("animal");

        //foreach (gameobject animal in animals) // loop through all the animals in the world
        //{
        //    animaldata animal_data = new animaldata();
        //    animal_data.type = animal.name;
        //    animal_data.ai_state = animal.getcomponent<animalai>().ai_state; // fix later [][][][][[][][[][][][][][ how does this work for different ai scripts?
        //    animal_data.health = animal.getcomponent<animalai>().health; // getcomponent is a method that gets the component (the animalai script in this case) of a game object to access its variables
        //    animal_data.thirst = animal.getcomponent<animalai>().thirst;
        //    animal_data.hunger = animal.getcomponent<animalai>().hunger;
        //    animal_data.position = animal.transform.position;
        //    animal_data.rotation = animal.transform.eulerangles;
        //    game_data.animal_data.add(animal_data);
        //}

        // collect vegetation data
        game_data.vegetation_data = new List<VegetationData>();
        GameObject[] vegetation = GameObject.FindGameObjectsWithTag("Vegetation");

        foreach (GameObject veg in vegetation)
        {
            VegetationData vegetation_data = new VegetationData();
            vegetation_data.vegetation_type = veg.name;
            vegetation_data.vegetation_position = veg.transform.position;
            vegetation_data.vegetation_rotation = veg.transform.eulerAngles;
            game_data.vegetation_data.Add(vegetation_data);
        }

        return game_data;
    }

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("MainCamera");

        GameData game_data = GatherGameData();
        string game_location_path = Application.persistentDataPath + "/Saves.json"; // Application.persistentDataPath is consistent across all platforms and points to the data folder of the game

        string save_file = JsonUtility.ToJson(game_data, true); // convert game data to JSON format. the boolean parameter (true) makes the JSON output more readable by adding more spacing
        System.IO.File.WriteAllText(game_location_path, save_file); // write the JSON data to a file
        Debug.Log("info - Game saved to " + game_location_path); // log a message to the dev console
    }

    public void LoadGame()
    {
        try // this is to prevent a possible crash if the file we are looking for does not exist. since this method will be called from a button click, we don't need to worry about making a loop handle this case
        {
            string json = System.IO.File.ReadAllText(Application.persistentDataPath + "/Saves.json"); // read the JSON data from the file
            GameData game_data = JsonUtility.FromJson<GameData>(json); // convert the JSON data back to a GameData object

            // load player data
            GameObject player = GameObject.FindGameObjectWithTag("MainCamera");
            MapGenerator.world_seed = game_data.world_seed;
            player.transform.position = game_data.player_position;
            player.transform.eulerAngles = game_data.player_rotation;

            // load animal data
            // ADD THIS LATER []

            // load vegetation data
            foreach (VegetationData veg_data in game_data.vegetation_data)
            {
                if (vegetation_prefabs.TryGetValue(veg_data.vegetation_type, out GameObject prefab))
                {
                    Instantiate(prefab, veg_data.vegetation_position, Quaternion.Euler(veg_data.vegetation_rotation), vegetation_parent);
                }
                else
                {
                    Debug.LogWarning($"Warning - No prefab found for vegetation type: {veg_data.vegetation_type}");
                }
            }

            Debug.Log("Info - Save loaded from " + Application.persistentDataPath + "/Saves.json");
        }
        catch
        {
            Debug.Log("Error - No save file found, cannot load game");
        }
        
    }

}