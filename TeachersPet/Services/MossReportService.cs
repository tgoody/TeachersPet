using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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
        /// <param name="useSeparateConsole"></param>
        /// <returns>URL to resultant MOSS report</returns>
        public static Process RunReportOnDirectory(DirectoryInfo workingDirectory,
            RunReportPage.MOSSLanguageObject language, int numSimilarLines,
            int numReports, string nameOfReport, List<string> excludedFiles, bool useSeparateConsole) {

            var resultantFiles = WalkDirectories(workingDirectory, excludedFiles, language.AcceptedExtensions);
            File.WriteAllText($"{workingDirectory.FullName}/moss.pl", Properties.Resources.MossPerlScript);
            var resultantFilesList = resultantFiles.Select(ConvertPathToWslPath).Select(path =>
                path.Replace($"{ConvertPathToWslPath(workingDirectory.FullName)}/", "")).ToList();

            var proc = Process.Start(CreateStartInfo(workingDirectory, useSeparateConsole));

            //WITHOUT THIS EVERYTHING WILL BREAK BECAUSE WINDOWS USES \r\n FOR RETURNS
            proc.StandardInput.NewLine = "\n";
            proc.StandardInput.WriteLine($"results=()");
            foreach (var file in resultantFilesList) {
                proc.StandardInput.WriteLine($"results+=( \"{file}\" )");
            }
            proc.StandardInput.WriteLine($"./moss.pl -l {language.LanguageCode} -n {numReports} -m {numSimilarLines} -x -d -c \"{nameOfReport}\" ${{results[@]}}");


            if (useSeparateConsole) { 
                proc.StandardInput.WriteLine("echo \"All done, feel free to close this window after getting the link.\"");
                proc.StandardInput.WriteLine("sleep 600");
            }
            
            return proc;
        }

        private static ProcessStartInfo CreateStartInfo(DirectoryInfo workingDirectory, bool separateConsole) {

            ProcessStartInfo startInfo;
            if (separateConsole) {
                startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = workingDirectory.FullName,
                    CreateNoWindow = false,
                    FileName = "wsl.exe",
                    RedirectStandardInput = true,
                    RedirectStandardError = false,
                    RedirectStandardOutput = false,
                    UseShellExecute = false
                };
            }
            else {
                startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = workingDirectory.FullName,
                    CreateNoWindow = true,
                    FileName = "wsl.exe",
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
            }
            return startInfo;
        }
        
        private static IEnumerable<string> WalkDirectories(DirectoryInfo directory, List<string> excludedFiles, List<string> acceptedExtensions) {

            var pathsOfCorrectFiles = new List<string>();
            var dirs = directory.GetDirectories();
            for (var index = 0; index < dirs.Length; index++) {
                var innerDir = dirs[index];

                //removing spaces
                if (innerDir.Name.Contains(' ')) {
                    var newName = innerDir.Name.Replace(' ', '_');
                    Directory.Move(innerDir.FullName, $"{directory.FullName}/{newName}");
                    innerDir = new DirectoryInfo($"{directory.FullName}/{newName}");
                }

                var potentialFixedName = Regex.Replace(innerDir.Name, $"[^A-Za-z0-9-_.]", "");
                if (innerDir.Name != potentialFixedName) {
                    Directory.Move(innerDir.FullName, $"{directory.FullName}/{potentialFixedName}");
                    innerDir = new DirectoryInfo($"{directory.FullName}/{potentialFixedName}");
                }

                if (innerDir.FullName.Contains("__MACOSX") || innerDir.FullName.ToLower().Contains("cmake")) {
                    continue;
                }

                pathsOfCorrectFiles.AddRange(WalkDirectories(innerDir, excludedFiles, acceptedExtensions)
                    .Where(item => !pathsOfCorrectFiles.Contains(item)));
            }

            var files = directory.GetFiles();
            for (var index = 0; index < files.Length; index++) {
                var innerFile = files[index];
                if (innerFile.FullName.ToLower().Contains("cmake")) {
                    continue;
                }

                //removing spaces
                if (innerFile.Name.Contains(' ')) {
                    var newName = innerFile.Name.Replace(' ', '_');
                    File.Move(innerFile.FullName, $"{directory.FullName}/{newName}");
                    innerFile = new FileInfo($"{directory.FullName}/{newName}");
                }

                var potentialFixedName = Regex.Replace(innerFile.Name, $"[^A-Za-z0-9-_\\.]", "");
                if (innerFile.Name != potentialFixedName) {
                    File.Move(innerFile.FullName, $"{directory.FullName}/{potentialFixedName}");
                    innerFile = new FileInfo($"{directory.FullName}/{potentialFixedName}");
                }

                if (Path.GetExtension(innerFile.FullName) == ".zip") {
                    var tempDir = directory;
                    try {
                        tempDir = Directory.CreateDirectory(
                            $"{directory.FullName}/{Path.GetFileNameWithoutExtension(innerFile.Name)}");
                        ZipFile.ExtractToDirectory(innerFile.FullName, tempDir.FullName);
                    }
                    catch (Exception) {
                    }

                    File.Delete(innerFile.FullName);
                    pathsOfCorrectFiles.AddRange(WalkDirectories(tempDir, excludedFiles, acceptedExtensions)
                        .Where(item => !pathsOfCorrectFiles.Contains(item)));
                }


                if (excludedFiles.Select(file => file.ToLower()).Contains(innerFile.Name.ToLower())) {
                    continue;
                }

                if (acceptedExtensions.Contains(Path.GetExtension(innerFile.FullName))) {
                    pathsOfCorrectFiles.Add(innerFile.FullName);
                }
            }

            return pathsOfCorrectFiles;
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