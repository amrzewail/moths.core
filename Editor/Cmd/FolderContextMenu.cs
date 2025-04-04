using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Moths.Editor.Cmd
{
    internal static class FolderContextMenu
    {
        [MenuItem("Assets/Open in Command-line", false)]
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
            return obj != null && AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj));
        }
    }
}