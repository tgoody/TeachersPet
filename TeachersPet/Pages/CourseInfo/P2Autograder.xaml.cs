using System.Windows.Controls;
using TeachersPet.BaseModules;

namespace TeachersPet.Pages.CourseInfo {
    public partial class P2Autograder : Page, CourseInfoModule {
        public P2Autograder() {
            InitializeComponent();
        }

        public string GetName() {
            return "Project 2 Autograder";
        }

        public void SetParentData(object parentInstance) {
            return;
        }

        public void InitializeData() {
            return;
        }
    }
}