﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TeachersPet.Services;

namespace TeachersPet.Pages {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LoginPage : Page {
        public LoginPage() {
            InitializeComponent();
        }

        public void Button_Click(object sender, RoutedEventArgs routedEventArgs) {
            
            AuthenticationProvider.Instance().CanvasLogin();
            Console.WriteLine(AuthenticationProvider.Instance().ApiToken);
            
            //do something to confirm login worked

            NavigationService?.Navigate(new CourseListPage());
        }
    }
}