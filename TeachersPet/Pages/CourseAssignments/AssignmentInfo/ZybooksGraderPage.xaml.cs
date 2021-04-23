using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class ZybooksGraderPage : Page, AssignmentInfoModule, INotifyPropertyChanged {
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
        private string graderOutput;
        private bool autoscroll = true;
        public string GraderOutput {
            get => graderOutput;
            set {
                graderOutput = value;
                RaisePropertyChanged(nameof(GraderOutput));
            } 
        }
        private void RaisePropertyChanged(string propName) {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        
        
        public ZybooksGraderPage() {
            InitializeComponent();
            _assignment = new AssignmentModel();
            _zybooksStudentModels = new List<ZybooksStudentModel>();
            _canvasStudentModels = new List<StudentModel>();
            DataContext = this;
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
                    else if (headerName.Contains("school email")) { //prioritize school email if that column is there
                        emailIndex = i;
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

        //TODO: use email to confirm zybooks student to canvas student
        private void DragDropBox_DragDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ConvertZybooksCSV(files[0]);
            GradientBrush.GradientStops.ElementAt(0).Color = (Color)ColorConverter.ConvertFromString("#7F0770A3");
            DragDropText.Text = "Loading students from Canvas still, please hold...";
            //TODO: this is bad.
            while (_canvasStudentModels == null || _canvasStudentModels.Count == 0) {
                Thread.Sleep(250);
            }

            DragDropText.Text = "File Loaded";
            GradeButton.Visibility = Visibility.Visible;
            Checkbox.Visibility = Visibility.Visible;
        }
        
        private void UpdateGrades(object sender, MouseButtonEventArgs e) {
            var combinedData = new Dictionary<StudentModel, string>();
            foreach (var zybooksStudent in _zybooksStudentModels) {
                var matchingCanvasStudent = GetClosestMatchedCanvasStudent(zybooksStudent);
                if (matchingCanvasStudent == null) { //couldn't match on name or email
                    continue;
                }
                try {
                    combinedData.Add(matchingCanvasStudent, zybooksStudent.grade.ToString());
                }
                catch (ArgumentException) {
                    Console.Error.WriteLine($"Student already found: {matchingCanvasStudent.Name}");
                }
            }
            Task.Run(() => PutGrades(combinedData));
            
            //handle UI
            GradeButton.Visibility = Visibility.Collapsed;
            Checkbox.Visibility = Visibility.Collapsed;
            StudentsGradedProgress.Visibility = Visibility.Visible;
            ScrollViewer.Visibility = Visibility.Visible;
            GradientBrush.GradientStops.ElementAt(0).Color = (Color)ColorConverter.ConvertFromString("#7FA38007");
        }

        private StudentModel GetClosestMatchedCanvasStudent(ZybooksStudentModel zybooksStudent) {

            string firstName = zybooksStudent.firstName, lastName = zybooksStudent.lastName;
            var queryResult = _canvasStudentModels.Where(s =>
                s.Name.ToLower().Contains($"{firstName.ToLower()}") &&
                s.Name.ToLower().Contains($"{lastName.ToLower()}")).ToList();
            
            //If we found the right student
            if (queryResult.Count == 1) {
                return queryResult.ElementAt(0);
            }
            
            //else, try to match explicitly on name
            queryResult = _canvasStudentModels.Where(s => 
                s.Name.ToLower() == $"{firstName.ToLower()} {lastName.ToLower()}").ToList();
            
            if (queryResult.Count == 1) {
                return queryResult.ElementAt(0);
            }
            
            //else, match on email
            queryResult = _canvasStudentModels.Where(s => s.Email.ToLower() == zybooksStudent.email.ToLower()).ToList();
            return queryResult.Count == 1 ? queryResult.ElementAt(0) : null;
        }
        
        private async void PutGrades(Dictionary<StudentModel, string> mapForStudentsToGrades) {
            var counter = 0;
            var overwriteGrades = false;
            Dispatcher.Invoke(() => { overwriteGrades = OverwriteGradeCheckbox.IsChecked ?? false; });
            
            foreach (var (studentModel, score) in mapForStudentsToGrades) {
                if (!overwriteGrades) {
                    var gradeResponse =
                        await CanvasAPI.GetSubmissionForAssignmentForStudent(_assignment.CourseId, _assignment.Id,
                            studentModel.Id);
                    var currentGrade = gradeResponse["grade"]?.ToString();
                    if (currentGrade != "") {
                        Dispatcher.Invoke(() => GraderOutput += "\n" + $"Grade not null for {studentModel.Name}");
                    }
                    else {
                        var result = CanvasAPI.UpdateSingleGradeFromStudentModelAndScore(studentModel,
                            score, _assignment.CourseId, _assignment.Id);
                    }
                }
                else {
                    var result = CanvasAPI.UpdateSingleGradeFromStudentModelAndScore(studentModel,
                        score, _assignment.CourseId, _assignment.Id);
                    Dispatcher.Invoke(() => GraderOutput += "\n" + $"Graded {studentModel.Name}: {score}");
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

        
        
        
        //Found https://stackoverflow.com/a/19315242
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0) { // Content unchanged : user scroll event
                autoscroll = ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight;
            }

            // Content scroll event : auto-scroll eventually
            if (autoscroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ExtentHeight);
            }
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
            var workingDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
            workingDirectory = Path.GetDirectoryName(workingDirectory);
            var modulePath = Directory.CreateDirectory($"{workingDirectory}/Modules").FullName;
            var zybooksModulePath = Directory.CreateDirectory($"{modulePath}/ZybooksGrader").FullName;
            Directory.CreateDirectory($"{zybooksModulePath}/{_assignment.CourseId}");
        }
    }
}