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
            StudentName.Text = $"{student.Name}";
            StudentId.Text = $"UFID: {student.SisUserId}";
            StudentEmail.Text = $"Email: {student.Email}";
            StudentCanvId.Text = $"Canvas ID: {student.Id}";
            Avatar.ImageSource = new BitmapImage(new Uri(student.AvatarUrl));


        }
        
        
    }
}