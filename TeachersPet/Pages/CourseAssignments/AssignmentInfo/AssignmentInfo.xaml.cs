using System.Windows.Controls;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseAssignments.AssignmentInfo {
    public partial class AssignmentInfo : Page {
        
        private AssignmentModel _assignment;
        
        

        public AssignmentModel Assignment => _assignment;
        public AssignmentInfo() {
            InitializeComponent();
        }
        public AssignmentInfo(AssignmentModel assignmentModel) {
            InitializeComponent();
            _assignment = assignmentModel;
            AssName.Text = _assignment.Name;
            AssDate.Text = "Due: " + _assignment.DueDateString;
            GenerateModuleService.CreateWrapPanel(typeof(AssignmentInfoModule), ref WrapPanel, this);

        }
    }
}