using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Pages.CourseInfo;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseAssignments {
    public partial class CourseAssignmentsPage : Page, CourseInfoModule {
        private List<AssignmentModel> assignments;
        private CourseModel _courseModel;
        
        
        public CourseAssignmentsPage() {
            InitializeComponent();
            assignments = new List<AssignmentModel>();
        }

        private async void GetAssignments() {

            while (_courseModel == null) {
                Thread.Sleep(250);
            }
            
            var jsonAssignmentList = await CanvasAPI.GetAssignmentListFromCourseId(_courseModel.Id);
            foreach(var jsonAssignment in jsonAssignmentList)
            {
                assignments.Add(jsonAssignment.ToObject<AssignmentModel>());
            }
            Dispatcher.Invoke( () => AssignmentList.ItemsSource = assignments);
        }






        public string GetName() {
            return "View Assignments";
        }
        public void SetParentData(object parentInstance) {
            var parent = parentInstance as CourseInfoPage;
            var courseModel = parent.CourseModel;
            _courseModel = courseModel;
        }
        public void InitializeData() {
            Task.Run(GetAssignments);
        }
    }
}