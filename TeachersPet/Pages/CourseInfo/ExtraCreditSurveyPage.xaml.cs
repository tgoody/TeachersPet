﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseInfo
{
    /// <summary>
    /// Interaction logic for ExtraCreditSurveyPage.xaml
    /// </summary>
    public partial class ExtraCreditSurveyPage : Page, CourseInfoModule, INotifyPropertyChanged
    {
        private class ExtraCreditReportStudentModel {
            public string firstName, lastName, ufid, email, creditsEarned;
        }

        private CourseModel parentData;
        private List<StudentModel> canvasStudentModels;
        private AssignmentModel extraCreditAssignmentModel;
        private List<ExtraCreditReportStudentModel> extraCreditReportStudentModels;
        private bool autoscroll = true;
        private string graderOutput;
        public string GraderOutput
        {
            get => graderOutput;
            set
            {
                graderOutput = value;
                RaisePropertyChanged(nameof(GraderOutput));
            }
        }
        private void RaisePropertyChanged(string propName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public ExtraCreditSurveyPage()
        {
            InitializeComponent();
            DataContext = this;
        }


        private void ParseExtraCreditReport(string path)
        {
            var file = new StreamReader(path);
            var line = file.ReadLine(); //skip the course name line
            line = file.ReadLine(); //read column header line

            var dataPoints = line.Split(',');

            int lastNameIndex = 1, firstNameIndex = 0, ufidIndex = 2, emailIndex = 3, creditsEarnedIndex = 5;

            while ((line = file.ReadLine()) != null)
            {
                dataPoints = line.Split(',');
                var tempModel = new ExtraCreditReportStudentModel();
                tempModel.firstName = dataPoints[firstNameIndex].Replace("\"", "");
                tempModel.lastName = dataPoints[lastNameIndex].Replace("\"", "");
                tempModel.ufid = dataPoints[ufidIndex].Replace("-", "").Replace("\"", "");
                tempModel.email = dataPoints[emailIndex].Replace("\"", "");
                tempModel.creditsEarned = dataPoints[creditsEarnedIndex].Replace("\"", "");
                extraCreditReportStudentModels.Add(tempModel);
            }
        }


        private void DragDropBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        //TODO: use email to confirm zybooks student to canvas student
        private void DragDropBox_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            ParseExtraCreditReport(files[0]);
            GradientBrush.GradientStops.ElementAt(0).Color = (Color)ColorConverter.ConvertFromString("#7F0770A3");
            DragDropText.Text = "Loading students from Canvas still, please hold...";
            Task.Run(() => {
                while (canvasStudentModels == null || canvasStudentModels.Count == 0)
                {
                    Thread.Sleep(250);
                }

                Dispatcher.Invoke(() => {
                    DragDropText.Text = "File Loaded";
                    UpdateExtraCreditButton.Visibility = Visibility.Visible;
                    MaxCourseExtraCredit.Visibility = Visibility.Visible;
                    MaxCourseExtraCreditTextBlock.Visibility = Visibility.Visible;
                    SurveyCreditValue.Visibility = Visibility.Visible;
                    SurveyCreditValueTextBlock.Visibility = Visibility.Visible;
                });
            });

        }

        private async void UpdateExtraCreditButton_Click(object sender, RoutedEventArgs e)
        {
            ScrollViewer.Visibility = Visibility.Visible;
            _ = Task.Run(async () =>
            {
                foreach (var ecstudentModel in extraCreditReportStudentModels)
                {
                    var numCredits = 0;
                    var didParseSuccessfully = int.TryParse(ecstudentModel.creditsEarned, out numCredits);
                    if (!didParseSuccessfully || numCredits < 1)
                    {
                        continue;
                    }

                    var scorePerCredit = 0;
                    Dispatcher.Invoke(() => { didParseSuccessfully = int.TryParse(SurveyCreditValue.Text, out scorePerCredit); });
                    
                    if (!didParseSuccessfully)
                    {
                        Console.Error.WriteLine("Text box with survey credit value didn't parse correctly");
                        continue;
                    }
                    var scoreToAdd = numCredits * scorePerCredit;

                    StudentModel foundStudent = null;
                    try
                    {
                        var studentId = int.Parse(ecstudentModel.ufid);
                        foundStudent = canvasStudentModels.Where(student => student.SisUserId == studentId).Single();
                    }
                    catch (Exception exception)
                    {
                        //couldn't parse UFID, try to match on name
                    }
                    if (foundStudent == null)
                    {
                        try
                        {
                            foundStudent = canvasStudentModels.Where(student => student.Name.Contains(ecstudentModel.firstName)
                                && student.Name.Contains(ecstudentModel.lastName)).Single();
                        }
                        catch (Exception exception)
                        {
                            //couldn't find matching student on name
                        }
                    }
                    if (foundStudent == null)
                    {
                        try
                        {
                            foundStudent = canvasStudentModels.Where(student => student.Email == ecstudentModel.email).Single();
                        }
                        catch (Exception exception)
                        {
                            //couldn't match on email either, so skip that student and hope for the best
                            Console.Error.WriteLine($"Couldn't parse: {ecstudentModel.firstName} {ecstudentModel.lastName}");
                            continue;
                        }
                    }

                    var gradeResponse = await CanvasAPI.GetSubmissionForAssignmentForStudent(parentData.Id, extraCreditAssignmentModel.Id, foundStudent.Id);
                    var currentGrade = gradeResponse["grade"]?.ToString();
                    var currentExtraCreditScore = 0;
                    int.TryParse(currentGrade, out currentExtraCreditScore);
                    currentExtraCreditScore += scoreToAdd;
                    var maxExtraCredit = 20;
                    Dispatcher.Invoke(() => int.TryParse(MaxCourseExtraCredit.Text, out maxExtraCredit));
                    if (currentExtraCreditScore > maxExtraCredit) currentExtraCreditScore = maxExtraCredit;

                    await CanvasAPI.UpdateSingleGradeFromStudentModelAndScore(foundStudent, currentExtraCreditScore.ToString(), parentData.Id,
                        extraCreditAssignmentModel.Id, "Added credit from survey.");
                    Dispatcher.Invoke(() =>
                    {
                        GraderOutput += "\n" + $"Added extra credit to: {ecstudentModel.firstName} {ecstudentModel.lastName}";
                    });
                }
                Dispatcher.Invoke(() => GraderOutput += "\n" + "Finished adding extra credit scores!");
                Dispatcher.Invoke(() => DragDropText.Text = "Completed!");
            });
            
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            { // Content unchanged : user scroll event
                autoscroll = ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight;
            }

            // Content scroll event : auto-scroll eventually
            if (autoscroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ExtentHeight);
            }
        }


        public string GetName()
        {
            return "Extra Credit Survey Submissions Tool";
        }

        public void InitializeData()
        {
            Task.Run(() =>{
                canvasStudentModels = CanvasAPI.GetStudentListFromCourseId(parentData.Id).Result
                    .ToObject<List<StudentModel>>();
            });
            Task.Run(() => {
                var assignments = CanvasAPI.GetAssignmentListFromCourseId(parentData.Id).Result
                    .ToObject<List<AssignmentModel>>();
                try
                {
                    extraCreditAssignmentModel = assignments.Where(assignment => assignment.Name.ToLower() == "extra credit").Single();
                }
                catch(Exception e)
                {
                    Console.Error.WriteLine("Found multiple (or no) assignments with the name \"extra credit\"");
                }
            });
            extraCreditReportStudentModels = new List<ExtraCreditReportStudentModel>();
        }

        public void SetParentData(object parentInstance)
        {
            var courseInfoPage = parentInstance as CourseInfoPage;
            parentData = courseInfoPage?.CourseModel;
        }
    }
}