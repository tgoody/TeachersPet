using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.Students {
    public partial class StudentsPage : Page, CourseInfoModule {

        private ObservableCollection<StudentModel> students;

        public StudentsPage() {
            InitializeComponent();
            students = new ObservableCollection<StudentModel>();
            Task.Run(GetStudents);
            UiStudentList.ItemsSource = students;
            //have some loading bar
            //currently just having a text box that says loading, which we will change in the async func

        }



        private async Task GetStudents() {
            
            var studentJsonList = await CanvasAPI.GetStudentListFromCourseId(App.CurrentClassData.Id);
            foreach (var student in studentJsonList) {
                var studentModel = student.ToObject<StudentModel>();
                Dispatcher.InvokeAsync(() => {
                    students.Add(studentModel);
                });

            }

            Dispatcher.Invoke(() => {
                UiLoadingBlock.Visibility = Visibility.Collapsed;
                UiStudentList.Visibility = Visibility.Visible;
            });


        }

        private async void ClickStudent(object sender, MouseButtonEventArgs e) {

            var listBoxItem = sender as ListBoxItem;
            var student = students.SingleOrDefault(s => s.Id == (string) listBoxItem.Tag);

            if (student.Email == null) {
                var studentProfile = await CanvasAPI.GetStudentProfileFromStudentId(student.Id);
                if (studentProfile["email"] == null && studentProfile["login_id"] == null) {
                    throw new Exception("Couldn't retrieve email.");
                }
                student.Email = (string) studentProfile["email"] ?? (string) studentProfile["login_id"];
            }

            NavigationService.Navigate(new StudentInfo.StudentInfo(student));
        }

        
        //Some students don't have retrievable avatars
        //This results in their initials being their avatar on Canvas
        //For us, we can't retrieve that data, so we're going to manually set the image here
        private void SetDefaultAvatar(object sender, ExceptionRoutedEventArgs e)
        {
            ((Image)sender).Source = new BitmapImage(new Uri("/Resources/Images/defaultAvatar.png", UriKind.Relative));
        }
        
        
        
        
        
        
        //Module data
        public string GetName() {
            return "View Students";
        }
    }
}