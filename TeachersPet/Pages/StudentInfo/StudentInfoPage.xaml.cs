using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.StudentInfo {
    public partial class StudentInfoPage : Page {

        private StudentModel _studentModel;
        private CourseModel _courseModel;
        private ObservableCollection<SubmissionModel> _submissions;
        private ICollectionView _submissionsView;

        //TODO: Understand why ICollectionView works with ObservableCollection vs List<>
        
        public StudentInfoPage(StudentModel student, CourseModel courseModel) {
            InitializeComponent();
            _studentModel = student;
            _courseModel = courseModel;
            _submissions = new ObservableCollection<SubmissionModel>();
            _submissionsView = CollectionViewSource.GetDefaultView(_submissions);
            _submissionsView.SortDescriptions.Add(new SortDescription("AssignmentModel.Name", ListSortDirection.Ascending));
            AssignmentList.ItemsSource = _submissionsView;
            
            Task.Run(async () => {
                await MergeProfileData();
                Dispatcher.Invoke(() => {
                    DataContext = null; //This is bad, but I'm not sure if it's worse than the "refresh all bindings" option discussed (also bad)
                    DataContext = _studentModel; //The correct solution is using INotifyPropertyChanged, but it would take too long to write that out for each property
                });
            });
            Task.Run(GetSubmissions);
        }

        //cool reflection way to merge in null values
        private async Task MergeProfileData()
        {
            var studentProfile = (await CanvasAPI.GetStudentProfileFromStudentId(_studentModel.Id)).ToObject<StudentModel>();

            if (string.IsNullOrEmpty(_studentModel.AvatarUrl) && !string.IsNullOrEmpty(studentProfile.AvatarUrl)) {
                _studentModel.AvatarUrl = studentProfile.AvatarUrl;
            }
            if (string.IsNullOrEmpty(_studentModel.Email) && !string.IsNullOrEmpty(studentProfile.Email)) {
                _studentModel.Email = studentProfile.Email;
            }
        }

        private async Task GetSubmissions() {
            var submissionJson = await 
                CanvasAPI.GetSubmissionsFromUserAndCourseId(_studentModel.Id, _courseModel.Id);
            foreach (var submission in submissionJson) {
                var submissionObject = submission.ToObject<SubmissionModel>();
                Dispatcher.Invoke( () => _submissions.Add(submissionObject));
                Dispatcher.Invoke( () => _submissionsView.Refresh());
            }

        }
        
        private void SubmitGradeChange(object sender, MouseButtonEventArgs e) {
            
            //TODO: error handling that input is decimal or int
            var button = sender as Button;
            var previousData = button.DataContext as SubmissionModel;
            ChangeStatus.Visibility = Visibility.Visible;
            if (NewCommentBox.Text.Length == 0 && previousData.Score == ScoreTextBox.Text) {
                ChangeStatus.Text = "No changes detected!";
                return;
            }

            previousData.Score = ScoreTextBox.Text;
            var result = CanvasAPI.UpdateGradeFromSubmissionModel(previousData, NewCommentBox.Text).Result;
            ChangeStatus.Text = "Submission updated!";

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

        private void FillRegretClause(object sender, RoutedEventArgs e) {

            ScoreTextBox.Text = "0";
            NewCommentBox.RemovePlaceHolder();
            NewCommentBox.Text = "Submission withdrawn by student request.";

        }
    }
}