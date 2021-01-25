using System.Windows.Controls;
using TeachersPet.BaseModules;

namespace TeachersPet.Pages.CourseList {
    public partial class CourseListModulePage1 : Page, CourseListModule {
        public CourseListModulePage1() {
            InitializeComponent();
        }

        public string GetName() {
            return "CourseListModule1";
        }
    }
}