using System;
using System.Windows;
using System.Runtime;
using System.Windows.Controls;
using System.Windows.Media;
using TeachersPet.Services;

namespace TeachersPet.Pages.Settings {
    public partial class SettingsPage : Page {

        
        public SettingsPage() {
            InitializeComponent();
            BetaModeCheckBox.IsChecked = CanvasAPI.CanvasBetaMode;
        }

        //Note: setting API token will reset cache
        private void TryToSetAPIToken(object sender, RoutedEventArgs e) {

            var apiToken = NewAPITokenTextBox.Text;
            if (!string.IsNullOrEmpty(apiToken)) {
                var oldToken = AuthenticationProvider.Instance().ApiToken;
                try {
                    Environment.SetEnvironmentVariable("CanvasAPIToken", apiToken, EnvironmentVariableTarget.User);
                    if (Environment.GetEnvironmentVariable("CanvasAPIToken", EnvironmentVariableTarget.User) != apiToken) {
                        throw new Exception("Failed to correctly set token in Environment.");
                    }
                }
                catch (Exception exception) {
                    TestSetAPITokenResponseTextBlock.Text = $"{exception.Message}";
                    TestSetAPITokenResponseTextBlock.Visibility = Visibility.Visible;
                    TestSetAPITokenResponseTextBlock.Foreground = Brushes.Red;
                }

                AuthenticationProvider.Instance().CanvasLogin();
                var response = CanvasAPI.GetCourseList().Result;
                if (!(response.Count > 0 && !string.IsNullOrEmpty((string)response[0]["course_code"]))) {
                    CanvasAPI.SetBearerToken(oldToken);
                    TestSetAPITokenResponseTextBlock.Text =
                        "Canvas rejected API Token, confirm token entered correctly.";
                    TestSetAPITokenResponseTextBlock.Visibility = Visibility.Visible;
                    TestSetAPITokenResponseTextBlock.Foreground = Brushes.Red;
                }
                else {
                    TestSetAPITokenResponseTextBlock.Text =
                        "New token set correctly. Cache has been cleared.";
                    TestSetAPITokenResponseTextBlock.Visibility = Visibility.Visible;
                    TestSetAPITokenResponseTextBlock.Foreground = Brushes.Green;
                }
            }
        }

        private void ClearCache(object sender, RoutedEventArgs e) {
            ConfirmClearCacheButton.Visibility = Visibility.Visible;
            ConfirmClearCacheTextBlock.Visibility = Visibility.Visible;
            ClearCacheWarning.Visibility = Visibility.Visible;
            ClearedSuccessfullyTextBlock.Visibility = Visibility.Collapsed;
        }

        private void ConfirmClearCache(object sender, RoutedEventArgs e) {
            CacheService.ClearCache();
            ClearedSuccessfullyTextBlock.Visibility = Visibility.Visible;
            ConfirmClearCacheButton.Visibility = Visibility.Collapsed;
            ConfirmClearCacheTextBlock.Visibility = Visibility.Collapsed;
            ClearCacheWarning.Visibility = Visibility.Collapsed;
        }

        private void BetaModeOn(object sender, RoutedEventArgs e) {
            CanvasAPI.TurnBetaModeOn();
            BetaModeResponse.Visibility = Visibility.Visible;
            BetaModeResponse.Text = "Beta mode activated";
            BetaModeResponse.Foreground = Brushes.Green;
        }

        private void BetaModeOff(object sender, RoutedEventArgs e) {
            CanvasAPI.TurnBetaModeOff();
            BetaModeResponse.Visibility = Visibility.Visible;
            BetaModeResponse.Text = "Beta mode deactivated. Changes will now be reflected in production";
            BetaModeResponse.Foreground = Brushes.Red;
        }
    }
}