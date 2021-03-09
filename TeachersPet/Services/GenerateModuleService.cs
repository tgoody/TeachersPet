using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Newtonsoft.Json.Linq;
using TeachersPet.BaseModules;

namespace TeachersPet.Services {
    public static class GenerateModuleService {

        /*TODO: Look into way of allowing child modules to define their own button click functions
            This would mean that its button could act differently if it wanted to, instead of just 
            calling the default constructor and navigating to its page.
            Currently we are relying on data stored on an application level.
        */
        /// <summary>
        /// Takes in a xaml WrapPanel and creates the buttons necessary for modules.
        /// Modules must implement "moduleType".
        /// WrapPanel must be passed in as ref.
        /// </summary>
        /// <param name="moduleType"></param>
        /// <param name="panel"></param>
        /// <param name="parentObj"></param>
        public static void CreateWrapPanel(Type moduleType, ref WrapPanel panel, object parentObj) {
            
            if (!App.Current.Properties.Contains(moduleType)) return;
            
            foreach (Type subType in (IList) App.Current.Properties[moduleType]) {
                //TODO: some way to store data in module interface so we could use it to pass data around here?
                //Like we could take the current object, copy it, store it as a reference?
                var module = (BaseModule) Activator.CreateInstance(subType);
                module.SetParentData(parentObj);
                module.InitializeData();
                var newButton = new Button {
                    Content = module.GetName(),
                    Tag = module,
                    Style = (Style)App.Current.Resources["ModuleButton"]
                };
                newButton.Click += (Button_Click);
                panel.Children.Add(newButton);
            }

        }
        private static void Button_Click(object sender, RoutedEventArgs e) {
            var button = sender as Button;
            (Application.Current.MainWindow as MainWindow)?.Frame.NavigationService.Navigate(button.Tag);
        }
    }
}