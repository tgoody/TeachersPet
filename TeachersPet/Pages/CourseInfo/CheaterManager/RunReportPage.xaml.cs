using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using TeachersPet.Services;

namespace TeachersPet.Pages.CourseInfo.CheaterManager {
    public partial class RunReportPage : Page {

        public class MOSSLanguageObject {
            public string LanguageCode { get; set; }
            public string LanguageDisplayString { get; set; }
            public List<string> AcceptedExtensions { get; set; }
        }


        private DirectoryInfo _workingDirectory;
        private List<MOSSLanguageObject> availableLanguages;
        private List<string> excludedFiles;
        private string resultHyperLink;

        public string ResultHyperlink => resultHyperLink;

        public RunReportPage(DirectoryInfo directoryToRunReport) {
            InitializeComponent();
            _workingDirectory = directoryToRunReport;
            InitializeLanguages();
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
                    AcceptedExtensions = new List<string>{".cpp", ".h", ".hpp", ".cc"}
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
                var language = LanguageComboBox.SelectedItem as MOSSLanguageObject;
                var numSimilarLines = int.Parse(NumSimilarLinesTextBox.Text);
                var numReports = int.Parse(NumReportsTextBox.Text);
                var nameOfReport = ReportNameTextBox.Text;
                Task.Run(() => {
                    resultHyperLink = MossReportService.RunReportOnDirectory(_workingDirectory, language, numSimilarLines,
                        numReports, nameOfReport, excludedFiles).Trim();
                    if (string.IsNullOrEmpty(resultHyperLink)) {
                        throw new Exception("No link available for MOSS");
                    }
                    Dispatcher.Invoke(() => {
                        LoadingTextBlock.Visibility = Visibility.Collapsed;
                        ResultsButton.Visibility = Visibility.Visible;
                    });
                });
                LoadingTextBlock.Visibility = Visibility.Visible;
            }
            catch (Exception exception) {
                ErrorMessageBlock.Visibility = Visibility.Visible;
                ErrorMessageBlock.Text = exception.Message;
            }
        }

        private void ViewResults(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo(resultHyperLink){UseShellExecute = true});
        }
    }
}