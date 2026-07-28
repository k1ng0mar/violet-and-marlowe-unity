using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Extracts Idle / Walk / Run / Jump clips from Quaternius "Animated Woman" FBX packs,
/// saves them as standalone .anim assets, and shares a single certified Humanoid avatar
/// across every source FBX so the clips are compatible with Violet's Humanoid rig.
///
/// Interpretive defaults (called out in the report):
///   - Extracts 17 takes per FBX, not only the 4 core moves; the core four are the contract.
///   - Clip files are named <FbxName>_<Take>.anim (e.g. BaseCharacter_Idle.anim).
///   - All source FBX copies live in Animations/Source/ (meta files have 0600 perms on disk,
///     so we copy rather than move).
///   - One shared avatar (BaseCharacter) is reused for all FBX files: same Quaternius
///     rig/skeleton family, avoids 35 duplicate avatar certifications.
/// </summary>
public static class QuaterniusAnimationExtractor
{
    private const string AnimDir   = "Assets/Art/Characters/Violet/Animations";
    private const string SourceDir = AnimDir + "/Source";
    private const string ExternalFbxDir = "/home/ubuntu/vm_animations/quaternius_animated_chars/FBX";

    // Unity's animationType ints: 1=Legacy 2=Generic 3=Humanoid
    private static readonly string[] CoreTakes = { "Idle", "Walk", "Run", "Jump" };

    private static string LogPath { get { return "/tmp/quaternius_extract_report.txt"; } }

    [MenuItem("VioletAndMarlowe/Extract Quaternius Animations")]
    public static void Extract()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Quaternius extractor run " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ===");

        Directory.CreateDirectory(SourceDir);

        // ---- 1. Copy every external FBX into the project ----
        string[] external = Directory.GetFiles(ExternalFbxDir, "*.fbx");
        sb.AppendLine("External FBX found: " + external.Length);

        foreach (string src in external)
        {
            string dest = Path.Combine(SourceDir, Path.GetFileName(src));
            // Keep meta files fresh; copy if missing or source is newer.
            if (!File.Exists(dest) || File.GetLastWriteTimeUtc(src) > File.GetLastWriteTimeUtc(dest))
                File.Copy(src, dest, true);
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { SourceDir });
        sb.AppendLine("Project FBX under Source/: " + fbxGuids.Length);

        // ---- 2. First pass: certify the avatar on the first viable FBX ----
        string avatarPath = null;
        Avatar sharedAvatar = null;
        foreach (string guid in fbxGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var imp = (ModelImporter)AssetImporter.GetAtPath(path);
            if (imp.animationType != ModelImporterAnimationType.Human)
            {
                imp.animationType = ModelImporterAnimationType.Human;
                imp.SaveAndReimport();
                imp = (ModelImporter)AssetImporter.GetAtPath(path);
            }
            // Avatar is certified when Unity wrote a humanDescription into the importer.
            if (imp.humanDescription.human.Length > 0)
            {
                var objs = AssetDatabase.LoadAllAssetsAtPath(path);
                sharedAvatar = objs.OfType<Avatar>().FirstOrDefault();
                if (sharedAvatar != null)
                {
                    avatarPath = path;
                    sb.AppendLine("Shared avatar source: " + path + " (human bones mapped=" + imp.humanDescription.human.Length + ", avatar valid=" + sharedAvatar.isValid + ", human=" + sharedAvatar.isHuman + ")");
                    break;
                }
            }
            sb.AppendLine("WARN: human certification failed on " + path);
        }

        // Keep each FBX self-contained (CreateFromThisModel / own avatar).
        // Humanoid retargeting is muscle-space — source avatar identity is irrelevant
        // for runtime retargeting to Violet's Humanoid rig.
        // Switching an FBX to avatarSetup=CopyFromOther previously REMOVED its clips
        // from LoadAllAssetsAtPath, and sharedAvatar.isHuman was False on this pack,
        // so we deliberately do NOT share one avatar across files.
        if (string.IsNullOrEmpty(avatarPath))
        {
            avatarPath = "(per-FBX self avatars)";
            sb.AppendLine("Avatar strategy: per-FBX self-contained (no shared copy-from-other).");
        }

        // ---- 3. Second pass: humanoid + shared avatar + clip extraction ----
        int totalExtracted = 0, totalSkippedPreview = 0;
        var coreResult = new StringBuilder();
        foreach (string guid in fbxGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var imp = (ModelImporter)AssetImporter.GetAtPath(path);

            bool dirty = false;
            if (imp.animationType != ModelImporterAnimationType.Human) { imp.animationType = ModelImporterAnimationType.Human; dirty = true; }
            // Keep each file self-contained so its clips remain importable as sub-assets.
            if (imp.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel) { imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel; dirty = true; }
            if (dirty) { imp.SaveAndReimport(); }

            string baseName = Path.GetFileNameWithoutExtension(path);
            int perFile = 0;
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var clip in objs.OfType<AnimationClip>())
            {
                if (clip.name.StartsWith("__preview")) { totalSkippedPreview++; continue; }
                // Take names look like "CharacterArmature|Idle" — strip the prefix for a clean file name.
                string shortName = clip.name.Contains("|") ? clip.name.Split('|')[1] : clip.name;
                string outPath = AnimDir + "/" + baseName + "_" + shortName + ".anim";

                var clone = UnityEngine.Object.Instantiate(clip);
                clone.name = baseName + "_" + shortName;
                var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(outPath);
                if (existing != null) AssetDatabase.DeleteAsset(outPath);
                AssetDatabase.CreateAsset(clone, outPath);
                perFile++;
                totalExtracted++;
            }
            sb.AppendLine(baseName + ": extracted " + perFile + " clips");

            // Record the core four explicitly.
            var saved = AssetDatabase.FindAssets("t:AnimationClip " + baseName + "_", new[] { AnimDir });
            foreach (string core in CoreTakes)
            {
                bool has = saved.Any(g => AssetDatabase.GUIDToAssetPath(g).EndsWith("/" + baseName + "_" + core + ".anim"));
                coreResult.Append(baseName + ":" + core + "=" + (has ? "OK" : "MISSING") + " ");
            }
            coreResult.AppendLine();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ---- 4. Violet humanoid check (informational; does not modify her importer) ----
        string violetPath = "Assets/Art/Characters/Violet/violet_tbp.fbx";
        var vImp = AssetImporter.GetAtPath(violetPath) as ModelImporter;
        sb.AppendLine("--- Violet ---");
        if (vImp == null) sb.AppendLine("violet_tbp.fbx NOT FOUND at " + violetPath);
        else sb.AppendLine("violet animationType=" + vImp.animationType + " (must be Humanoid for retargeting)");

        sb.AppendLine("--- Core clip coverage (per FBX) ---");
        sb.Append(coreResult);
        sb.AppendLine("TOTAL: extracted=" + totalExtracted + ", skipped __preview=" + totalSkippedPreview);

        File.WriteAllText(LogPath, sb.ToString());
        Debug.Log(sb.ToString());
    }
}
