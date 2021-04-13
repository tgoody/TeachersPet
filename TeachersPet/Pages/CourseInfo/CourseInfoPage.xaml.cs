using System;
using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseInfo {
    public partial class CourseInfoPage : Page {
        public CourseModel CourseModel => _courseModel;

        private CourseModel _courseModel;
        
        public CourseInfoPage() {
            InitializeComponent();
        }

        public CourseInfoPage(CourseModel courseData) {
            InitializeComponent();
            _courseModel = courseData;
            GenerateModuleService.CreateWrapPanel(typeof(CourseInfoModule), ref WrapPanel, this); //generates child page buttons
            CourseCode.Text = courseData.CourseCode;
            CourseName.Text = courseData.CourseName;
            CourseInfo.Text = courseData.Id;
        }
        
    }
}