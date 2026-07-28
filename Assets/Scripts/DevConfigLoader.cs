using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Loads devsettings.json from StreamingAssets at startup.
/// On Android, streamingAssets is inside the APK jar — must use UnityWebRequest, NOT File.ReadAllText.
/// Falls back to hardcoded defaults on any error.
/// </summary>
public class DevConfigLoader : MonoBehaviour
{
    [System.Serializable]
    public class DevConfigData
    {
        public float lookSensitivity = 0.25f;
        public float joystickDeadzone = 0.1f;
        public float cameraDistance = 2.5f;
        public bool invertY = false;
    }

    public PlayerController playerController;

    void Start()
    {
        StartCoroutine(LoadConfig());
    }

    IEnumerator LoadConfig()
    {
        string path;
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Application.streamingAssetsPath + "/devsettings.json";
#else
        path = "file://" + Application.streamingAssetsPath + "/devsettings.json";
#endif
        var req = UnityWebRequest.Get(path);
        yield return req.SendWebRequest();

        DevConfigData data;
        if (req.error == null && req.downloadHandler != null && !string.IsNullOrEmpty(req.downloadHandler.text))
        {
            data = ParseConfig(req.downloadHandler.text);
            Debug.Log($"[DevConfig] Loaded from StreamingAssets: lookSensitivity={data.lookSensitivity}, joystickDeadzone={data.joystickDeadzone}");
        }
        else
        {
            data = new DevConfigData(); // hardcoded defaults
            Debug.Log($"[DevConfig] Using defaults (load failed or empty): lookSensitivity={data.lookSensitivity}, joystickDeadzone={data.joystickDeadzone}");
        }

        ApplyConfig(data);
    }

    /// <summary>
    /// Deterministic parser — fed a JSON string, returns DevConfigData.
    /// Uses JsonUtility (Unity built-in, no external deps).
    /// </summary>
    public static DevConfigData ParseConfig(string json)
    {
        try
        {
            var data = JsonUtility.FromJson<DevConfigData>(json);
            if (data == null) data = new DevConfigData();
            // Validate ranges
            if (data.lookSensitivity <= 0f || data.lookSensitivity > 10f)
                data.lookSensitivity = 0.25f;
            if (data.joystickDeadzone < 0f || data.joystickDeadzone > 1f)
                data.joystickDeadzone = 0.1f;
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[DevConfig] Parse error: {e.Message}, using defaults");
            return new DevConfigData();
        }
    }

    void ApplyConfig(DevConfigData data)
    {
        DevSettings.LookSensitivity = data.lookSensitivity;
        DevSettings.JoystickDeadzone = data.joystickDeadzone;
        DevSettings.CameraDistance = data.cameraDistance;
        DevSettings.InvertY = data.invertY;

        if (playerController != null)
        {
            playerController.lookSensitivity = data.lookSensitivity;
            // Apply camera distance to standing offset
            playerController.standingCameraOffset = new Vector3(0, 1.6f, -data.cameraDistance);
            Debug.Log($"[DevConfig] Applied lookSensitivity={data.lookSensitivity}, cameraDistance={data.cameraDistance}, invertY={data.invertY} to PlayerController");
        }

        var joystick = Object.FindObjectOfType<VirtualJoystick>();
        if (joystick != null)
        {
            Debug.Log($"[DevConfig] Applied joystickDeadzone={data.joystickDeadzone} to VirtualJoystick (via DevSettings)");
        }
    }
}

/// <summary>
/// Static holder for runtime dev settings (read from StreamingAssets/devsettings.json).
/// Parsed by DevConfigLoader, consumed by VirtualJoystick and PlayerController.
/// </summary>
public static class DevSettings
{
    public static float JoystickDeadzone = 0.1f;
    public static float LookSensitivity = 0.25f;
    public static float CameraDistance = 2.5f;
    public static bool InvertY = false;
}
