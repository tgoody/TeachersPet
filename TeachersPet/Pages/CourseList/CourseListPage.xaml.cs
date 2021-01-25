using System.Windows.Controls;
using TeachersPet.BaseModules;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseList {
    public partial class CourseListPage : Page {
        
        public CourseListPage() {
            InitializeComponent();
            WrapPanel.Orientation = Orientation.Horizontal;
            GenerateModuleService.CreateWrapPanel(typeof(CourseListModule), ref WrapPanel);
        }
        
    }
}