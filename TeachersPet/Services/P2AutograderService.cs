using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TeachersPet.Services {
    public class P2AutograderService {

        private class StudentData {
            public string name = "";
            public int score = 0;
            public bool namingError = false;
            public bool containsMakefile = true;
            public List<string> logs = new List<string>();
        }

        private string totalSubmissionZipPath;
        private string modulePath;
        private string studentFolderDirectory;
        private string exampleFolderDirectory;
        private string inputFolderDirectory;
        private ConcurrentDictionary<string, StudentData> studentData = new ConcurrentDictionary<string, StudentData>();
        
        public void Init(string pathToModuleDirectory, string pathToSubmissionsZip) {
            totalSubmissionZipPath = pathToSubmissionsZip;
            modulePath = pathToModuleDirectory;
        }
        public async void RunSetup() {
            var studentZipDirectory = Directory.CreateDirectory($"{modulePath}/StudentZips");
            Empty(studentZipDirectory);
            ZipFile.ExtractToDirectory(totalSubmissionZipPath, studentZipDirectory.FullName, true);
            // "/Resources/P2AutograderGradingExecutable/GradingExec"
            studentFolderDirectory = await UnzipStudentZipsToFolders(studentZipDirectory.FullName);
            VerifyMakefiles();
            //await MakeStudentsSubmissions();
            await RunStudentsSubmissions();
        }

        private async Task<string> UnzipStudentZipsToFolders(string studentZipDirectory) {
            var studentFolderDirectory = Directory.CreateDirectory($"{modulePath}/StudentFolders");
            Empty(studentFolderDirectory);
            var studentSubmissionZips = Directory.GetFiles(studentZipDirectory);
            var tasks = new List<Task>();
            var regex = new Regex(@".*\.*\..*project2.*\.zip");
            foreach (var zip in studentSubmissionZips) {
                tasks.Add(Task.Run(() => {TryToExtractStudentZipToStudentFolder(zip, regex, studentFolderDirectory.FullName);}));
            }
            //Waiting on all unzips
            await Task.WhenAll(tasks);
            return studentFolderDirectory.FullName;
        }
        
        private void TryToExtractStudentZipToStudentFolder(string studentZip, Regex zipPattern, string studentFolderDirectory) {

            var zipName = Path.GetFileName(studentZip).ToLower();
            //remove excess canvas data from submission
            zipName = zipName.Substring(zipName.LastIndexOf('_') + 1);
            StudentData temp;
            var studentFolder = "";
            if (!zipPattern.IsMatch(zipName)) {
                var fileName = Path.GetFileName(studentZip);
                var studentName = fileName.Substring(0, fileName.IndexOf('_')); 
                temp = new StudentData{
                    name = studentName,
                    namingError = true
                };
            }
            else {
                var splitZipName = zipName.Split('.');
                var studentName = splitZipName[0] + "." + splitZipName[1];
                temp = new StudentData {
                    name = studentName,
                };
            }
            var realName = temp.name;
            var failCounter = 0;
            while(!studentData.TryAdd(temp.name, temp)) {
                temp.name = realName + $"-{++failCounter}";
                Thread.Sleep(250);
            }
            
            studentFolder = Directory.CreateDirectory($"{studentFolderDirectory}/{temp.name}").FullName;
            try {
                ZipFile.ExtractToDirectory(studentZip, studentFolder, true);
            }
            catch (Exception) {
                Console.Error.WriteLine($"{studentZip} is an invalid zip file");
            }
        }
        
        private void VerifyMakefiles() {
            var dirInfo = new DirectoryInfo(studentFolderDirectory);
            foreach (var directory in dirInfo.GetDirectories()) {
                if (!(directory.GetFiles().Select(fileInfo => fileInfo.Name).Contains("makefile") ||
                      directory.GetFiles().Select(fileInfo => fileInfo.Name).Contains("Makefile"))) {
                    
                    StudentData temp;
                    studentData.TryGetValue(directory.Name, out temp);
                    StudentData oldtemp = temp;
                    temp.containsMakefile = false;
                    studentData.TryUpdate(directory.Name, temp, oldtemp);
                }
            }
        }
        
        private async Task MakeStudentsSubmissions() {
            
            var dirInfo = new DirectoryInfo(studentFolderDirectory);
            var tasks = new List<Task>();
            foreach (var dir in dirInfo.GetDirectories()) {
                tasks.Add(Task.Run(() => {
                    studentData.TryGetValue(dir.Name, out var temp);
                    if (!temp.containsMakefile) {
                        return;
                    }
                    var oldtemp = temp;
                    var startInfo = new ProcessStartInfo
                    {
                        WorkingDirectory = dir.FullName,
                        Arguments = "/C wsl make",
                        CreateNoWindow = true,
                        FileName = "cmd.exe",
                        RedirectStandardInput = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    var proc = Process.Start(startInfo);
                    proc?.WaitForExit();
                    var outputNormal = proc?.StandardOutput.ReadToEnd();
                    var outputError = proc?.StandardError.ReadToEnd();
                    temp.logs.Add(outputNormal);
                    temp.logs.Add(outputError);
                    studentData.TryUpdate(dir.Name, temp, oldtemp);
                }));
            }
            await Task.WhenAll(tasks);
        }
        
        private async Task RunStudentsSubmissions() {
            
            var dirInfo = new DirectoryInfo(studentFolderDirectory);
            foreach (var dir in dirInfo.GetDirectories()) {

                File.Copy("/Resources/P2AutograderResources/examples.zip", dir.FullName + "/examples.zip");
                File.Copy("/Resources/P2AutograderResources/input.zip", dir.FullName + "/examples.zip");

            }

        }
        
        private static void Empty(DirectoryInfo directory)
        {
            foreach (var file in directory.GetFiles()) {
                try {
                    file.Delete();
                }
                catch (Exception) {
                    // ignored
                }
            }

            foreach (var subDirectory in directory.GetDirectories()) {
                try {
                    subDirectory.Delete(true);
                }
                catch (Exception) {
                    // ignored
                }
            }
        }
        
    }
}