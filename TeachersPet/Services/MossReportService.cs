using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using TeachersPet.Pages.CourseInfo.CheaterManager;

namespace TeachersPet.Services {
    public class MossReportService {
        
        /// <summary>
        /// Runs a MOSS report on a given directory
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="language"></param>
        /// <param name="numSimilarLines"></param>
        /// <param name="numReports"></param>
        /// <param name="nameOfReport"></param>
        /// <param name="excludedFiles"></param>
        /// <returns>URL to resultant MOSS report</returns>
        public static string RunReportOnDirectory(DirectoryInfo workingDirectory, RunReportPage.MOSSLanguageObject language, int numSimilarLines,
            int numReports, string nameOfReport, List<string> excludedFiles) {
            
            var resultantFiles = WalkDirectories(workingDirectory, excludedFiles, language.AcceptedExtensions);
            File.WriteAllText($"{workingDirectory.FullName}/moss.pl", Properties.Resources.MossPerlScript);
            
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory.FullName,
                Arguments = $"./moss.pl -l {language.LanguageCode} -n {numReports} -m {numSimilarLines} -x -c \"{nameOfReport}\" -d {string.Join(" ", resultantFiles.Select(ConvertPathToWslPath))} | tail -1",
                CreateNoWindow = true,
                FileName = "wsl.exe",
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var proc = Process.Start(startInfo);
            proc?.WaitForExit();
            return proc?.StandardOutput.ReadToEnd();

        }
        
        private static IEnumerable<string> WalkDirectories(DirectoryInfo directory, List<string> excludedFiles, List<string> acceptedExtensions) {

            var pathsOfCorrectFiles = new List<string>();
            foreach (var innerDir in directory.GetDirectories()) {
                if (innerDir.FullName.Contains("__MACOSX")) {
                    continue;
                }
                pathsOfCorrectFiles.AddRange(WalkDirectories(innerDir, excludedFiles, acceptedExtensions).Where(item => !pathsOfCorrectFiles.Contains(item)));
            }

            foreach (var innerFile in directory.GetFiles()) {
                if (Path.GetExtension(innerFile.FullName) == ".zip") {
                    try {
                        ZipFile.ExtractToDirectory(innerFile.FullName, directory.FullName);
                    }
                    catch (Exception) {}

                    File.Delete(innerFile.FullName);
                    pathsOfCorrectFiles.AddRange(WalkDirectories(directory, excludedFiles, acceptedExtensions).Where(item => !pathsOfCorrectFiles.Contains(item)));
                }

                if (excludedFiles.Contains(innerFile.Name)) {
                    continue;
                }

                if (acceptedExtensions.Contains(Path.GetExtension(innerFile.FullName))) {
                    pathsOfCorrectFiles.Add(innerFile.FullName);
                }
            }

            return pathsOfCorrectFiles;
        }

        private List<string> ConvertListOfPathsToWslPaths(IEnumerable<string> windowsPaths) {
            return windowsPaths.Select(ConvertPathToWslPath).ToList();
        }
        
        private static string ConvertPathToWslPath(string windowsPath) {

            var result = windowsPath.Replace('\\', '/');
            var elements = result.Split('/');
            var regex = new Regex("^([A-Za-z]):$", RegexOptions.Compiled);
            if (regex.IsMatch(elements[0])) {
                elements[0] = elements[0].Remove(elements[0].IndexOf(':'));
                //Gotta be lowercase for some reason
                elements[0] = "/mnt/" + elements[0].ToLower();
            }
            return string.Join("/", elements);
        }

    }
}