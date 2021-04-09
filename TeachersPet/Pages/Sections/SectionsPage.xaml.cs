using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Pages.CourseInfo;
using TeachersPet.Pages.SectionInfo;
using TeachersPet.Services;

namespace TeachersPet.Pages.Sections {
    public partial class Sections : CourseInfoModule {
        private CourseModel _courseModel;
        private ObservableCollection<SectionModel> _sectionList;
        private ICollectionView _sectionView;
        
        public Sections() {
            InitializeComponent();
        }

        public string GetName() {
            return "View Sections";
        }

        public void SetParentData(object parentInstance) {
            _courseModel = (parentInstance as CourseInfoPage)?.CourseModel;
        }

        public void InitializeData() {
            Task.Run(() => {
                _sectionList = CanvasAPI.GetSectionsFromCourseId(_courseModel.Id).Result.ToObject<ObservableCollection<SectionModel>>();
                Dispatcher.Invoke( () => {
                    _sectionView = CollectionViewSource.GetDefaultView(_sectionList);
                    SectionsListView.ItemsSource = _sectionView;
                    _sectionView.Refresh();
                });
            });
        }

        private void ClickSection(object sender, MouseButtonEventArgs e) {
            
            var listBoxItem = sender as ListViewItem;
            var sectionObject = listBoxItem.Tag as SectionModel;
            
            NavigationService.Navigate(new SectionInfoPage(sectionObject));
        }
    }
}