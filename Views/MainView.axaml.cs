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
                new FtpClientView(),
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
                switch (sidebar.SelectedIndex)
                {
                    case 0:
                        mainContent.Content = _pages[0];
                        break;
                    case 1:
                        mainContent.Content = _pages[1];
                        break;
                    case 2:
                        mainContent.Content = _pages[2];
                        break;
                    case 3:
                        mainContent.Content = _pages[3];
                        break;
                    case 4:
                        mainContent.Content = new FtpClientView();
                        break;
                    case 5:
                        mainContent.Content = new CodeEditorView();
                        break;
                    case 6:
                        mainContent.Content = new EncryptionToolsView();
                        break;
                    case 7:
                        mainContent.Content = new RdpView();
                        break;
                    case 8:
                        mainContent.Content = new SettingsView();
                        break;
                }
            }
        }
    }
}
