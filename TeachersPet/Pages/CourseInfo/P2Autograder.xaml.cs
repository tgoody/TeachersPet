using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TeachersPet.BaseModules;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseInfo {
    public partial class P2Autograder : Page, CourseInfoModule {

        private P2AutograderService _p2AutograderService;
        private string pathToModuleFolder;
        public P2Autograder() {
            InitializeComponent();
            _p2AutograderService = new P2AutograderService();
        }

        public string GetName() {
            return "Project 2 Autograder";
        }

        public void SetParentData(object parentInstance) {
            return;
        }

        public void InitializeData() {
            var workingDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
            workingDirectory = Path.GetDirectoryName(workingDirectory);
            var modulePath = Directory.CreateDirectory($"{workingDirectory}/Modules").FullName;
            pathToModuleFolder = Directory.CreateDirectory($"{modulePath}/P2Autograder").FullName;
        }

        private void DragDropBox_DragEnter(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.Copy;
        }

        private void DragDropBox_DragDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            _p2AutograderService.Init(pathToModuleFolder, files[0]);
            DragDropText.Text = "File Loaded";
            GradeButton.Visibility = Visibility.Visible;
        }


        private void RunGrader(object sender, MouseButtonEventArgs e) {

            try {
                _p2AutograderService.RunSetup();
                GradeButton.Visibility = Visibility.Collapsed;
                
            }
            catch (Exception exception) {
                ErrorText.Visibility = Visibility.Visible;
                ErrorText.Text = exception.Message;
            }

        }

        private void ExamplesDragDropBox_OnDrop(object sender, DragEventArgs e) {
            throw new NotImplementedException();
        }

        private void InputDragDropBox_OnDrop(object sender, DragEventArgs e) {
            throw new NotImplementedException();
        }
    }
}