using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (MapGenerator))]
public class MapGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator map_gen = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            if (map_gen.auto_update_map)
            {
                map_gen.GenerateMap();
            }
        }

        if (GUILayout.Button("Generate")) // to be able to manually generate the map without starting the game (for development purposes)
        {
            map_gen.GenerateMap();
        }
    }

}
