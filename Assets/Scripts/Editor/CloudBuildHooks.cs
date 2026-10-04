using System;
using System.IO;
using System.Linq;
using Core.GridPawns.Data;
using Core.Tasks;
using UnityEditor;
using UnityEditor.Build;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEngine;

namespace LocalMerge.Editor
{
    // Unity Build Automation's documented pre-/post-export callbacks.
    // No Apple or Unity credentials are stored in the project.
    public static class CloudBuildHooks
    {
        public static void PreExport()
        {
            var bundle = Environment.GetEnvironmentVariable("IOS_BUNDLE_ID") ?? "com.localmerge.sandbox";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundle);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(BuildTargetGroup.iOS, 1);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.requiresFullScreen = true;
            var producers = Resources.LoadAll<ProducerDataSO>("Data/ProducerData/Producers");
            var appliances = Resources.LoadAll<ApplianceDataSO>("Data/ApplianceData/Appliances");
            var tasks = Resources.LoadAll<TaskSO>("Tasks");
            if (producers.Length != 1 || appliances.Length != 1 || tasks.Length != 12)
                throw new BuildFailedException("Required producer, appliance or order resources are missing.");
            if (tasks.Any(t => t.Goals.Any(g => g.Level < 1 || g.Level > 11)))
                throw new BuildFailedException("An order goal references an unavailable appliance level.");
            Debug.Log("Cloud pre-export checks passed; Play Mode gameplay tests are a separate validation step.");
        }

        public static void PostExport(string exportPath)
        {
#if UNITY_IOS
            var path = Path.Combine(exportPath, "Info.plist");
            var info = new PlistDocument();
            info.ReadFromFile(path);
            info.root.SetBoolean("UIRequiresFullScreen", true);
            foreach (var key in new[] { "UISupportedInterfaceOrientations", "UISupportedInterfaceOrientations~ipad" })
                info.root.CreateArray(key).AddString("UIInterfaceOrientationPortrait");
            info.WriteToFile(path);
#endif
        }
    }
}
