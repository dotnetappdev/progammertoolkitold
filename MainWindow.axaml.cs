
using Avalonia.Controls;
using Avalonia.Styling;
using screenshareav.Database;
using System.Runtime.InteropServices;

namespace screenshareav
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var systemRadio = this.FindControl<RadioButton>("SystemThemeRadio");
            var lightRadio = this.FindControl<RadioButton>("LightThemeRadio");
            var darkRadio = this.FindControl<RadioButton>("DarkThemeRadio");

            // Load theme from DB
            string? savedTheme = DatabaseManager.GetSetting("Theme");
            switch (savedTheme)
            {
                case "Light":
                    if (lightRadio != null) lightRadio.IsChecked = true;
                    SetTheme("Light");
                    break;
                case "Dark":
                    if (darkRadio != null) darkRadio.IsChecked = true;
                    SetTheme("Dark");
                    break;
                default:
                    if (systemRadio != null) systemRadio.IsChecked = true;
                    SetTheme("System");
                    break;
            }

            if (systemRadio != null)
                systemRadio.IsCheckedChanged += (s, e) => { if (systemRadio.IsChecked == true) { SetTheme("System"); DatabaseManager.SetSetting("Theme", "System"); } };
            if (lightRadio != null)
                lightRadio.IsCheckedChanged += (s, e) => { if (lightRadio.IsChecked == true) { SetTheme("Light"); DatabaseManager.SetSetting("Theme", "Light"); } };
            if (darkRadio != null)
                darkRadio.IsCheckedChanged += (s, e) => { if (darkRadio.IsChecked == true) { SetTheme("Dark"); DatabaseManager.SetSetting("Theme", "Dark"); } };
        }

        private void SetTheme(string theme)
        {
            var app = Avalonia.Application.Current;
            if (app == null) return;
            var resources = app.Resources;
            
            string actualTheme = theme;
            if (theme == "System")
            {
                actualTheme = DetectSystemTheme();
            }

            if (actualTheme == "Light")
            {
                resources["AccentColor"] = resources["AccentColorLight"];
                resources["BackgroundColor"] = resources["BackgroundColorLight"];
                resources["SidebarColor"] = resources["SidebarColorLight"];
                resources["ButtonColor"] = resources["ButtonColorLight"];
                resources["ButtonHoverColor"] = resources["ButtonHoverColorLight"];
                resources["TextColor"] = resources["TextColorLight"];
                
                // Update window background
                this.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#ffffff"));
            }
            else // Dark theme
            {
                resources["AccentColor"] = resources["AccentColorDark"];
                resources["BackgroundColor"] = resources["BackgroundColorDark"];
                resources["SidebarColor"] = resources["SidebarColorDark"];
                resources["ButtonColor"] = resources["ButtonColorDark"];
                resources["ButtonHoverColor"] = resources["ButtonHoverColorDark"];
                resources["TextColor"] = resources["TextColorDark"];
                
                // Update window background
                this.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#181a20"));
            }
        }

        private string DetectSystemTheme()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return DetectWindowsTheme();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return DetectMacOSTheme();
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return DetectLinuxTheme();
                }
            }
            catch
            {
                // Fallback to light theme if detection fails
            }
            
            return "Light";
        }

        private string DetectWindowsTheme()
        {
            try
            {
                // Check Windows registry for dark mode preference
                var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                if (value is int intValue)
                {
                    return intValue == 0 ? "Dark" : "Light";
                }
            }
            catch
            {
                // Fallback if registry access fails
            }
            
            return "Light";
        }

        private string DetectMacOSTheme()
        {
            try
            {
                // On macOS, we can use NSUserDefaults to check for dark mode
                // This is a simplified approach - in a real app you might use P/Invoke
                var isDark = System.Environment.GetEnvironmentVariable("DARK_MODE") == "1";
                return isDark ? "Dark" : "Light";
            }
            catch
            {
                // Fallback
            }
            
            return "Light";
        }

        private string DetectLinuxTheme()
        {
            try
            {
                // Check common Linux environment variables and settings
                var gtkTheme = System.Environment.GetEnvironmentVariable("GTK_THEME");
                var qtStyle = System.Environment.GetEnvironmentVariable("QT_STYLE_OVERRIDE");
                
                if (!string.IsNullOrEmpty(gtkTheme) && gtkTheme.ToLower().Contains("dark"))
                    return "Dark";
                if (!string.IsNullOrEmpty(qtStyle) && qtStyle.ToLower().Contains("dark"))
                    return "Dark";
            }
            catch
            {
                // Fallback
            }
            
            return "Light";
        }
    }
}