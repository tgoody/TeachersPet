using System.Windows.Controls;
using TeachersPet.BaseModules;

namespace TeachersPet.Pages.CourseList {
    public partial class CourseListModulePage2 : Page, CourseListModule {
        public CourseListModulePage2() {
            InitializeComponent();
        }

        public string GetName() {
            return "CourseListModule2";
        }
    }
}