using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TeachersPet.Modules.BaseModules;

namespace TeachersPet.Services {
    public static class GenerateModuleService {
        public static void CreateWrapPanel(Type moduleType, ref WrapPanel panel) {
            
            if (!App.Current.Properties.Contains(moduleType)) return;
            
            foreach (Type subType in (IList) App.Current.Properties[moduleType]) {
                var module = (BaseModule) Activator.CreateInstance(subType);
                var newButton = new Button {
                    Content = module?.GetName(),
                    Tag = module
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