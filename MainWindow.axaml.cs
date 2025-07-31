
using Avalonia.Controls;
using screenshareav.Database;

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
            if (theme == "System")
            {
                resources["AccentColor"] = resources["AccentColor"];
                resources["BackgroundColor"] = resources["BackgroundColor"];
                resources["SidebarColor"] = resources["SidebarColor"];
                resources["ButtonColor"] = resources["ButtonColor"];
                resources["ButtonHoverColor"] = resources["ButtonHoverColor"];
                resources["TextColor"] = resources["TextColor"];
            }
            else if (theme == "Light")
            {
                resources["AccentColor"] = resources["AccentColorLight"];
                resources["BackgroundColor"] = resources["BackgroundColorLight"];
                resources["SidebarColor"] = resources["SidebarColorLight"];
                resources["ButtonColor"] = resources["ButtonColorLight"];
                resources["ButtonHoverColor"] = resources["ButtonHoverColorLight"];
                resources["TextColor"] = resources["TextColorLight"];
            }
            else if (theme == "Dark")
            {
                resources["AccentColor"] = resources["AccentColorDark"];
                resources["BackgroundColor"] = resources["BackgroundColorDark"];
                resources["SidebarColor"] = resources["SidebarColorDark"];
                resources["ButtonColor"] = resources["ButtonColorDark"];
                resources["ButtonHoverColor"] = resources["ButtonHoverColorDark"];
                resources["TextColor"] = resources["TextColorDark"];
            }
        }
    }
}