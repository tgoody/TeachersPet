using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Pages.CourseInfo;
using TeachersPet.Pages.Settings;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseList {
    public partial class CourseListPage : Page {
        private ObservableCollection<ListViewItem> listData = new ObservableCollection<ListViewItem>();
        
        public CourseListPage() {
            InitializeComponent();
        }
        
        public CourseListPage(JArray courses) {
            InitializeComponent();
            ListView.ItemsSource = listData;
            CreateListViewFromCourses(courses);
        }
        
        private void CreateListViewFromCourses(JArray courses) {
            var courseData = new List<CourseModel>();
            try {
                //Turn JArray into Course list and sort
                courseData.AddRange(courses.Select(course => course.ToObject<CourseModel>()));
                courseData = courseData.OrderBy(c => c.StartDateTime).ToList();
                
                //For all courses, put them into semester categories
                var currentSemesterString = "";
                foreach (var course in courseData) {
                    var semesterString = CreateTermStringFromDateTime((DateTime)course.StartDateTime);
                    if (currentSemesterString != semesterString) {
                        currentSemesterString = semesterString;
                        var semesterHeader = new ListViewItem {
                            Style = TryFindResource("SemesterHeader") as Style, Content = currentSemesterString
                        };
                        listData.Add(semesterHeader);
                    }
                    var courseItem = new ListViewItem {
                        DataContext = course, Style = TryFindResource("CourseButton") as Style, Tag = course,
                        Content = $"{course.CourseCode}: {course.CourseName}"
                    };
                    listData.Add(courseItem);
                }
            }
            catch (Exception e) {
                Console.Error.WriteLine("Error populating course list");
                Console.Error.WriteLine(e);
            }
        }

        private static string CreateTermStringFromDateTime(DateTime semesterDateTime) {

            if (semesterDateTime == DateTime.MaxValue) {
                return "Other";
            }
            
            var monthOfDateTime = semesterDateTime.Month;
            var yearOfDateTime = semesterDateTime.Year;

            if (monthOfDateTime >= 1 && monthOfDateTime <= 4) {
                return $"Spring {yearOfDateTime}";
            }
            if (monthOfDateTime >= 4 && monthOfDateTime <= 7) {
                return $"Summer {yearOfDateTime}";
            }
            if (monthOfDateTime >= 8 && monthOfDateTime <= 12) {
                return $"Fall {yearOfDateTime}";
            }
            throw new Exception("Error finding month?");
        }
        
        
        private void CourseButtonClick(object sender, RoutedEventArgs e) {
            var item = sender as ListViewItem;
            NavigationService?.Navigate(new CourseInfoPage(item?.Tag as CourseModel));
        }

        private void OpenSettingsPage(object sender, RoutedEventArgs e) {
            NavigationService?.Navigate(new SettingsPage());
        }
    }
}