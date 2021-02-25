using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TeachersPet.CustomControls;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.StudentInfo {
    public partial class StudentInfo : Page {

        private StudentModel _studentModel;
        private CourseModel _courseModel;
        private ObservableCollection<SubmissionModel> _submissions;
        private ICollectionView _submissionsView;

        //TODO: Understand why ICollectionView works with ObservableCollection vs List<>
        
        public StudentInfo(StudentModel student, CourseModel courseModel) {
            InitializeComponent();
            _studentModel = student;
            _courseModel = courseModel;
            _submissions = new ObservableCollection<SubmissionModel>();
            _submissionsView = CollectionViewSource.GetDefaultView(_submissions);
            _submissionsView.SortDescriptions.Add(new SortDescription("AssignmentModel.Name", ListSortDirection.Ascending));
            AssignmentList.ItemsSource = _submissionsView;
            DataContext = _studentModel;
            //TODO: Loading bar for submissions data
            Task.Run(GetSubmissions);
            
        }

        private async Task GetSubmissions() {
            var submissionJson = await 
                CanvasAPI.GetSubmissionsFromUserAndCourseId(_studentModel.Id, _courseModel.Id);
            foreach (var submission in submissionJson) {
                var submissionObject = submission.ToObject<SubmissionModel>();
                Dispatcher.Invoke( () => _submissions.Add(submissionObject));
            }
            Dispatcher.Invoke( () => _submissionsView.Refresh());

        }


        private void ScoreTextChanged(object sender, TextChangedEventArgs e) {

            var textBox = sender as TextBox;
            var parent = textBox.Parent as WrapPanel;
            var checkbox = parent.Children.OfType<Image>().SingleOrDefault(sibling => sibling.Name == "ConfirmChanges");
            var commentBox = parent.Children.OfType<PlaceHolderTextBox>().SingleOrDefault(sibling => sibling.Name == "NewCommentBox");
            checkbox.Visibility = Visibility.Visible;
            commentBox.Visibility = Visibility.Visible;
        }

        private void SubmitGradeChange(object sender, MouseButtonEventArgs e) {
            
            //TODO: error handling that input is decimal or int
            var image = sender as Image;
            var parent = image.Parent as WrapPanel;
            var commentBox = parent.Children.OfType<PlaceHolderTextBox>().SingleOrDefault(sibling => sibling.Name == "NewCommentBox");
            image.Visibility = Visibility.Hidden;
            commentBox.Visibility = Visibility.Collapsed;
            var submission = image.Tag as SubmissionModel;
            var result = CanvasAPI.UpdateGradeFromSubmissionModel(submission, commentBox.Text).Result;

        }

        private void OpenSpeedGrader(object sender, MouseButtonEventArgs e) {

            var image = sender as Image;
            var submission = image.Tag as SubmissionModel;
            var url = CanvasAPI.CanvasApiUrl.Replace("api/v1/", "") +
                      $"courses/{_courseModel.Id}/gradebook/speed_grader?assignment_id={submission.AssignmentId}&student_id={submission.UserId}";
            //should be able to just do Process.Start(url) but I was getting an error, so this should fix it.
            Process.Start(new ProcessStartInfo(url){UseShellExecute = true});

        }

        private void SelectAssignment(object sender, SelectionChangedEventArgs e) {

            var itemSelected = (sender as ListView).SelectedItem as SubmissionModel;
            AssignmentInfoPanel.DataContext = itemSelected;
            AssignmentInfoPanel.Visibility = Visibility.Visible;



        }
    }
}