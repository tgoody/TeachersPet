using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace TeachersPet {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        
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
        
        
        
        
        
        
        
        // public IList createList(Type type)
        // {
        //     Type genericListType = typeof(List<>).MakeGenericType(type);
        //     return (IList)Activator.CreateInstance(genericListType);
        // }
        
        
        
    }
}