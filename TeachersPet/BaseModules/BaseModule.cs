namespace TeachersPet.BaseModules {
    public interface BaseModule {
        string GetName();
        void SetParentData(object parentInstance);
        void InitializeData();
    }
}