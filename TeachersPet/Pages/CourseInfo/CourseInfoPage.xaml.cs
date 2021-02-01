using System.Windows.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseInfo {
    public partial class CourseInfoPage : Page {
        public CourseInfoPage() {
            InitializeComponent();
        }

        public CourseInfoPage(CourseModel courseData) {
            InitializeComponent();
            GenerateModuleService.CreateWrapPanel(typeof(CourseInfoModule), ref WrapPanel);
            TextBlock.Text = JsonConvert.SerializeObject(courseData);
        }
        
    }
}