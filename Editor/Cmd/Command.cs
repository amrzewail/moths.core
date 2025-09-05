using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Moths.Cmd
{
    public static class Command
    {

        public static (string result, string error) Run(string program, string arguments)
        {
            return Run("", program, arguments);
        }

        public static (string result, string error) Run(Object target, string program, string arguments)
        {
            return Run(Path.GetDirectoryName(AssetDatabase.GetAssetPath(target)), program, arguments);
        }


        public static (string result, string error) Run(string directory, string program, string arguments)
        {
            UnityEngine.Debug.Log($"$ git {arguments}");
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), directory.Replace('/', '\\'))
                };

                using (var process = Process.Start(psi))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        UnityEngine.Debug.LogWarning($"command error ({program} {arguments}): {error}");
                        return (null, error);
                    }

                    return (output, error);
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to run command '{program} {arguments}': {ex.Message}");
                return (null, ex.Message);
            }
        }

    }
}