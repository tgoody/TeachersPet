using System;
using System.Collections.Generic;
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
        
        public ZybooksGraderPage() {
            InitializeComponent();
            _zybooksStudentModels = new List<ZybooksStudentModel>();
            _canvasStudentModels = new List<StudentModel>();
            _assignment = new AssignmentModel();
        }

        private void ConvertZybooksCSV(string path) {
            
            var file = new StreamReader(path);
            var line = file.ReadLine();;
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
                studentGrade = 0;//Math.Round(studentGrade, MidpointRounding.AwayFromZero);
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

        
        
                
        private void DragDropBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void DragDropBox_DragDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ConvertZybooksCSV(files[0]);
            var canvas = sender as Canvas;
            canvas.Background = new SolidColorBrush(Colors.DarkBlue);
            GradeButton.Visibility = Visibility.Visible;
        }
        
        public string GetName() {
            return "Zybooks Grader";
        }

        public void SetParentData(object parentInstance) {
            var parentPage = parentInstance as AssignmentInfo;
            _assignment = parentPage.Assignment;
        }

        public void InitializeData() {
            //TODO: Caching system in CanvasAPI itself will speed this up so much
            Task.Run(() => {
                _canvasStudentModels = CanvasAPI.GetStudentListFromCourseId(_assignment.CourseId).Result
                    .ToObject<List<StudentModel>>();
            });
        }

        private void UpdateGrades(object sender, MouseButtonEventArgs e) {

            while (_canvasStudentModels == null || _canvasStudentModels.Count == 0) {
                Console.WriteLine("loading students...");
                Thread.Sleep(250);
            }
            
            if (_canvasStudentModels.Count != _zybooksStudentModels.Count) {
                Console.WriteLine("Warning: mismatch in number of canvas students and zybooks students!");
            }

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
                    Console.Error.WriteLine($"Error: Multiple students with the name {firstName} {lastName}");
                    continue;
                }
                if (matchingCanvasStudent == null){
                    Console.Error.WriteLine($"Error: No student found with name {firstName} {lastName} in Canvas from Zybooks report");
                    continue;
                }

                try {
                    combinedData.Add(matchingCanvasStudent, zybooksStudent.grade.ToString());
                }
                catch (ArgumentException) {
                    Console.Error.WriteLine($"key already found: {matchingCanvasStudent.Name}");
                }
            }
            
            
            var canvasProgress = CanvasAPI.UpdateGradesFromStudentModelsAndScores(combinedData, _assignment.CourseId, _assignment.Id).Result;
            Task.Run(() => CheckProgressOfGrades(canvasProgress));
            GradeButton.Visibility = Visibility.Collapsed;
            StudentsGradedProgress.Visibility = Visibility.Visible;
            DragDropBox.Background = new SolidColorBrush(Colors.Orange);

        }

        void CheckProgressOfGrades(JToken canvasProgressResponse) {

            
            //It seems that the progress response from Canvas API gets turned into a JValue instead of JToken
            //This is a quick fix to change it, but unsure why we're getting that result
            try {
                var testForStringResponse = canvasProgressResponse["workflow_state"];
            }
            catch (InvalidOperationException) {
                canvasProgressResponse = JToken.Parse(canvasProgressResponse.ToString());
            }
            if (canvasProgressResponse["workflow_state"] == null && canvasProgressResponse["completion"] == null) {
                return;
            }

            while ((string) canvasProgressResponse["workflow_state"] != "completed" &&
                   (string) canvasProgressResponse["completion"] != "100") {
                Thread.Sleep(2000);
                Dispatcher.Invoke(() => {
                    StudentsGradedProgress.Text =
                        $"{canvasProgressResponse["workflow_state"]}...";
                });
                canvasProgressResponse = CanvasAPI.RefreshProgress(canvasProgressResponse).Result;
                Console.WriteLine(canvasProgressResponse);
            }

            Dispatcher.Invoke(() => {

                DragDropBox.Background = new SolidColorBrush(Colors.Chartreuse);
                StudentsGradedProgress.Text = "Completed!";

            });
        }
    }
}