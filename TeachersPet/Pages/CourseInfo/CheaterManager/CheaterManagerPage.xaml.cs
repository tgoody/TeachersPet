using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            SetCheaterDirectories();
        }

        private void AddDirectory(object sender, RoutedEventArgs e) {
            NewDirectoryName.Visibility = Visibility.Visible;
            ConfirmAddButton.Visibility = Visibility.Visible;
            ViewDirectoryButton.Visibility = Visibility.Collapsed;
            DeleteDirectoryButton.Visibility = Visibility.Collapsed;
            RunReportButton.Visibility = Visibility.Collapsed;
        }

        private void ConfirmAdd(object sender, RoutedEventArgs e) {
            try {
                Directory.CreateDirectory($"{moduleDirectoryInfo.FullName}/{NewDirectoryName.Text}");
                SetCheaterDirectories();
                ConfirmAddButton.Visibility = Visibility.Collapsed;
                NewDirectoryName.Text = string.Empty;
                NewDirectoryName.Visibility = Visibility.Collapsed;
                NewDirectoryName.SetPlaceholder();
            }
            catch (Exception exception) {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = exception.Message;
            }
        }
        
        private void DirectorySelected(object sender, MouseButtonEventArgs e) {
            DeleteDirectoryButton.Visibility = Visibility.Visible;
            ViewDirectoryButton.Visibility = Visibility.Visible;
            RunReportButton.Visibility = Visibility.Visible;
        }

        private void ViewDirectory(object sender, RoutedEventArgs e) {
            try {
                Process.Start("explorer.exe",@$"{(CheaterDirectoryListView.SelectedItem as DirectoryInfo).FullName}");
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
        }
        
        private void ConfirmDelete(object sender, RoutedEventArgs e) {
            try {
                Directory.Delete((CheaterDirectoryListView.SelectedItem as DirectoryInfo)?.FullName, true);
                SetCheaterDirectories();
                ConfirmDeleteButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception exception) {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = exception.Message;
            }
        }

        private void SetCheaterDirectories() {
            cheaterDirectories.Clear();
            foreach (var cheaterDir in moduleDirectoryInfo.GetDirectories()) {
                cheaterDirectories.Add(cheaterDir);
            } 
            cheaterDirectoriesView.Refresh();
        }
    }
}