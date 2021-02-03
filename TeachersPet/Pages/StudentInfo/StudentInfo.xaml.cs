using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.StudentInfo {
    public partial class StudentInfo : Page {

        private StudentModel _studentModel;
        private List<SubmissionModel> _submissions;

       
        public StudentInfo() {
            InitializeComponent();
            _studentModel = new StudentModel();
            throw new Exception("StudentInfo page should be generated with StudentModel param");
        }

        public StudentInfo(StudentModel student) {
            InitializeComponent();
            _studentModel = student;
            _submissions = new List<SubmissionModel>();
            DataContext = _studentModel;
            HeaderCanvas.DataContext = _studentModel;
            var submissionJson =
                CanvasAPI.GetSubmissionsFromUserAndCourseId(_studentModel.Id, App.CurrentCourseModel.Id).Result;
            foreach (var submission in submissionJson) {
                var submissionObject = submission.ToObject<SubmissionModel>();
                _submissions.Add(submissionObject);
            }
            _submissions = _submissions.OrderBy(submission => submission.AssignmentModel.Name).ToList();
            AssignmentList.ItemsSource = _submissions;
        }
    }
}