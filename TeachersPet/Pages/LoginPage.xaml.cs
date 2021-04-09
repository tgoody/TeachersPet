using System;
using System.Windows;
using System.Windows.Controls;
using TeachersPet.Pages.CourseList;
using TeachersPet.Pages.Settings;
using TeachersPet.Services;

namespace TeachersPet.Pages {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginPage : Page {
        public LoginPage() {
            InitializeComponent();
            
            
            
        }

        private void Button_Click(object sender, RoutedEventArgs routedEventArgs) {
            AuthenticationProvider.Instance().CanvasLogin();
            Console.WriteLine(AuthenticationProvider.Instance().ApiToken);
            var courseList = CanvasAPI.GetCourseList().Result;
            NavigationService?.Navigate(new CourseListPage(courseList));
        }

        private void OpenSettingsPage(object sender, RoutedEventArgs e) {
            NavigationService?.Navigate(new SettingsPage());
        }
    }
}