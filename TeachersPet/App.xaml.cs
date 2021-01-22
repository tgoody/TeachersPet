using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TeachersPet {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        
        private App() {

            var types = System.Reflection.Assembly.GetExecutingAssembly().GetTypes();
            var baseModules = types.Where(type => type.Namespace?.Contains("BaseModules") ?? false);

            foreach (var baseModule in baseModules) {

                //var moduleList = createList(baseModule);
                var inheritedTypes =
                    types.Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(baseModule)).ToList();
                // foreach (var type in inheritedTypes) {
                //     moduleList.Add(type);
                // }

                Current.Properties.Add(baseModule, inheritedTypes);
            }

        }
        
        
        
        
        
        
        
        // public IList createList(Type type)
        // {
        //     Type genericListType = typeof(List<>).MakeGenericType(type);
        //     return (IList)Activator.CreateInstance(genericListType);
        // }
        
        
        
    }
}