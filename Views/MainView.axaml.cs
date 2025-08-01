using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace ProgrammersToolKit.Views
{
    public partial class MainView : UserControl
    {
        private UserControl[] _pages;

        public MainView()
        {
            AvaloniaXamlLoader.Load(this);
            var ctfToolsView = new CtfToolsView();
            var apiTesterView = new ApiTesterView { CtfToolsViewInstance = ctfToolsView };
            _pages = new UserControl[]
            {
                new GeneralView(),            // 0
                new MonitorSharingView(),      // 1
                new RemoteControlView(),       // 2
                new FileSharingView(),         // 3
                new FtpClientView(),           // 4
                new CodeEditorView(),          // 5
                new EncryptionToolsView(),     // 6
                new RdpView(),                 // 7
                apiTesterView,                 // 8
                new JsonQueryView(),           // 9
                new DiffEditorView(),          // 10
                new NotepadView(),             // 11
                ctfToolsView,                  // 12
                new TextStripperView(),        // 13
                new SettingsView()             // 14
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

            // Update sidebar selection to handle new TextStripperView
            if (sidebar != null)
            {
                sidebar.SelectionChanged += (s, e) =>
                {
                    if (mainContent != null && sidebar.SelectedIndex >= 0 && sidebar.SelectedIndex < _pages.Length)
                    {
                        mainContent.Content = _pages[sidebar.SelectedIndex];
                    }
                };
            }

            // Wire up CTF sidebar buttons
            var inspectHeadersBtn = this.FindControl<Button>("SidebarInspectHeadersBtn");
            var jwtDecodeBtn = this.FindControl<Button>("SidebarJwtDecodeBtn");
            var hashCrackBtn = this.FindControl<Button>("SidebarHashCrackBtn");
            if (inspectHeadersBtn != null)
                inspectHeadersBtn.Click += (s, e) => ctfToolsView.GetType().GetMethod("ShowLastResponseHeaders", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(ctfToolsView, null);
            if (jwtDecodeBtn != null)
                jwtDecodeBtn.Click += (s, e) => ctfToolsView.GetType().GetMethod("ShowJwtDecodeDialog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(ctfToolsView, null);
            if (hashCrackBtn != null)
                hashCrackBtn.Click += (s, e) => ctfToolsView.GetType().GetMethod("ShowHashCrackDialog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(ctfToolsView, null);
            var cookieDecodeBtn = this.FindControl<Button>("SidebarCookieDecodeBtn");
            if (cookieDecodeBtn != null)
                cookieDecodeBtn.Click += (s, e) => ctfToolsView.GetType().GetMethod("ShowCookieDecodeDialog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(ctfToolsView, null);
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
