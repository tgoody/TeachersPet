using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseInfo {
    public partial class CourseInfoPage : Page {
        public CourseInfoPage() {
            InitializeComponent();
        }

        public CourseInfoPage(JToken courseData) {
            InitializeComponent();
            GenerateModuleService.CreateWrapPanel(typeof(CourseInfoModule), ref WrapPanel);
            TextBlock.Text = courseData.ToString();
        }
        
    }
}