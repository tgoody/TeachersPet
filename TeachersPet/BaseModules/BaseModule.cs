namespace TeachersPet.BaseModules {
    public interface BaseModule {
        string GetName();
        /// <summary>
        /// Interface function that takes in the parent page object to be used for passing data to inherited instances
        /// </summary>
        /// <param name="parentInstance">Parent instance as a Page object</param>
        void SetParentData(object parentInstance);
        /// <summary>
        /// Interface function used to initialize data outside the constructor after <see cref="SetParentData"/> is called
        /// </summary>
        /// <remarks>
        /// <para>Useful when using parent data to make an API request in a thread (as this shouldn't be done in the constructor) </para>
        /// <para> Strongly recommend this code to be asynchronous (use Task.Run) </para>
        /// </remarks>
        void InitializeData();
    }
}