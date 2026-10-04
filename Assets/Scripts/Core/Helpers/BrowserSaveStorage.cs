using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Core.Helpers
{
    // Web-only synchronous backup protects saves when Safari suspends the page.
    public static class BrowserSaveStorage
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern string LocalMergeReadSave(string key);
        [DllImport("__Internal")] private static extern void LocalMergeWriteSave(string key, string value);
#endif

        public static string ReadFile(string path)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var backup = LocalMergeReadSave(Path.GetFileName(path));
            if (backup != null) return backup;
#endif
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public static void WriteFile(string path, string value)
        {
            File.WriteAllText(path, value);
#if UNITY_WEBGL && !UNITY_EDITOR
            LocalMergeWriteSave(Path.GetFileName(path), value);
#endif
        }

        public static string ReadCompletedTasks()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var backup = LocalMergeReadSave("CompletedTasks");
            if (backup != null) return backup;
#endif
            return PlayerPrefs.HasKey("CompletedTasks") ? PlayerPrefs.GetString("CompletedTasks") : null;
        }

        public static void WriteCompletedTasks(string value)
        {
            PlayerPrefs.SetString("CompletedTasks", value);
            PlayerPrefs.Save();
#if UNITY_WEBGL && !UNITY_EDITOR
            LocalMergeWriteSave("CompletedTasks", value);
#endif
        }
    }
}
