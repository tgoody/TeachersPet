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
            public bool namingError = false;
            public bool containsMakefile = true;
            public List<string> logs = new List<string>();
            public List<string> errorLogs = new List<string>();
            public string graderOutput;
            public string finalGraderLine;
        }

        private static class UnzipHelper {
            public static void Unzip(string sourceName, string targetDirectory, bool overwrite = true) {

                ZipFile.ExtractToDirectory(sourceName, targetDirectory, overwrite);
                var dirInfo = new DirectoryInfo(targetDirectory);
                var files = dirInfo.GetDirectories();
                //TODO: find a better way to filter variably. Handling this because this is the most common.
                if (files.Any(file => file.Name.Contains("__MACOSX"))) {
                    Directory.Delete(files.Single(file => file.Name.Contains("__MACOSX")).FullName, true);
                }

                files = dirInfo.GetDirectories();
                if (files.Length != 1 || dirInfo.GetFiles().Length != 0) return;
                var innerDir = new DirectoryInfo(files[0].FullName);

                foreach (var innerFile in innerDir.GetFiles()) {
                    File.Move(innerFile.FullName, targetDirectory + $"/{innerFile.Name}");
                }

                foreach (var subDir in innerDir.GetDirectories()) {
                    Directory.Move(subDir.FullName, targetDirectory + $"/{subDir.Name}");
                }

                Directory.Delete(innerDir.FullName, true);
            }
        }

        private readonly string p2GraderName = "P2GraderExec";
        private int taskScore = 9;
        private int ecScore = 10;
        private string totalSubmissionZipPath;
        private string modulePath;
        private string studentFolderDirectory;
        private string exampleFolderDirectory;
        private string inputFolderDirectory;
        private int numStudentFolders;
        private int numProjectsMade;
        private int numFoldersImagesCopiedTo;
        private int numStudentsGraded;
        private ConcurrentDictionary<string, StudentData> studentData = new ConcurrentDictionary<string, StudentData>();

        //Getters
        public string ExampleFolderDirectory => exampleFolderDirectory;
        public string InputFolderDirectory => inputFolderDirectory;
        public string ModulePath => modulePath;
        public int NumStudentFolders => numStudentFolders;
        public int NumProjectsMade => numProjectsMade;
        public int NumFoldersImagesCopiedTo => numFoldersImagesCopiedTo;
        public int NumStudentsGraded => numStudentsGraded;

        public int TaskScore {
            get => taskScore;
            set => taskScore = value;
        }

        public int EcScore {
            get => ecScore;
            set => ecScore = value;
        }

        public void Init(string pathToModuleDirectory, string pathToSubmissionsZip) {
            totalSubmissionZipPath = pathToSubmissionsZip;
            modulePath = pathToModuleDirectory;
            Empty(new DirectoryInfo(modulePath));
        }

        public async Task RunSetup() {
            var studentZipDirectory = Directory.CreateDirectory($"{modulePath}/StudentZips");
            Empty(studentZipDirectory);
            UnzipHelper.Unzip(totalSubmissionZipPath, studentZipDirectory.FullName, true);
            BuildGraderCpp();
            studentFolderDirectory = await UnzipStudentZipsToFolders(studentZipDirectory.FullName);
            VerifyMakefiles();
            await MakeStudentsSubmissions();
        }

        public void SetExamplesDirectory(string pathToDirectory) {
            var extension = Path.GetExtension(pathToDirectory);
            exampleFolderDirectory = Directory.CreateDirectory($"{modulePath}/examples").FullName;
            Empty(new DirectoryInfo(exampleFolderDirectory));
            if (extension == ".zip") {
                UnzipHelper.Unzip(pathToDirectory, exampleFolderDirectory);
            }
            else if (extension.Length == 0) {
                var dirInfo = new DirectoryInfo(pathToDirectory);
                foreach (var image in dirInfo.GetFiles()) {
                    File.Copy(image.FullName, exampleFolderDirectory + $"/{image.Name}");
                }
            }
            else {
                throw new Exception("Bad examples folder");
            }
        }

        public void SetInputDirectory(string pathToDirectory) {
            var extension = Path.GetExtension(pathToDirectory);
            inputFolderDirectory = Directory.CreateDirectory($"{modulePath}/input").FullName;
            Empty(new DirectoryInfo(inputFolderDirectory));
            if (extension == ".zip") {
                UnzipHelper.Unzip(pathToDirectory, inputFolderDirectory);
            }
            else if (extension.Length == 0) {
                var dirInfo = new DirectoryInfo(pathToDirectory);
                foreach (var image in dirInfo.GetFiles()) {
                    File.Copy(image.FullName, inputFolderDirectory + $"/{image.Name}");
                }
            }
            else {
                throw new Exception("Bad examples folder");
            }
        }

        public void CopyImagesToStudentFolders() {

            var studentFolderDirInfo = new DirectoryInfo(studentFolderDirectory);
            var inputFolderDirInfo = new DirectoryInfo(inputFolderDirectory);
            var exampleFolderDirInfo = new DirectoryInfo(exampleFolderDirectory);

            foreach (var studentFolder in studentFolderDirInfo.GetDirectories()) {

                Directory.CreateDirectory($"{studentFolder.FullName}/input");
                Directory.CreateDirectory($"{studentFolder.FullName}/output");
                Directory.CreateDirectory($"{studentFolder.FullName}/examples");
                foreach (var image in inputFolderDirInfo.GetFiles()) {
                    File.Copy(image.FullName, studentFolder.FullName + $"/input/{image.Name}");
                }

                foreach (var image in exampleFolderDirInfo.GetFiles()) {
                    File.Copy(image.FullName, studentFolder.FullName + $"/examples/{image.Name}");
                }

                numFoldersImagesCopiedTo++;
            }
        }

        public void RunGrader() {
            var studentFolderDirInfo = new DirectoryInfo(studentFolderDirectory);
            var directories = studentFolderDirInfo.GetDirectories();
            Parallel.ForEach(directories, studentFolder => {
                    numStudentsGraded++;
                    //no makefile, return, also get student object to store logs in later
                    studentData.TryGetValue(studentFolder.Name, out var studentObject);
                    if (!studentObject.containsMakefile) {
                        return;
                    }

                    //find a .out executable
                    var studentExec = studentFolder.GetFiles().SingleOrDefault(file => file.Name.EndsWith(".out"));

                    //if they didn't name the executable right
                    if (studentExec == null || !studentExec.Exists) {
                        //get makefile
                        var makefilePath = studentFolder.GetFiles()
                            .Single(file => file.Name.ToLower() == "makefile").FullName;
                        //read data of makefile
                        var makefileString = new StreamReader(makefilePath).ReadToEnd();
                        //split all elements of makefile up (on whitespace)
                        var elementsOfMakefile = makefileString.Split();
                        var currIndex = 0;
                        //find all -o flags, see if any of the names after them exist as files in the directory
                        while (studentExec == null || !studentExec.Exists) {
                            try {
                                currIndex = elementsOfMakefile.ToList().FindIndex(currIndex, s => s == "-o");
                                //return if no more found
                                if (currIndex < 0) {
                                    throw new Exception("No index found");
                                }

                                if (studentFolder.GetFiles()
                                    .Any(file => file.Name == elementsOfMakefile[++currIndex])) {
                                    studentExec = studentFolder.GetFiles()
                                        .Single(file => file.Name == elementsOfMakefile[currIndex]);
                                }
                            }
                            catch (Exception) {
                                studentObject.errorLogs.Add("No executable found to run, compilation error?");
                                return;
                            }
                        }
                    }

                    var startInfo = new ProcessStartInfo {
                        WorkingDirectory = studentFolder.FullName,
                        Arguments = $"timeout 30s ./{studentExec.Name} | head -n200",
                        CreateNoWindow = true,
                        FileName = "wsl.exe",
                        RedirectStandardInput = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = false,
                        UseShellExecute = false
                    };
                    //Console.WriteLine($"Running {studentFolder.Name}");
                    var proc = Process.Start(startInfo);
                    Thread.Sleep(2000);
                    while (!proc.HasExited) {
                        var processesWithName = Process.GetProcessesByName(studentExec.Name);
                        processesWithName.Where(p => p.PrivateMemorySize64 > 2147483648).ToList().ForEach(p => {
                            Console.Error.WriteLine($"Killing process {p.ProcessName}");
                            try {
                                p.Kill();
                            }
                            catch (InvalidOperationException) {
                            }
                        });
                        Thread.Sleep(2000);
                    }

                    var outputError = proc?.StandardError.ReadToEnd();
                    //store logs in student data
                    studentObject.errorLogs.Add(outputError);

                    var outputInfo = new DirectoryInfo($"{studentFolder.FullName}/output");
                    var filesCreated = outputInfo.GetFiles().Aggregate("STUDENT FILES CREATED:\n",
                        (current, file) => current + (file.Name + "\n"));
                    studentObject.logs.Add(filesCreated);

                    //swap files for error checking, see P2Grader.cpp
                    File.Move($"{studentFolder.FullName}/examples/EXAMPLE_part2.tga",
                        $"{studentFolder.FullName}/examples/EXAMPLE_part2old.tga");
                    File.Copy($"{studentFolder.FullName}/examples/EXAMPLE_part3.tga",
                        $"{studentFolder.FullName}/examples/EXAMPLE_part2.tga", true);

                    File.Copy($"{modulePath}/{p2GraderName}", $"{studentFolder.FullName}/{p2GraderName}");
                    startInfo = new ProcessStartInfo {
                        WorkingDirectory = studentFolder.FullName,
                        Arguments = $"/C wsl ./{p2GraderName} {taskScore} {ecScore}",
                        CreateNoWindow = true,
                        FileName = "cmd.exe",
                        RedirectStandardInput = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false
                    };
                    var graderProc = Process.Start(startInfo);
                    graderProc?.WaitForExit();
                    Console.WriteLine($"Finished grading {studentFolder.Name}");
                    var graderOutputNormal = graderProc?.StandardOutput.ReadToEnd();
                    studentObject.graderOutput = graderOutputNormal;
                    //getting last line from output and storing it for text file
                    studentObject.finalGraderLine =
                        graderOutputNormal?.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None)[^2];
                    Directory.Delete($"{studentFolder.FullName}/input", true);
                    Directory.Delete($"{studentFolder.FullName}/examples", true);
                    Directory.Delete($"{studentFolder.FullName}/output", true);
                });

            try {
                WriteOutputFiles();
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message);
            }
        }

        private void WriteOutputFiles() {
            
            //Handling score text file
            
            var writer = new StreamWriter($"{modulePath}/StudentScores.txt");
            var kvpList = studentData.OrderBy(kvp => kvp.Key).ToList();
            foreach (var (key, value) in kvpList) {
                writer.WriteLine("############");
                writer.WriteLine($"Student: {key} -- {value.finalGraderLine}");
                if (!value.containsMakefile) {
                    writer.WriteLine("No makefile! Couldn't compile");
                }
                if (value.namingError) {
                    writer.WriteLine("Student submitted their zip file but didn't follow naming instructions");
                }
                writer.WriteLine();
            }
            writer.Close();
            
            //Handling logs text file
            
            writer = new StreamWriter($"{modulePath}/StudentLogs.txt");
            foreach (var (key, value) in kvpList) {
                writer.WriteLine("############");
                writer.WriteLine($"Student: {key}");
                var validErrorMessages = value.errorLogs.Where(s => s.Length > 0).ToList();
                if (validErrorMessages.Count > 0) {
                    writer.WriteLine("Errors:");
                    validErrorMessages.ForEach(error => writer.WriteLine(error));
                    writer.WriteLine();
                }
                writer.WriteLine("Console output:");
                value.logs.ForEach(log => {
                    if (log.Length > 0) {
                        writer.WriteLine(log);
                    }
                });
                writer.WriteLine();
                writer.WriteLine(value.graderOutput ?? "No grader output");
                writer.WriteLine();
            }
            writer.Close();
            
        }
        
        private async Task<string> UnzipStudentZipsToFolders(string studentZipDirectory) {
            var studentFolder = Directory.CreateDirectory($"{modulePath}/StudentFolders");
            Empty(studentFolder);
            studentFolderDirectory = studentFolder.FullName;
            var studentSubmissionZips = Directory.GetFiles(studentZipDirectory);
            var tasks = new List<Task>();
            var regex = new Regex(@".*\.*\..*project2.*\.zip");
            foreach (var zip in studentSubmissionZips) {
                tasks.Add(Task.Run(() => {TryToExtractStudentZipToStudentFolder(zip, regex);}));
            }
            //Waiting on all unzips
            await Task.WhenAll(tasks);
            await ClearStudentFolders();
            return studentFolderDirectory;
        }
        
        private void TryToExtractStudentZipToStudentFolder(string studentZip, Regex zipPattern) {

            var zipName = Path.GetFileName(studentZip).ToLower();
            //remove excess canvas data from submission
            zipName = zipName.Substring(zipName.LastIndexOf('_') + 1);
            StudentData temp;
            var studentFolder = "";
            if (!zipPattern.IsMatch(zipName)) {
                var fileName = Path.GetFileName(studentZip);
                var studentName = fileName.Substring(0, fileName.IndexOf('_')).Trim(); 
                temp = new StudentData{
                    name = studentName,
                    namingError = true
                };
            }
            else {
                var splitZipName = zipName.Split('.');
                var studentName = (splitZipName[0] + "." + splitZipName[1]).Trim();
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
                UnzipHelper.Unzip(studentZip, studentFolder);
            }
            catch (Exception) {
                Console.Error.WriteLine($"{studentZip} is an invalid zip file");
            }

            numStudentFolders++;
        }

        private async Task ClearStudentFolders() {
            var dirInfo = new DirectoryInfo(studentFolderDirectory);
            var tasks = new List<Task>();
            foreach (var dir in dirInfo.GetDirectories()) {
                tasks.Add(Task.Run(() => {
                    try {
                        foreach (var file in dir.GetFiles()) {
                            if (Path.GetExtension(file.FullName) != ".cpp" &&
                                Path.GetExtension(file.FullName) != ".h" &&
                                Path.GetExtension(file.FullName) != ".hpp" &&
                                file.Name.ToLower() != "makefile") {
                                File.Delete(file.FullName);
                            }
                        }

                        foreach (var file in dir.GetDirectories()) {
                            if (file.Name != "src") {
                                Directory.Delete(file.FullName, true);
                            }
                        }
                    }
                    catch (Exception) {
                        //ignored
                    }
                }));
            }
            await Task.WhenAll(tasks);
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
            Parallel.ForEach(dirInfo.GetDirectories(), dir =>
            {
                studentData.TryGetValue(dir.Name, out var temp);
                if (!temp.containsMakefile) {
                    return;
                }
                Console.WriteLine($"Making {temp.name}");
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
                var proc = new Process {StartInfo = startInfo};
                proc.ErrorDataReceived += (s, e) => temp.errorLogs.Add(e.Data);
                proc.OutputDataReceived += (s, e) => temp.logs.Add(e.Data);
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                studentData.TryUpdate(dir.Name, temp, oldtemp);
                Interlocked.Increment(ref numProjectsMade);

            });
        }

        private void BuildGraderCpp() {
            File.WriteAllText($"{modulePath}/P2Grader.cpp",Properties.Resources.P2Grader);
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = $"{modulePath}",
                Arguments = $"/C wsl g++ P2Grader.cpp -o {p2GraderName} -std=c++11",
                CreateNoWindow = true,
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var proc = Process.Start(startInfo);
            proc?.WaitForExit();
            Console.WriteLine(proc?.StandardOutput.ReadToEnd());
            Console.Error.WriteLine(proc?.StandardError.ReadToEnd());
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