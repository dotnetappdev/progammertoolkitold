using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProgrammersToolKit.Views
{
    public sealed partial class MainView : UserControl
    {
        private UserControl[] _pages;

        public MainView()
        {
            this.InitializeComponent();
            
            // Initialize pages array with converted views
            _pages = new UserControl[]
            {
                new GeneralView(),            // 0
                new GeneralView(),            // 1 - placeholder for MonitorSharingView
                new GeneralView(),            // 2 - placeholder for RemoteControlView
                new GeneralView(),            // 3 - placeholder for FileSharingView
                new GeneralView(),            // 4 - placeholder for FtpClientView
                new GeneralView(),            // 5 - placeholder for CodeEditorView
                new EncryptionToolsView(),    // 6 - EncryptionToolsView (converted)
                new GeneralView(),            // 7 - placeholder for RdpView
                new ApiTesterView(),          // 8 - ApiTesterView (converted)
                new GeneralView(),            // 9 - placeholder for JsonQueryView
                new GeneralView(),            // 10 - placeholder for DiffEditorView
                new NotepadView(),            // 11 - NotepadView (converted)
                new GeneralView(),            // 12 - placeholder for CtfToolsView
                new TextStripperView(),       // 13 - TextStripperView (converted)
                new SettingsView()            // 14 - SettingsView (converted)
            };

            // Set initial content
            MainContent.Content = _pages[0];
            
            // Wire up sidebar selection
            Sidebar.SelectionChanged += Sidebar_SelectionChanged;
        }

        private void Sidebar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Sidebar.SelectedIndex >= 0 && Sidebar.SelectedIndex < _pages.Length)
            {
                MainContent.Content = _pages[Sidebar.SelectedIndex];
            }
        }
    }
}