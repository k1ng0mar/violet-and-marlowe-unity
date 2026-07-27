using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class BuildSettingsSetup
{
    static BuildSettingsSetup()
    {
        // Add WalkTheBlock scene to build settings
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/WalkTheBlock.unity", true)
        };
        Debug.Log("Build settings updated: WalkTheBlock scene added");
    }
}
