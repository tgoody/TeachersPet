using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseAssignments.AssignmentInfo {
    public partial class AssignmentStatistics : Page, AssignmentInfoModule {
        private class StatisticData {
            public SectionModel Section { get; set; }
            public double AssignmentAverageForSection { get; set; }
        }
        
        
        private AssignmentModel _assignmentModel;
        private List<SubmissionModel> _submissionsForAssignment;
        private List<SectionModel> _sections;
        private ObservableCollection<StatisticData> _statisticDataObjects;
        
        public AssignmentStatistics() {
            InitializeComponent();
            _statisticDataObjects = new ObservableCollection<StatisticData>();
            StatisticList.ItemsSource = _statisticDataObjects;
        }

        public string GetName() {
            return "View Statistics";
        }

        public void SetParentData(object parentInstance) {
            var parentPage = parentInstance as AssignmentInfo;
            _assignmentModel = parentPage.Assignment;
        }

        public void InitializeData() {
            var task1 = Task.Run(() => {
                _submissionsForAssignment = CanvasAPI.GetSubmissionsFromCourseAndAssignmentId(_assignmentModel.CourseId, _assignmentModel.Id).Result
                    .ToObject<List<SubmissionModel>>();
            });
            var task2 = Task.Run(() => {
                _sections = CanvasAPI.GetSectionsFromCourseId(_assignmentModel.CourseId).Result
                    .ToObject<List<SectionModel>>();
            });

            Task.Run(async () => {
                Console.WriteLine("trying to wait on these tasks");
                Console.WriteLine(task1.Status);
                Console.WriteLine(task2.Status);
                await Task.WhenAll(task1, task2);
                foreach (var section in _sections) {
                    var stat = new StatisticData {
                        Section = section,
                        AssignmentAverageForSection = CalculateMeanForSection(section)
                    };
                    Dispatcher.InvokeAsync(() => _statisticDataObjects.Add(stat));
                }
            });
        }

        private double CalculateMeanForSection(SectionModel sectionModel) {
            try {
                var sectionSubmissions = _submissionsForAssignment.Where(submission =>
                    sectionModel.Students.Select(student => student.Id).Contains(submission.UserId)).ToList();
                return sectionSubmissions.Select(submission => double.Parse(submission.Score)).ToList().Average();
            }
            catch {
                return 0.0;
            }
        }
        
        
        
        
    }
}