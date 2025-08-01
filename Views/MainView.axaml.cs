using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using screenshareav.Views;

namespace screenshareav.Views
{
    public partial class MainView : UserControl
    {
        private UserControl[] _pages;

        public MainView()
        {
            AvaloniaXamlLoader.Load(this);
            _pages = new UserControl[]
            {
                new GeneralView(),
                new MonitorSharingView(),
                new RemoteControlView(),
                new FileSharingView(),
                new FtpClientView(),
                new CodeEditorView(),
                new EncryptionToolsView(),
                new ApiTestView(),
                new RdpView(),
                new SettingsView()
            };
            var sidebar = this.FindControl<ListBox>("Sidebar");
            if (sidebar != null)
            {
                sidebar.SelectionChanged += Sidebar_SelectionChanged;
            }
            var mainContent = this.FindControl<ContentControl>("MainContent");
            if (mainContent != null)
            {
                mainContent.Content = _pages[0];
            }
        }

        private void Sidebar_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var sidebar = sender as ListBox;
            var mainContent = this.FindControl<ContentControl>("MainContent");
            if (sidebar?.SelectedIndex >= 0 && sidebar.SelectedIndex < _pages.Length && mainContent != null)
            {
                mainContent.Content = _pages[sidebar.SelectedIndex];
            }
        }
    }
}
