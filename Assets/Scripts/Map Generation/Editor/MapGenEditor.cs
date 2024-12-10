using UnityEngine;
using UnityEditor;

// this attribute tells Unity to use this custom editor for the MapGenerator class
[CustomEditor(typeof(MapGenerator))]
public class MapGenEditor : Editor
{
    // override the default inspector GUI
    public override void OnInspectorGUI()
    {
        // cast the target object to MapGenerator
        MapGenerator map_gen = (MapGenerator)target;

        // draw the default inspector and check if any value was changed
        if (DrawDefaultInspector())
        {
            // if auto-update is enabled, generate the map whenever a value changes
            if (map_gen.is_auto_update_map)
            {
                map_gen.GenerateMap();
            }
        }

        // add a button to the inspector to manually generate the map
        if (GUILayout.Button("Generate"))
        {
            // generate the map when the button is clicked
            map_gen.GenerateMap();
        }
    }
}
