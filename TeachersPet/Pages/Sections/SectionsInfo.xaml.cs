using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using TeachersPet.BaseModules;
using TeachersPet.Models;
using TeachersPet.Pages.CourseInfo;
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
    }
}