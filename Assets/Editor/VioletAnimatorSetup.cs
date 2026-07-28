using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// One-shot setup: fix root motion node on Mixamo anim FBX imports and build
/// the VioletAnimator controller. Run via -executeMethod VioletAnimatorSetup.Run
/// </summary>
public static class VioletAnimatorSetup
{
    const string AnimDir = "Assets/Art/Characters/Violet/Animations";
    const string ControllerPath = "Assets/Art/Characters/Violet/VioletAnimator.controller";

    [MenuItem("VioletAndMarlowe/Setup Violet Animator")]
    public static void Run()
    {
        var report = new List<string>();

        // --- 1. Root motion node = Hips on all 5 anim imports ---
        string[] fbxNames = { "Idle", "Walking", "Running", "Jump", "Start Walking" };
        foreach (var name in fbxNames)
        {
            string path = $"{AnimDir}/{name}.fbx";
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) { report.Add($"FAIL: no ModelImporter at {path}"); continue; }

            bool changed = false;
            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                changed = true;
            }
            if (importer.motionNodeName != "mixamorig:Hips")
            {
                importer.motionNodeName = "mixamorig:Hips";
                changed = true;
            }
            // Ensure loop settings survive
            var clips = importer.defaultClipAnimations;
            for (int i = 0; i < clips.Length; i++)
            {
                bool wantLoop = name != "Jump";
                if (clips[i].loopTime != wantLoop)
                {
                    clips[i].loopTime = wantLoop;
                    changed = true;
                }
            }
            importer.clipAnimations = clips;

            if (changed)
            {
                importer.SaveAndReimport();
                report.Add($"OK: {path} reimported (motionNodeName=mixamorig:Hips, humanoid, copy-avatar)");
            }
            else
            {
                report.Add($"OK: {path} already configured, no reimport");
            }
        }

        // --- 2. Load clips (each FBX has one clip named "mixamo.com") ---
        AnimationClip LoadClip(string fbx)
        {
            string path = $"{AnimDir}/{fbx}.fbx";
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (obj is AnimationClip clip && !clip.name.StartsWith("__preview"))
                    return clip;
            }
            report.Add($"FAIL: no AnimationClip found in {path}");
            return null;
        }

        var idle = LoadClip("Idle");
        var walk = LoadClip("Walking");
        var run = LoadClip("Running");
        var jump = LoadClip("Jump");
        if (idle == null || walk == null || run == null || jump == null)
        {
            File.WriteAllLines("/tmp/violet_animator_setup.txt", report);
            Debug.LogError("[VioletAnimatorSetup] Missing clips, aborting. See /tmp/violet_animator_setup.txt");
            return;
        }
        report.Add($"Clips: idle={idle.name} ({idle.length:F2}s, loop={idle.isLooping}), walk={walk.name} ({walk.length:F2}s, loop={walk.isLooping}), run={run.name} ({run.length:F2}s, loop={run.isLooping}), jump={jump.name} ({jump.length:F2}s, loop={jump.isLooping})");

        // --- 3. Build controller ---
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (existing != null) AssetDatabase.DeleteAsset(ControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

        var sm = controller.layers[0].stateMachine;

        var idleState = sm.AddState("Idle"); idleState.motion = idle;
        var walkState = sm.AddState("Walk"); walkState.motion = walk;
        var runState = sm.AddState("Run"); runState.motion = run;
        var jumpState = sm.AddState("Jump"); jumpState.motion = jump;
        sm.defaultState = idleState;

        AnimatorStateTransition T(AnimatorState from, AnimatorState to, AnimatorConditionMode mode, string param, float threshold, bool exitTime, float exit = 0.9f)
        {
            var t = from.AddTransition(to);
            t.hasExitTime = exitTime;
            if (exitTime) t.exitTime = exit;
            t.duration = 0.1f;
            if (!string.IsNullOrEmpty(param)) t.AddCondition(mode, threshold, param);
            return t;
        }

        T(idleState, walkState, AnimatorConditionMode.Greater, "Speed", 0.5f, false);
        T(walkState, idleState, AnimatorConditionMode.Less, "Speed", 0.3f, false);
        T(walkState, runState, AnimatorConditionMode.Greater, "Speed", 5.0f, false);
        T(runState, walkState, AnimatorConditionMode.Less, "Speed", 4.5f, false);

        var anyJump = sm.AddAnyStateTransition(jumpState);
        anyJump.hasExitTime = false;
        anyJump.duration = 0.1f;
        anyJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
        anyJump.canTransitionToSelf = false;

        var jumpExit = jumpState.AddTransition(idleState);
        jumpExit.hasExitTime = true;
        jumpExit.exitTime = 0.9f;
        jumpExit.duration = 0.1f;

        AssetDatabase.SaveAssets();
        report.Add($"OK: controller created at {ControllerPath} with params [Speed(float), Jump(trigger)], states [Idle(default), Walk, Run, Jump]");

        File.WriteAllLines("/tmp/violet_animator_setup.txt", report);
        foreach (var line in report) Debug.Log("[VioletAnimatorSetup] " + line);
    }
}
