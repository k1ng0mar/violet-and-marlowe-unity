using UnityEditor;
using System.Reflection;
using UnityEngine;
using System.Linq;

public static class InstallAndroidModule
{
    public static void Install()
    {
        Debug.Log("[INSTALL] Attempting Android Build Support installation...");
        
        var assembly = typeof(EditorApplication).Assembly;
        var quickInstallType = assembly.GetType("UnityEditor.QuickInstallModule");
        
        if (quickInstallType != null)
        {
            Debug.Log("[INSTALL] Found QuickInstallModule");
            
            foreach (var m in quickInstallType.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                Debug.Log($"[INSTALL] Method: {m.Name} ({string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name))})");
            }
            
            var installMethod = quickInstallType.GetMethod("Install", new[] { typeof(string) });
            if (installMethod != null)
            {
                Debug.Log("[INSTALL] Calling QuickInstallModule.Install(android)");
                object result = installMethod.Invoke(null, new object[] { "android" });
                Debug.Log($"[INSTALL] Result type: {result?.GetType().Name ?? "null"}");
                
                // If it returns a Task or similar, wait for it
                var taskWaitProp = result?.GetType().GetProperty("WaitForCompletion");
                if (taskWaitProp != null)
                {
                    var waitMethod = taskWaitProp.GetValue(result, null);
                    if (waitMethod is System.Func<bool> waitFunc)
                    {
                        bool completed = waitFunc();
                        Debug.Log($"[INSTALL] Wait completed: {completed}");
                    }
                }
            }
        }
        else
        {
            Debug.LogError("[INSTALL] QuickInstallModule NOT found");
        }
        
        Debug.Log("[INSTALL] Done.");
    }
}
