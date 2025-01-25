using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace CliffLeeCL
{
    /// <summary>
    /// Build project with menu item and provide method for CI auto build.
    /// </summary>
    public class ProjectBuilder
    {
        [MenuItem("Cliff Lee CL/Build project Default _F5", false, 0)]
        public static void BuildProject()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            string outputPath = "";

            // Handle command line arguments.
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                Dictionary<string, string> commandLineArgumentToValue = ParseCommandLineArgument();

                if (commandLineArgumentToValue.ContainsKey("-outputPath"))
                    outputPath = commandLineArgumentToValue["-outputPath"];
            }

            BuildProject(target, outputPath);
        }

        [MenuItem("Cliff Lee CL/Build project Windowsx32 _F6", false, 1)]
        public static void BuildProjectWindows32()
        {
            BuildProject(BuildTarget.StandaloneWindows);
        }

        [MenuItem("Cliff Lee CL/Build project Windowsx64 _F7", false, 2)]
        public static void BuildProjectWindows64()
        {
            BuildProject(BuildTarget.StandaloneWindows64);
        }

        /// <summary>
        /// Build Unity project.
        /// </summary>
        /// <param name="buildTarget">Target platform.</param>
        /// <param name="outputPath">The output folder path.</param>
        static void BuildProject(BuildTarget buildTarget, string outputPath = "")
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string fileName = PlayerSettings.productName + GetFileExtension(buildTarget);
            string timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
            string fullBuildPath;
            BuildReport error;

            outputPath = (outputPath == "") ? desktopPath : outputPath;
            fullBuildPath = Path.Combine(outputPath, PlayerSettings.productName);
            fullBuildPath = Path.Combine(fullBuildPath, buildTarget.ToString());
            fullBuildPath = Path.Combine(fullBuildPath, timeStamp);
            fullBuildPath = Path.Combine(fullBuildPath, fileName);
            fullBuildPath = fullBuildPath.Replace(@"\", @"\\");
            BuildPlayerOptions buildPlayerOption = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where((s) => s.enabled).Select((s) => s.path).ToArray(),
                locationPathName = fullBuildPath,
                target = buildTarget,
                options = BuildOptions.None
            };

            Debug.Log("Output path: " + outputPath);
            Debug.Log("Product name: " + PlayerSettings.productName);
            Debug.Log("Build target: " + buildTarget.ToString());
            Debug.Log("Time stamp: " + timeStamp);
            Debug.Log("File name: " + fileName);
            Debug.Log("Build project at: " + fullBuildPath);
            error = BuildPipeline.BuildPlayer(buildPlayerOption);
            if (error.summary.result != BuildResult.Succeeded)
                throw new Exception("BuildPlayer failure: " + error);  // Return error code for batch mode to know.
        }

        /// <summary>
        /// Parse command line argument and extract custom command line arguments.
        /// </summary>
        /// <returns>Dictionary about custom command line arguments.</returns>
        static Dictionary<string, string> ParseCommandLineArgument()
        {
            Dictionary<string, string> commandLineArgumentToValue = new Dictionary<string, string>();         
            string[] customCommandLineArgument = { "-outputPath" };
            string[] commandLineArgument = Environment.GetCommandLineArgs();

            for (int i = 0; i < commandLineArgument.Length; i++)
            {
                for (int j = 0; j < customCommandLineArgument.Length; j++) {
                    if (commandLineArgument[i] == customCommandLineArgument[j])
                        commandLineArgumentToValue.Add(customCommandLineArgument[j], commandLineArgument[(i + 1) % commandLineArgument.Length]);
                }
            }

            return commandLineArgumentToValue;
        }

        /// <summary>
        /// Return file extension according to build target.
        /// </summary>
        /// <param name="target">The build target.</param>
        /// <returns>file extension string.</returns>
        static string GetFileExtension(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return ".exe";
                case BuildTarget.StandaloneOSX:
                    return ".app";
                case BuildTarget.StandaloneLinux:
                    return ".x86";
                case BuildTarget.StandaloneLinux64:
                    return ".x86_64";
                case BuildTarget.WebGL:
                    return ".webgl";
                case BuildTarget.Android:
                    return ".apk";
                case BuildTarget.iOS:
                    return ".ipa";
                default:
                    Debug.LogError("No corresponding extension!");
                    return "";
            }
        }
    }
}
