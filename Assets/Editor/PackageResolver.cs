using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

// Small editor utility to resolve/install packages when running Unity in batchmode.
// Usage: -executeMethod PackageResolver.ResolvePackages
public static class PackageResolver
{
    // Simpler approach: fire Client.Resolve() and wait a short fixed time for the
    // package manager to complete. This avoids referencing ResolveRequest types that
    // may not be available in all editor assemblies.
    private static double s_EndTime = 0;

    public static void ResolvePackages()
    {
        Debug.Log("PackageResolver: invoking Client.Resolve()");
        Client.Resolve();
        // wait up to 20 seconds for Package Manager to do its work
        s_EndTime = EditorApplication.timeSinceStartup + 20.0;
        EditorApplication.update += WaitAndExit;
    }

    private static void WaitAndExit()
    {
        if (EditorApplication.timeSinceStartup >= s_EndTime)
        {
            Debug.Log("PackageResolver: timeout reached, exiting.");
            EditorApplication.update -= WaitAndExit;
            EditorApplication.Exit(0);
        }
    }
}
