using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace screenshareav.Views
{
    public partial class TabbedMonitorHost : Window
    {
        public TabbedMonitorHost()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
