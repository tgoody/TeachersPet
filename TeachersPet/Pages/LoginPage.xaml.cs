using System;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
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
            JArray courseList;
            try {
                AuthenticationProvider.Instance().CanvasLogin();
                Console.WriteLine(AuthenticationProvider.Instance().ApiToken);
                courseList = CanvasAPI.GetCourseList().Result;
            }
            catch (Exception) {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = "Error logging in. Confirm API Token is set in settings.";
                return;
            }

            NavigationService?.Navigate(new CourseListPage(courseList));
        }

        private void OpenSettingsPage(object sender, RoutedEventArgs e) {
            NavigationService?.Navigate(new SettingsPage());
        }
    }
}