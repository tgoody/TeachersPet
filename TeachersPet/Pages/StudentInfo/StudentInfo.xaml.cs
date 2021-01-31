using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TeachersPet.Models;

namespace TeachersPet.Pages.StudentInfo {
    public partial class StudentInfo : Page {
        public StudentInfo() {
            InitializeComponent();
        }


        public StudentInfo(StudentModel student) {
            InitializeComponent();
            TextBlock.Text = $"{student.Name}, {student.SisUserId}\n{student.Email}";
            Avatar.Source = new BitmapImage(new Uri(student.AvatarUrl));


        }
        
        
    }
}