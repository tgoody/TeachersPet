using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            InitialSetupButton.Visibility = Visibility.Visible;
        }


        private void SetupGrader(object sender, MouseButtonEventArgs e) {

            try {
                InitialSetupButton.Visibility = Visibility.Collapsed;
                DragDropText.Text = "Making students' programs now...";
                _p2AutograderService.TaskScore = int.Parse(TaskScore.Text);
                _p2AutograderService.EcScore = int.Parse(ECScore.Text);
                var task = _p2AutograderService.RunSetup();
                Task.Run(() => {
                    while (!task.IsCompleted) {
                        Dispatcher.Invoke(() => {
                            ProgressText.Visibility = Visibility.Visible;
                            ProgressText.Text =
                                $"{_p2AutograderService.NumProjectsMade}/{_p2AutograderService.NumStudentFolders}";
                        });
                        Thread.Sleep(1000);
                    }
                    
                    Dispatcher.Invoke(() => {
                        ProgressText.Visibility = Visibility.Collapsed;
                        DragDropBox.Visibility = Visibility.Collapsed;
                        DragDropText.Visibility = Visibility.Collapsed;
                        ExamplesDragDropBox.Visibility = Visibility.Visible;
                        InputDragDropBox.Visibility = Visibility.Visible;
                    });
                });
                
                TaskScorePanel.Visibility = Visibility.Collapsed;
                ECScorePanel.Visibility = Visibility.Collapsed;
            }
            catch (Exception exception) {
                ErrorText.Visibility = Visibility.Visible;
                ErrorText.Text = exception.Message;
            }
        }

        private void ExamplesDragDropBox_OnDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            _p2AutograderService.SetExamplesDirectory(files[0]);
            ExampleGradient.GradientStops.ElementAt(0).Color = (Color)ColorConverter.ConvertFromString("#7F07A332");
            if (_p2AutograderService.InputFolderDirectory.Length != 0 && _p2AutograderService.ExampleFolderDirectory.Length != 0) {
                CopyImagesButton.Visibility = Visibility.Visible;
            }
        }

        private void InputDragDropBox_OnDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            _p2AutograderService.SetInputDirectory(files[0]);
            InputGradient.GradientStops.ElementAt(0).Color = (Color)ColorConverter.ConvertFromString("#7F07A332");
            if (_p2AutograderService.InputFolderDirectory.Length != 0 && _p2AutograderService.ExampleFolderDirectory.Length != 0) {
                CopyImagesButton.Visibility = Visibility.Visible;
            }
        }

        private void CopyImages(object sender, MouseButtonEventArgs e) {

            CopyImagesButton.Visibility = Visibility.Collapsed;
            DragDropText.Text = "Copying images to student directories...";
            var task = Task.Run(() => _p2AutograderService.CopyImagesToStudentFolders());
            Task.Run(() => {
                while (!task.IsCompleted) {
                    Dispatcher.Invoke(() => {
                        DragDropText.Visibility = Visibility.Visible;
                        ProgressText.Visibility = Visibility.Visible;
                        ProgressText.Text =
                            $"{_p2AutograderService.NumFoldersImagesCopiedTo}/{_p2AutograderService.NumStudentFolders}";
                    });
                    Thread.Sleep(1000);
                }

                Dispatcher.Invoke(() => {
                    DragDropText.Visibility = Visibility.Collapsed;
                    ProgressText.Visibility = Visibility.Collapsed;
                    InputDragDropBox.Visibility = Visibility.Collapsed;
                    ExamplesDragDropBox.Visibility = Visibility.Collapsed;
                    RunGraderButton.Visibility = Visibility.Visible;
                });
            });
        }

        private void RunGrader(object sender, MouseButtonEventArgs e) {
            var task = Task.Run(() => _p2AutograderService.RunGrader());
            DragDropText.Text = "Grading students...";
            RunGraderButton.Visibility = Visibility.Collapsed;

            Task.Run(() => {
                while(!task.IsCompleted) {
                    Dispatcher.Invoke(() => {
                        DragDropText.Visibility = Visibility.Visible;
                        ProgressText.Visibility = Visibility.Visible;
                        ProgressText.Text =
                            $"{_p2AutograderService.NumStudentsGraded}/{_p2AutograderService.NumStudentFolders}";
                    });
                    
                    Thread.Sleep(1000);
                }

                Dispatcher.Invoke(() => {
                    DragDropText.Visibility = Visibility.Collapsed;
                    DragDropText.Visibility = Visibility.Visible;
                    DragDropText.Text = "All finished! Please view the StudentScores.txt and StudentLogs.txt files.";
                });
            });
        }

        private void OpenDirectory(object sender, MouseButtonEventArgs e) {
            Process.Start(_p2AutograderService.ModulePath);
        }
    }
}