using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using screenshareav.Database;

namespace screenshareav.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            AvaloniaXamlLoader.Load(this);
            LoadSettings();
        }

        private void LoadSettings()
        {
            var themeComboBox = this.FindControl<ComboBox>("ThemeComboBox");
            var startWithSystemCheckBox = this.FindControl<CheckBox>("StartWithSystemCheckBox");
            var minimizeToTrayCheckBox = this.FindControl<CheckBox>("MinimizeToTrayCheckBox");
            var enableLoggingCheckBox = this.FindControl<CheckBox>("EnableLoggingCheckBox");
            var enableEthicalHackingCheckBox = this.FindControl<CheckBox>("EnableEthicalHackingCheckBox");
            var enablePenetrationTestingCheckBox = this.FindControl<CheckBox>("EnablePenetrationTestingCheckBox");
            var logSecurityEventsCheckBox = this.FindControl<CheckBox>("LogSecurityEventsCheckBox");

            // Load theme setting
            var themeSetting = DatabaseManager.GetSetting("Theme") ?? "System Default";
            themeComboBox!.SelectedIndex = themeSetting switch
            {
                "Light" => 1,
                "Dark" => 2,
                _ => 0 // System Default
            };

            // Load other settings
            startWithSystemCheckBox!.IsChecked = DatabaseManager.GetSetting("StartWithSystem") == "true";
            minimizeToTrayCheckBox!.IsChecked = DatabaseManager.GetSetting("MinimizeToTray") == "true";
            enableLoggingCheckBox!.IsChecked = DatabaseManager.GetSetting("EnableLogging") == "true";
            enableEthicalHackingCheckBox!.IsChecked = DatabaseManager.GetSetting("EnableEthicalHacking") == "true";
            enablePenetrationTestingCheckBox!.IsChecked = DatabaseManager.GetSetting("EnablePenetrationTesting") == "true";
            logSecurityEventsCheckBox!.IsChecked = DatabaseManager.GetSetting("LogSecurityEvents") == "true";
        }

        private void ThemeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem != null)
            {
                var selectedTheme = comboBox.SelectedItem.ToString();
                ApplyTheme(selectedTheme);
            }
        }

        private void ApplyTheme(string? theme)
        {
            if (Application.Current != null)
            {
                var themeVariant = theme switch
                {
                    "Light" => ThemeVariant.Light,
                    "Dark" => ThemeVariant.Dark,
                    _ => ThemeVariant.Default // System Default
                };

                Application.Current.RequestedThemeVariant = themeVariant;
            }
        }

        private void SaveSettingsButton_Click(object? sender, RoutedEventArgs e)
        {
            var themeComboBox = this.FindControl<ComboBox>("ThemeComboBox");
            var startWithSystemCheckBox = this.FindControl<CheckBox>("StartWithSystemCheckBox");
            var minimizeToTrayCheckBox = this.FindControl<CheckBox>("MinimizeToTrayCheckBox");
            var enableLoggingCheckBox = this.FindControl<CheckBox>("EnableLoggingCheckBox");
            var enableEthicalHackingCheckBox = this.FindControl<CheckBox>("EnableEthicalHackingCheckBox");
            var enablePenetrationTestingCheckBox = this.FindControl<CheckBox>("EnablePenetrationTestingCheckBox");
            var logSecurityEventsCheckBox = this.FindControl<CheckBox>("LogSecurityEventsCheckBox");

            // Save theme setting
            var themeSetting = themeComboBox!.SelectedIndex switch
            {
                1 => "Light",
                2 => "Dark",
                _ => "System Default"
            };
            DatabaseManager.SetSetting("Theme", themeSetting);

            // Save other settings
            DatabaseManager.SetSetting("StartWithSystem", startWithSystemCheckBox!.IsChecked?.ToString().ToLower() ?? "false");
            DatabaseManager.SetSetting("MinimizeToTray", minimizeToTrayCheckBox!.IsChecked?.ToString().ToLower() ?? "false");
            DatabaseManager.SetSetting("EnableLogging", enableLoggingCheckBox!.IsChecked?.ToString().ToLower() ?? "false");
            DatabaseManager.SetSetting("EnableEthicalHacking", enableEthicalHackingCheckBox!.IsChecked?.ToString().ToLower() ?? "false");
            DatabaseManager.SetSetting("EnablePenetrationTesting", enablePenetrationTestingCheckBox!.IsChecked?.ToString().ToLower() ?? "false");
            DatabaseManager.SetSetting("LogSecurityEvents", logSecurityEventsCheckBox!.IsChecked?.ToString().ToLower() ?? "false");

            // Apply theme immediately
            ApplyTheme(themeSetting);
        }

        private void ResetButton_Click(object? sender, RoutedEventArgs e)
        {
            // Reset all settings to defaults
            var themeComboBox = this.FindControl<ComboBox>("ThemeComboBox");
            var startWithSystemCheckBox = this.FindControl<CheckBox>("StartWithSystemCheckBox");
            var minimizeToTrayCheckBox = this.FindControl<CheckBox>("MinimizeToTrayCheckBox");
            var enableLoggingCheckBox = this.FindControl<CheckBox>("EnableLoggingCheckBox");
            var enableEthicalHackingCheckBox = this.FindControl<CheckBox>("EnableEthicalHackingCheckBox");
            var enablePenetrationTestingCheckBox = this.FindControl<CheckBox>("EnablePenetrationTestingCheckBox");
            var logSecurityEventsCheckBox = this.FindControl<CheckBox>("LogSecurityEventsCheckBox");

            themeComboBox!.SelectedIndex = 0; // System Default
            startWithSystemCheckBox!.IsChecked = false;
            minimizeToTrayCheckBox!.IsChecked = false;
            enableLoggingCheckBox!.IsChecked = false;
            enableEthicalHackingCheckBox!.IsChecked = false;
            enablePenetrationTestingCheckBox!.IsChecked = false;
            logSecurityEventsCheckBox!.IsChecked = false;

            ApplyTheme("System Default");
        }
    }
}
