using System.Windows.Controls;
using Newtonsoft.Json.Linq;

namespace TeachersPet.Pages.CourseInfo {
    public partial class CourseInfoPage : Page {
        public CourseInfoPage() {
            InitializeComponent();
        }

        public CourseInfoPage(JToken courseData) {
            InitializeComponent();
            TextBlock.Text = courseData.ToString();
        }
        
    }
}