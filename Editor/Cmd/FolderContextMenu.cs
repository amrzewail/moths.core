using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Moths.Editor.Cmd
{
    internal static class FolderContextMenu
    {
        [MenuItem("Assets/Command-line/Open Project in Command-line", false, priority = 1000)]
        private static void OpenProjectCmd()
        {
            var path = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets/".Length);
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/K cd /d \"{path}\"",
                UseShellExecute = true
            });
        }

        [MenuItem("Assets/Command-line/Open in Command-line", false, priority = 1001)]
        private static void OpenCmd()
        {
            var path = GetSelectedFolderPath();
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/K cd /d \"{path}\"",
                UseShellExecute = true
            });
        }

        private static string GetSelectedFolderPath()
        {
            var obj = Selection.activeObject;
            var path = AssetDatabase.GetAssetPath(obj);
            path = Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(path))
            {
                path = "";
            }
            else
            {
                path = path.Substring("Assets".Length);
            }
            return Application.dataPath + path;
        }

        [MenuItem("Assets/Open in Command-line", true)]
        private static bool ValidateOpenCmd()
        {
            var obj = Selection.activeObject;
            return obj != null;
        }
    }
}