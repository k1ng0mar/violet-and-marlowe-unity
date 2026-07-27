using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.Build;

public static class AndroidBuilder
{
    public static void BuildAPK()
    {
        // Ensure Android build target is active
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        // Set ARM64-only architecture via the Android-specific API
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

        // Verify settings
        Debug.Log($"[BUILD] Android targetArchitectures: {PlayerSettings.Android.targetArchitectures}");
        Debug.Log($"[BUILD] Scripting backend: {PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android)}");

        // Build
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/WalkTheBlock.unity" },
            locationPathName = "/home/ubuntu/violet-and-marlowe-unity/build/violet-and-marlowe.apk",
            target = BuildTarget.Android,
            targetGroup = BuildTargetGroup.Android,
            options = BuildOptions.Development | BuildOptions.AllowDebugging
        });

        var summary = report.summary;
        Debug.Log($"[BUILD] Result: {summary.result}");
        Debug.Log($"[BUILD] Total errors: {summary.totalErrors}");
        Debug.Log($"[BUILD] Total warnings: {summary.totalWarnings}");
        Debug.Log($"[BUILD] Output path: {summary.outputPath}");
        Debug.Log($"[BUILD] Total time: {summary.totalTime.TotalSeconds}s");
    }
}
