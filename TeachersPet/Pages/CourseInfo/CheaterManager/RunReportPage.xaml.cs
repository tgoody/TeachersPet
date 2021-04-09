using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseInfo.CheaterManager {
    public partial class RunReportPage : Page, INotifyPropertyChanged {

        public class MOSSLanguageObject {
            public string LanguageCode { get; set; }
            public string LanguageDisplayString { get; set; }
            public List<string> AcceptedExtensions { get; set; }
        }


        private DirectoryInfo _workingDirectory;
        private List<MOSSLanguageObject> availableLanguages;
        private List<string> excludedFiles;
        private string resultHyperLink;
        private string consoleOutput = "Working...";
        private bool autoscroll = true;


        public string ConsoleOutput {
            get => consoleOutput;
            set {
                consoleOutput = value;
                RaisePropertyChanged("ConsoleOutput");
            } 
        }
        private void RaisePropertyChanged(string propName) {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propName));

        }
        public event PropertyChangedEventHandler PropertyChanged;


        public string ResultHyperlink => resultHyperLink;

        public RunReportPage(DirectoryInfo directoryToRunReport) {
            InitializeComponent();
            _workingDirectory = directoryToRunReport;
            InitializeLanguages();
            DataContext = this;
            LanguageComboBox.ItemsSource = availableLanguages;
            LanguageComboBox.SelectedIndex = 0;
            excludedFiles =new List<string>();
            ExcludedFileListView.ItemsSource = excludedFiles;
        }
        
        private void InitializeLanguages() {
            availableLanguages = new List<MOSSLanguageObject> {
                new MOSSLanguageObject {
                    LanguageCode = "cc",
                    LanguageDisplayString = "C++",
                    AcceptedExtensions = new List<string>{".cpp", ".h", ".hpp"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "c",
                    LanguageDisplayString = "C",
                    AcceptedExtensions = new List<string>{".c", ".h"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "java",
                    LanguageDisplayString = "Java",
                    AcceptedExtensions = new List<string>{".java"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "python",
                    LanguageDisplayString = "Python",
                    AcceptedExtensions = new List<string>{".py"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "csharp",
                    LanguageDisplayString = "C#",
                    AcceptedExtensions = new List<string>{".cs"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "javascript",
                    LanguageDisplayString = "JavaScript",
                    AcceptedExtensions = new List<string>{".js"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "verilog",
                    LanguageDisplayString = "Verilog",
                    AcceptedExtensions = new List<string>{".v", ".sv"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "mips",
                    LanguageDisplayString = "MIPS",
                    AcceptedExtensions = new List<string>{".s", ".asm", ".mipsel"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "matlab",
                    LanguageDisplayString = "MATLAB",
                    AcceptedExtensions = new List<string>{".m"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "perl",
                    LanguageDisplayString = "Perl",
                    AcceptedExtensions = new List<string>{".pl"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "vhdl",
                    LanguageDisplayString = "VHDL",
                    AcceptedExtensions = new List<string>{".vhdl", ".vhd"}
                },
                new MOSSLanguageObject {
                    LanguageCode = "ascii",
                    LanguageDisplayString = "ASCII (Plain text)",
                    AcceptedExtensions = new List<string>()
                },

            };
        }
        
        private void AddExcludedFile(object sender, RoutedEventArgs e) {
            var filename = ExcludedFileTextBox.Text;
            if (string.IsNullOrEmpty(filename) || excludedFiles.Contains(filename)) return;
            excludedFiles.Add(filename);
            CollectionViewSource.GetDefaultView(excludedFiles).Refresh();
            ExcludedFileTextBox.Text = string.Empty;
            ExcludedFileTextBox.SetPlaceholder();
        }

        private void RemoveExcludedFile(object sender, RoutedEventArgs e) {
            excludedFiles.RemoveAt(ExcludedFileListView.SelectedIndex);
            CollectionViewSource.GetDefaultView(excludedFiles).Refresh();
            RemoveExcludedFileButton.Visibility = Visibility.Collapsed;
        }

        private void ExcludedFileSelected(object sender, MouseButtonEventArgs e) {
            RemoveExcludedFileButton.Visibility = Visibility.Visible;
        }

        private void RunReport(object sender, RoutedEventArgs e) {
            try {
                RunReportButton.Visibility = Visibility.Hidden;
                var language = LanguageComboBox.SelectedItem as MOSSLanguageObject;
                var numSimilarLines = int.Parse(NumSimilarLinesTextBox.Text);
                var numReports = int.Parse(NumReportsTextBox.Text);
                var nameOfReport = ReportNameTextBox.Text;
                Process mossReportProcess = null;
                var useSeparateConsole = SeparateConsoleCheckbox.IsChecked.Value;
                Task.Run(() => {
                    mossReportProcess = MossReportService.RunReportOnDirectory(_workingDirectory, language, numSimilarLines,
                        numReports, nameOfReport, excludedFiles, useSeparateConsole);

                    if (mossReportProcess != null) {
                        Dispatcher.Invoke(() => {
                            LoadingTextBlock.Visibility = Visibility.Collapsed;
                            if (!useSeparateConsole) {
                                ScrollViewer.Visibility = Visibility.Visible;
                                ConsoleOutputTextBlock.Visibility = Visibility.Visible;
                            }
                            else{
                                RunReportButton.Visibility = Visibility.Visible; //bring back button after process made
                            }
                        });
                    }
                    else {
                        throw new Exception("Error starting MOSS process");
                    }
                });
                LoadingTextBlock.Visibility = Visibility.Visible;

                //writing to UI from process
                if (!useSeparateConsole) {
                    Task.Run(() => {

                        while (mossReportProcess == null) {
                            Thread.Sleep(200);
                        }

                        var latestOutput = mossReportProcess.StandardOutput.ReadLine();
                        while (!string.IsNullOrEmpty(latestOutput)) {
                            Dispatcher.Invoke(() => {
                                ConsoleOutput += "\n" + latestOutput;
                            });
                            latestOutput = mossReportProcess.StandardOutput.ReadLine();
                            Console.WriteLine(latestOutput);
                        }

                        Dispatcher.Invoke(() => {
                            resultHyperLink = consoleOutput.Trim().Split('\n')[^1];
                            var result = Uri.TryCreate(resultHyperLink, UriKind.Absolute, out var uriResult) 
                                         && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                            if (!result) {
                                throw new Exception(
                                    "Sorry, we couldn't complete the request to MOSS. \n" +
                                    "This can happen if they're busy. Maybe try again later?");
                            }
                            ResultsButton.Visibility = Visibility.Visible;
                            RunReportButton.Visibility = Visibility.Visible;
                        });

                    });
                }

            }
            catch (Exception exception) {
                ErrorMessageBlock.Visibility = Visibility.Visible;
                ErrorMessageBlock.Text = exception.Message;
                RunReportButton.Visibility = Visibility.Visible;
            }
        }

        private void ViewResults(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo(resultHyperLink){UseShellExecute = true});
        }


        //Found https://stackoverflow.com/a/19315242

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0) { // Content unchanged : user scroll event
                autoscroll = ScrollViewer.VerticalOffset == ScrollViewer.ScrollableHeight;
            }

            // Content scroll event : auto-scroll eventually
            if (autoscroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                ScrollViewer.ScrollToVerticalOffset(ScrollViewer.ExtentHeight);
            }
        }


    }
}