using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseAssignments.AssignmentInfo {
    public partial class ZybooksGraderPage : Page, AssignmentInfoModule {
        private struct ZybooksStudentModel {
            public string lastName;
            public string firstName;
            public string email;
            public decimal grade;
            public decimal percentage;
        }
        private AssignmentModel _assignment;
        private List<ZybooksStudentModel> _zybooksStudentModels;
        private List<StudentModel> _canvasStudentModels;
        private Dictionary<string, string> fixedNames;
        private string pathToZybooksGraderFolder;
        public ZybooksGraderPage() {
            InitializeComponent();
        }

        private void ConvertZybooksCSV(string path) {

            try {
                var file = new StreamReader(path);
                var line = file.ReadLine();
            
                var dataPoints = line.Split(',');
                int lastNameIndex = 0, firstNameIndex = 1, emailIndex = 2, percentageIndex = 6, gradeIndex = 7, labTotal = 10;
                int labGrade = 0;
                for (int i = 0; i < dataPoints.Length; i++) {
                    var headerName = dataPoints[i].ToLower();
                    if (headerName.Contains("last name")) {
                        lastNameIndex = i;
                    }
                    else if (headerName.Contains("first name")) {
                        firstNameIndex = i;
                    }
                    else if (headerName.Contains("email")) {
                        emailIndex = i;
                    }
                    else if (headerName.Contains("percent")) {
                        percentageIndex = i;
                    }
                    else if (headerName.Contains("earned")) {
                        gradeIndex = i;
                    }
                    else if (headerName.Contains("lab total")) {
                        labTotal = i;
                        var firstParen = dataPoints[i].IndexOf('(');
                        var secondParen = dataPoints[i].IndexOf(')');
                        var subStr = dataPoints[i].Substring(firstParen+1, secondParen - firstParen-1);
                        labGrade = Convert.ToInt32(subStr);
                    }
                }
                
                while ((line=file.ReadLine()) != null) {

                    dataPoints = line.Split(',');
                    var studentGrade = Convert.ToDecimal(dataPoints[labTotal]);
                    studentGrade /= 100;
                    studentGrade *= labGrade;
                    studentGrade = Math.Round(studentGrade, MidpointRounding.AwayFromZero);
                    //newStudent.grade = Convert.ToDecimal(dataPoints[gradeIndex]);
                    var newStudent = new ZybooksStudentModel {
                        lastName = dataPoints[lastNameIndex],
                        firstName = dataPoints[firstNameIndex],
                        email = dataPoints[emailIndex],
                        percentage = Convert.ToDecimal(dataPoints[percentageIndex]),
                        grade = studentGrade
                    };
                    _zybooksStudentModels.Add(newStudent);
                }
            }
            catch (Exception e) {
                ErrorText.Visibility = Visibility.Visible;
                ErrorText.Text = $"Error: {e.Message}";
                throw e;
            }
            ErrorText.Visibility = Visibility.Hidden;
        }
        
        private void DragDropBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void DragDropBox_DragDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ConvertZybooksCSV(files[0]);
            var canvas = sender as Canvas;
            GradientBrush.GradientStops.ElementAt(0).Color = (Color)ColorConverter.ConvertFromString("#7F0770A3");

            DragDropText.Text = "Loading students from Canvas still, please hold...";
            
            //TODO: this is bad.
            while (_canvasStudentModels == null || _canvasStudentModels.Count == 0) {
                Console.WriteLine("loading students...");
                Thread.Sleep(250);
            }

            if (_canvasStudentModels.Count != _zybooksStudentModels.Count) {
                Console.WriteLine("Warning: mismatch in number of canvas students and zybooks students!");
                try {
                    GetFixedNames();
                }
                //This will happen if GetFixedNames throws an exception, meaning there is no fixednames.txt
                catch (Exception) {
                    noFixedNamesText.Text = "Warning: mismatch in number of canvas students and zybooks students!\n" +
                           "No FixedNames.txt found, please create one";
                    noFixedNamesText.Visibility = Visibility.Visible;
                    GenerateBadNames(_zybooksStudentModels, _canvasStudentModels);
                }
            }
            
            DragDropText.Text = "File Loaded";
            GradeButton.Visibility = Visibility.Visible;
            Checkbox.Visibility = Visibility.Visible;
        }
        
        private void UpdateGrades(object sender, MouseButtonEventArgs e) {
            var combinedData = new Dictionary<StudentModel, string>();
            foreach (var zybooksStudent in _zybooksStudentModels) {

                var firstName = zybooksStudent.firstName;
                var lastName = zybooksStudent.lastName;
                StudentModel matchingCanvasStudent;
                try {
                    matchingCanvasStudent =
                        _canvasStudentModels.SingleOrDefault(s => s.Name.ToLower().Contains($"{firstName.ToLower()}") && s.Name.ToLower().Contains($"{lastName.ToLower()}"));
                }
                catch (InvalidOperationException) {
                    //If the first line fails, try to match explicitly
                    //Example problem (a real one we ran into) -- two students:
                    //1. Leonardo Leon 
                    //2. Leonardo <something else>
                    //Because of the first student, their last name is contained in the first name of the second student
                    //Causes an issue.
                    matchingCanvasStudent =
                        _canvasStudentModels.SingleOrDefault(s =>
                            s.Name.ToLower() == $"{firstName.ToLower()} {lastName.ToLower()}");
                    if (matchingCanvasStudent == null) {
                        Console.Error.WriteLine($"Error: Multiple students with the name {firstName} {lastName}");
                        continue;
                    }
                }
                if (matchingCanvasStudent == null){
                    var studentName = (zybooksStudent.firstName + " " + zybooksStudent.lastName).ToLower();
                    if (fixedNames.ContainsKey(studentName)) {
                    
                        var fixedName = fixedNames[studentName];
                        try {
                            matchingCanvasStudent =
                                _canvasStudentModels.SingleOrDefault(s => s.Name.ToLower().Contains($"{fixedName}"));
                        }
                        catch (InvalidOperationException) {
                            Console.Error.WriteLine($"Error: Multiple students with the name {fixedName}");
                            continue;
                        }
                    }
                    else {
                        Console.Error.WriteLine(
                            $"Error: No student found with name {firstName} {lastName} in Canvas from Zybooks report");
                        continue;
                    }
                }

                try {
                    combinedData.Add(matchingCanvasStudent, zybooksStudent.grade.ToString());
                }
                catch (ArgumentException) {
                    Console.Error.WriteLine($"key already found: {matchingCanvasStudent.Name}");
                }
            }
            Task.Run(() => PutGrades(combinedData));
            
            //handle UI
            GradeButton.Visibility = Visibility.Collapsed;
            Checkbox.Visibility = Visibility.Collapsed;
            StudentsGradedProgress.Visibility = Visibility.Visible;
            GradientBrush.GradientStops.ElementAt(0).Color = (Color)ColorConverter.ConvertFromString("#7FA38007");

        }

        private async void PutGrades(Dictionary<StudentModel, string> mapForStudentsToGrades) {
            var counter = 0;
            
            bool overwriteGrades = false;
            Dispatcher.Invoke(() => { overwriteGrades = OverwriteGradeCheckbox.IsChecked ?? false; });
            
            foreach (var (studentModel, score) in mapForStudentsToGrades) {
                if (!overwriteGrades) {
                    var gradeResponse =
                        await CanvasAPI.GetSubmissionForAssignmentForStudent(_assignment.CourseId, _assignment.Id,
                            studentModel.Id);
                    var stringCheck = gradeResponse["grade"]?.ToString();
                    if (stringCheck != "") {
                        Console.WriteLine($"Grade not null for {studentModel.Name}");
                    }
                    else {
                        var result = CanvasAPI.UpdateSingleGradeFromStudentModelAndScore(studentModel,
                            score, _assignment.CourseId, _assignment.Id);
                    }
                }
                else {
                    var result = CanvasAPI.UpdateSingleGradeFromStudentModelAndScore(studentModel,
                        score, _assignment.CourseId, _assignment.Id);
                    Console.WriteLine($"Graded {studentModel.Name}");
                }

                counter++;
                Dispatcher.Invoke(() => {
                    DragDropText.Text = $"Graded {counter}/{mapForStudentsToGrades.Count} students!";
                });
            }
            Dispatcher.Invoke(() => {
                GradientBrush.GradientStops.ElementAt(0).Color = (Color)ColorConverter.ConvertFromString("#7F07A332");
            });
                
        }
        
        private void GetFixedNames() {
            var file = new StreamReader($"{pathToZybooksGraderFolder}/FixedNames.txt");
            string fixedNamesLine;
            while ((fixedNamesLine = file.ReadLine()) != null) {
                var names = fixedNamesLine.Split(',');
                fixedNames.Add(names[0].ToLower(), names[1].ToLower());
            }
        }
        
        private void OpenExplorer(object sender, MouseButtonEventArgs e) {
            Process.Start("explorer.exe" , @pathToZybooksGraderFolder);
        }

        /// <summary>
        /// Used to generate a text file named "IncorrectNames.txt" containing mismatched student names between zybooks and canvas
        /// </summary>
        /// <param name="studentsFromFile">Zybooks List</param>
        /// <param name="canvasStudents">Students from Canvas in Dictionary format</param>
        private void GenerateBadNames(List<ZybooksStudentModel> studentsFromFile, List<StudentModel> canvasStudents) {
            var file = new StreamWriter($"{pathToZybooksGraderFolder}/IncorrectNames.txt");
            foreach (var student in studentsFromFile) {
                if (!canvasStudents.Exists(s => s.Name.ToLower().Contains($"{student.firstName.ToLower()}") && s.Name.ToLower().Contains($"{student.lastName.ToLower()}"))) {
                    var studentName = (student.firstName + " " + student.lastName).ToLower();
                    file.WriteLine(studentName);
                }
            }
            file.Close();
        }
        
        public string GetName() {
            return "Zybooks Grader";
        }

        public void SetParentData(object parentInstance) {
            var parentPage = parentInstance as AssignmentInfo;
            _assignment = parentPage.Assignment;
        }

        public void InitializeData() {
            Task.Run(() => {
                _canvasStudentModels = CanvasAPI.GetStudentListFromCourseId(_assignment.CourseId).Result
                    .ToObject<List<StudentModel>>();
            });
            _zybooksStudentModels = new List<ZybooksStudentModel>();
            _canvasStudentModels = new List<StudentModel>();
            fixedNames = new Dictionary<string, string>();
            var workingDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
            workingDirectory = Path.GetDirectoryName(workingDirectory);
            var zybooksModulePath = Directory.CreateDirectory($"{workingDirectory}/ZybooksGrader").FullName;
            pathToZybooksGraderFolder =
                Directory.CreateDirectory($"{zybooksModulePath}/{_assignment.CourseId}").FullName;
        }
    }
}