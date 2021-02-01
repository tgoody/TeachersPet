using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TeachersPet.Models;

namespace TeachersPet.Pages.StudentInfo {
    public partial class StudentInfo : Page {

        private StudentModel _studentModel;

        public StudentInfo() {
            InitializeComponent();
            _studentModel = new StudentModel();
            throw new Exception("StudentInfo page should be generated with StudentModel param");
        }


        public StudentInfo(StudentModel student) {
            InitializeComponent();
            _studentModel = student;
            DataContext = _studentModel;
            HeaderCanvas.DataContext = _studentModel;
        }
        
        
    }
}