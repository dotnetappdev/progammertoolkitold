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
                new GeneralView(),           // 0 - General
                new MonitorSharingView(),    // 1 - Monitor Sharing
                new RemoteControlView(),     // 2 - Remote Control
                new FileSharingView(),       // 3 - File Sharing
                new FtpClientView(),         // 4 - FTP Client
                new CodeEditorView(),        // 5 - Code Editor
                new EncryptionToolsView(),   // 6 - Encryption Tools
                new APITestingView(),        // 7 - API Testing
                new RdpView(),               // 8 - RDP
                new SettingsView()           // 9 - Settings
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
