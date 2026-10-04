using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LocalMerge.Editor
{
    public static class WebBuild
    {
        [MenuItem("Tools/Local Merge/Export Offline Web Candidate")]
        public static void Export()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new BuildFailedException("Install the official WebGL Build Support module first.");
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.WebGL, ScriptingImplementation.IL2CPP);
            PlayerSettings.WebGL.template = "PROJECT:OfflineMerge";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.dataCaching = false; // Service worker owns the complete offline bundle.
            PlayerSettings.WebGL.initialMemorySize = 64;
            PlayerSettings.WebGL.maximumMemorySize = 512;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Linear;
            PlayerSettings.WebGL.linearMemoryGrowthStep = 16;
            var output = Path.GetFullPath(Environment.GetEnvironmentVariable("WEB_BUILD_PATH") ?? "Builds/Web");
            var orientation = PlayerSettings.defaultInterfaceOrientation;
            BuildReport report;
            try
            {
                // Safari cannot lock orientation; the page keeps a portrait play area.
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                    locationPathName = output, target = BuildTarget.WebGL, options = BuildOptions.StrictMode
                });
            }
            finally { PlayerSettings.defaultInterfaceOrientation = orientation; }
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"Web export failed: {report.summary.result}, {report.summary.totalErrors} errors");
            File.WriteAllText("Builds/unity-web-result.json", "{\"succeeded\":true,\"target\":\"WebGL\"}");
        }
    }
}
