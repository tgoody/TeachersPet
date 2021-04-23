using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using TeachersPet.BaseModules;
using TeachersPet.CustomControls;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseInfo.CheaterManager {
    public partial class CheaterManagerPage : Page, CourseInfoModule {
        private CourseModel parentData;
        private DirectoryInfo moduleDirectoryInfo;
        private ObservableCollection<DirectoryInfo> cheaterDirectories;
        private ICollectionView cheaterDirectoriesView;

        public CheaterManagerPage() {
            InitializeComponent();
            cheaterDirectories = new ObservableCollection<DirectoryInfo>();
            cheaterDirectoriesView = CollectionViewSource.GetDefaultView(cheaterDirectories);
            CheaterDirectoryListView.ItemsSource = cheaterDirectories;
        }

        public string GetName() {
            return "Cheater Manager";
        }

        public void SetParentData(object parentInstance) {
            var courseInfoPage = parentInstance as CourseInfoPage;
            parentData = courseInfoPage?.CourseModel;
        }

        public void InitializeData() {
            var workingDirectory = System.Reflection.Assembly.GetExecutingAssembly().Location;
            workingDirectory = Path.GetDirectoryName(workingDirectory);
            var modulePath = Directory.CreateDirectory($"{workingDirectory}/Modules").FullName;
            var cheaterModulePath = Directory.CreateDirectory($"{modulePath}/CheaterManager").FullName;
            moduleDirectoryInfo = Directory.CreateDirectory($"{cheaterModulePath}/{parentData.Id}");
            var streamWriter = new StreamWriter($"{moduleDirectoryInfo}/Directories.txt", true);
            streamWriter.Close();
            SetCheaterDirectories();
        }

        private void AddDirectory(object sender, RoutedEventArgs e) {
            ViewDirectoryButton.Visibility = Visibility.Collapsed;
            DeleteDirectoryButton.Visibility = Visibility.Collapsed;
            RunReportButton.Visibility = Visibility.Collapsed;

            var dialog = new CommonOpenFileDialog
                {InitialDirectory = $"{moduleDirectoryInfo.FullName}", IsFolderPicker = true};
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                AppendFileToDirectories(dialog.FileName);
                SetCheaterDirectories();
            }

        }

        private void DirectorySelected(object sender, MouseButtonEventArgs e) {
            DeleteDirectoryButton.Visibility = Visibility.Visible;
            ViewDirectoryButton.Visibility = Visibility.Visible;
            RunReportButton.Visibility = Visibility.Visible;
        }

        private void ViewDirectory(object sender, RoutedEventArgs e) {
            try {
                Process.Start("explorer.exe", @$"{(CheaterDirectoryListView.SelectedItem as DirectoryInfo).FullName}");
            }
            catch (Exception exception) {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = exception.Message;
            }
        }

        private void RunReport(object sender, RoutedEventArgs e) {
            NavigationService?.Navigate(new RunReportPage(CheaterDirectoryListView.SelectedItem as DirectoryInfo));
        }

        private void DeleteDirectory(object sender, RoutedEventArgs e) {
            ConfirmDeleteButton.Visibility = Visibility.Visible;
            DeleteFolderFromFilesystemCheckbox.Visibility = Visibility.Visible;
            DeleteFolderFromFilesystemCheckbox.IsChecked = false;
        }

        private void ConfirmDelete(object sender, RoutedEventArgs e) {
            try {
                var fileName = (CheaterDirectoryListView.SelectedItem as DirectoryInfo)?.FullName;
                if (DeleteFolderFromFilesystemCheckbox.IsChecked.Value) {
                    Directory.Delete(fileName, true);
                }
                RemoveFileFromDirectories(fileName);
                SetCheaterDirectories();
                ConfirmDeleteButton.Visibility = Visibility.Collapsed;
                DeleteFolderFromFilesystemCheckbox.Visibility = Visibility.Collapsed;
            }
            catch (Exception exception) {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = exception.Message;
            }
        }

        private void SetCheaterDirectories() {
            cheaterDirectories.Clear();
            
            try {
                var streamReader = new StreamReader($"{moduleDirectoryInfo}/Directories.txt");
                string path;
                while (!string.IsNullOrEmpty(path = streamReader.ReadLine())) {
                    try {
                        var newDir = new DirectoryInfo(path);
                        //make sure there are no duplicates
                        if (cheaterDirectories.All(dir => dir.FullName != newDir.FullName))
                            cheaterDirectories.Add(new DirectoryInfo(path));
                    }
                    catch (Exception) {
                        //Catch bad directory paths (maybe if files don't exist anymore?)
                    }
                }
                streamReader.Close();
            }
            //create file if does not exist -- shouldn't ever happen
            catch (FileNotFoundException) {
                var streamWriter = new StreamWriter($"{moduleDirectoryInfo}/Directories.txt", true);
                streamWriter.Close();
            }

            cheaterDirectoriesView.Refresh();
        }

        private void AppendFileToDirectories(string additionalPath) {
            var pathsInFile = File.ReadAllLines($"{moduleDirectoryInfo}/Directories.txt");
            var streamWriter = new StreamWriter($"{moduleDirectoryInfo}/Directories.txt", true);
            if (!pathsInFile.Contains(additionalPath)) {
                streamWriter.WriteLine(additionalPath);
            }
            streamWriter.Close();
        }

        private void RemoveFileFromDirectories(string pathToRemove) {
            var pathsInFile = File.ReadAllLines($"{moduleDirectoryInfo}/Directories.txt").ToList();
            var pathsRemoved = pathsInFile.RemoveAll(path => path == pathToRemove);
            //if found paths to remove (should always be true, but just making sure)
            if (pathsRemoved > 0) {
                //delete and rewrite file
                File.Delete($"{moduleDirectoryInfo}/Directories.txt");
                var streamWriter = new StreamWriter($"{moduleDirectoryInfo}/Directories.txt", true);
                pathsInFile.ForEach(path => streamWriter.WriteLine(path));
                streamWriter.Close();
            }
        }
    }
}