using System;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;
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
                        Tag = course["id"],
                        Content = $"{course["course_code"]}: {course["name"]}"
                    };
                    StackPanel.Children.Add(newButton);
                }
            }
            catch (Exception e) {
                Console.Error.WriteLine("Error populating course list");
                Console.Error.WriteLine(e);
            }

        }
        
    }
}