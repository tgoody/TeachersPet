using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
using TeachersPet.Models;

namespace TeachersPet {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        //Developer could add other partial class definitions to add more variables if they wanted
        //Or just change this one, because at that point, I wouldn't know the difference.

        public static CourseModel CurrentClassData { get; set; }
        public static List<AssignmentModel> CurrentClassAssignmentModels { get; set; }


        private App() {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            var baseModules = types.Where(type => type.Namespace?.Contains("BaseModules") ?? false);

            foreach (var baseModule in baseModules) {
                var inheritedTypes =
                    types.Where(type => type.IsClass && !type.IsAbstract && baseModule.IsAssignableFrom(type)).ToList();
                
                Current.Properties.Add(baseModule, inheritedTypes);
            }

            Current.Properties.Add("CanvasAPIUrl", "https://ufl.instructure.com/api/v1/");
            Current.Properties.Add("BetaCanvasAPIUrl", "https://ufl.beta.instructure.com/api/v1/");
        }

    }
}