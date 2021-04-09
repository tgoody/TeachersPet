using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Pages.CourseInfo;
using TeachersPet.Services;

namespace TeachersPet.Pages.Students {
    public partial class StudentsPage : Page, CourseInfoModule {

        private ObservableCollection<StudentModel> students;
        private ICollectionView filteredStudents;
        private CourseModel _courseModel;

        //TODO: Should we use this weird ICollectionView solution
        //Or just make another list that contains the full data of students
        //and a second one that contains the matched students from the filter
        //and just display that second one?
        //That latter option is more intuitive, but probably not the "best" way
        public StudentsPage() {
            InitializeComponent();
            students = new ObservableCollection<StudentModel>();
            filteredStudents = CollectionViewSource.GetDefaultView(students);
            filteredStudents.Filter = FilterStudents;
            UiStudentList.ItemsSource = students;
        }

        private async Task GetStudents() {
            
            //TODO: race condition here? this gets run from activator, but coursemodel relies on the setdata function, which comes after activator
            var studentJsonList = await CanvasAPI.GetStudentListFromCourseId(_courseModel.Id);
            
            foreach (var student in studentJsonList) {
                var studentModel = student.ToObject<StudentModel>();
                Dispatcher.Invoke(() => {students.Add(studentModel);});
            }
            Dispatcher.Invoke(() => {
                UiLoadingBlock.Visibility = Visibility.Collapsed;
                UiStudentList.Visibility = Visibility.Visible;
                SearchBar.Visibility = Visibility.Visible;
            });


        }

        private async void ClickStudent(object sender, MouseButtonEventArgs e) {

            var listBoxItem = sender as ListBoxItem;
            var student = listBoxItem.Tag as StudentModel;

            NavigationService.Navigate(new StudentInfo.StudentInfoPage(student, _courseModel));
        }

        
        //Some students don't have retrievable avatars
        //This results in their initials being their avatar on Canvas
        //For us, we can't retrieve that data, so we're going to manually set the image here
        private void SetDefaultAvatar(object sender, ExceptionRoutedEventArgs e)
        {
            ((Image)sender).Source = new BitmapImage(new Uri("/Resources/Images/defaultAvatar.png", UriKind.Relative));
        }

        private bool FilterStudents(object sender)
        {
            if (string.IsNullOrWhiteSpace(SearchBar.Text)) {
                return true;
            }
            //TODO: better search method, allow for typos, searching multiple things
            return sender is StudentModel student && 
                (student.Name.ToLower().Contains(SearchBar.Text.ToLower()) || 
                student.SisUserId.ToString().ToLower().Contains(SearchBar.Text.ToLower()));
        }

        private void RefreshList(object sender, TextChangedEventArgs textChangedEventArgs) {
            //TODO: investigate why this throws undefined/null exception -- comes from Activator()
            filteredStudents?.Refresh();
        }
        
        
        
        //Module data
        public string GetName() {
            return "View Students";
        }
        public void SetParentData(object parentInstance) {
            var parent = parentInstance as CourseInfoPage;
            _courseModel = parent.CourseModel;
        }
        public void InitializeData() {
            Task.Run(GetStudents);
        }
    }
}