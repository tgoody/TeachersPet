using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.SectionInfo {
    public partial class SectionInfoPage : Page {

        public SectionModel SectionModel => _sectionModel;
        
        private SectionModel _sectionModel;
        private CourseModel _courseModel;
        public SectionInfoPage(SectionModel sectionModel) {
            InitializeComponent();
            _sectionModel = sectionModel;
            DataContext = this;
            StudentListView.Items.Clear(); //this was complaining that items were already in it? Unsure, so clearing it to be safe.
            StudentListView.ItemsSource = _sectionModel.Students;
            //this should be cached, so this should be quick
            Task.Run(async () => {
                var courseData = await CanvasAPI.GetCourseList();
                _courseModel = courseData.Select(course => course.ToObject<CourseModel>())
                    .Single(course => course.Id == _sectionModel.CourseId);

            });
        }

        private void ClickStudent(object sender, MouseButtonEventArgs e) {
            if (_courseModel == null) {
                ErrorMessageTextBlock.Visibility = Visibility.Visible;
                ErrorMessageTextBlock.Text = "Course Model not loaded. Try waiting and try again.";
            }
            NavigationService?.Navigate(new StudentInfo.StudentInfoPage((sender as ListViewItem).Tag as StudentModel, _courseModel));

        }
    }
}