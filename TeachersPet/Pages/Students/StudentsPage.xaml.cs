using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.Students {
    public partial class StudentsPage : Page, CourseInfoModule {

        private List<StudentModel> students;

        public StudentsPage() {
            InitializeComponent();
            GetStudents();
            
            
            //have some loading bar
            //currently just having a text box that says loading, which we will change in the async func
            
        }



        private async Task GetStudents() {
            
            var studentJsonList = CanvasAPI.GetStudentListFromCourseID(App.CurrentClassData["id"]?.ToString()).Result;
            foreach (var student in studentJsonList) {
                
                //deserialize object when model code is complete
                var newItem = new ListViewItem {
                    Content = student["name"],
                    Tag = student["id"]
                };
                UiStudentList.Items.Add(newItem);
            }

            await Task.Delay(10000);
            
            
            UiLoadingBlock.Visibility = Visibility.Collapsed;
            UiStudentList.Visibility = Visibility.Visible;

        }
        
        
        
        
        
        
        
        //Module data
        public string GetName() {
            return "View Students";
        }
    }
}