using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace LocalMerge.Editor
{
    public static class IosBuild
    {
        [MenuItem("Tools/Local Merge/Export iOS Xcode Project")]
        public static void Export()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                throw new BuildFailedException("Install the official Unity iOS Build Support module first.");
            PlayerSettings.companyName = "LocalMerge";
            PlayerSettings.productName = "Merge Sandbox";
            var bundle = Environment.GetEnvironmentVariable("IOS_BUNDLE_ID") ?? "com.localmerge.sandbox";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundle);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1); // ARM64
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.requiresFullScreen = true;
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            EditorUserBuildSettings.symlinkSources = false; // Export is copied to a separate macOS runner.
            var path = Path.GetFullPath(Environment.GetEnvironmentVariable("IOS_BUILD_PATH") ?? "Builds/iOS");
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length != 3 || !scenes[0].EndsWith("LoadScene.unity"))
                throw new BuildFailedException("The bootstrap, main and merge scenes must be enabled in order.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes, locationPathName = path, target = BuildTarget.iOS,
                options = BuildOptions.StrictMode
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"iOS export failed: {report.summary.result}, {report.summary.totalErrors} errors");
#if UNITY_IOS
            var projectPath = PBXProject.GetPBXProjectPath(path);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            var app = project.GetUnityMainTargetGuid();
            var framework = project.GetUnityFrameworkTargetGuid();
            var team = Environment.GetEnvironmentVariable("APPLE_TEAM_ID");
            var profile = Environment.GetEnvironmentVariable("IOS_PROFILE_UUID");
            project.SetBuildProperty(framework, "CODE_SIGNING_ALLOWED", "NO");
            if (!string.IsNullOrEmpty(team) && !string.IsNullOrEmpty(profile))
            {
                project.SetBuildProperty(app, "CODE_SIGN_STYLE", "Manual");
                project.SetBuildProperty(app, "DEVELOPMENT_TEAM", team);
                project.SetBuildProperty(app, "PROVISIONING_PROFILE_SPECIFIER", profile);
                project.SetBuildProperty(app, "CODE_SIGN_IDENTITY", "Apple Development");
            }
            else project.SetBuildProperty(app, "CODE_SIGNING_ALLOWED", "NO");
            project.WriteToFile(projectPath);
            // Fixed portrait avoids half-screen layouts; no camera/mic/contacts permissions.
            var infoPath = Path.Combine(path, "Info.plist");
            var info = new PlistDocument();
            info.ReadFromFile(infoPath);
            info.root.SetBoolean("UIRequiresFullScreen", true);
            foreach (var key in new[] { "UISupportedInterfaceOrientations", "UISupportedInterfaceOrientations~ipad" })
                info.root.CreateArray(key).AddString("UIInterfaceOrientationPortrait");
            info.WriteToFile(infoPath);
#endif
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/unity-export-result.json", "{\"succeeded\":true,\"target\":\"iOS\"}");
        }
    }
}
