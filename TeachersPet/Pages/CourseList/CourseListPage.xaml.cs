using System;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;
using TeachersPet.Pages.CourseInfo;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseList {
    public partial class CourseListPage : Page {
        
        public CourseListPage() {
            InitializeComponent();
            //GenerateModuleService.CreateWrapPanel(typeof(CourseListModule), ref WrapPanel);
            //No modules on this page probably
        }


        public CourseListPage(JArray courses) {

            InitializeComponent();
            try {
                foreach (var course in courses) {
                    var newButton = new Button {
                        Tag = course,
                        Content = $"{course["course_code"]}: {course["name"]}"
                    };
                    newButton.Click += CourseButtonClick;
                    StackPanel.Children.Add(newButton);
                }
            }
            catch (Exception e) {
                Console.Error.WriteLine("Error populating course list");
                Console.Error.WriteLine(e);
            }

        }



        private void CourseButtonClick(object sender, RoutedEventArgs e) {

            var button = sender as Button;
            NavigationService?.Navigate(new CourseInfoPage(button?.Tag as JToken));

        }
        
    }
}