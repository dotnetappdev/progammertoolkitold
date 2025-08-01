using Microsoft.UI.Xaml;

namespace ProgrammersToolKit
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(null);
        }
    }
}