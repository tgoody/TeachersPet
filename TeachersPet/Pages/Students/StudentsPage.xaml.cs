using System;
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
            Task.Run(GetStudents);
            
            //have some loading bar
            //currently just having a text box that says loading, which we will change in the async func

        }



        private async Task GetStudents() {
            
            var studentJsonList = await CanvasAPI.GetStudentListFromCourseId(App.CurrentClassData["id"]?.ToString());
            foreach (var student in studentJsonList) {
                //TODO: work on the async stuff here
                //TODO: deserialize object when model code is complete
                Dispatcher.InvokeAsync(() => {
                    var newItem = new ListViewItem {
                        Content = (string)student["name"],
                        Tag = (string)student["id"]
                    };
                    UiStudentList.Items.Add(newItem);
                });
            }

            Dispatcher.Invoke(() => {
                UiLoadingBlock.Visibility = Visibility.Collapsed;
                UiStudentList.Visibility = Visibility.Visible;
            });


        }
        
        
        
        
        
        
        
        //Module data
        public string GetName() {
            return "View Students";
        }
    }
}