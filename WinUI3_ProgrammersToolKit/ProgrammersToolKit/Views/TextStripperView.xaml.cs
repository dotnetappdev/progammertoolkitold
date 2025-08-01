using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.RegularExpressions;

namespace ProgrammersToolKit.Views
{
    public sealed partial class TextStripperView : UserControl
    {
        public TextStripperView()
        {
            this.InitializeComponent();
            
            StripBtn.Click += (s, e) =>
            {
                // Remove all non-printable, illegal, or unwanted characters
                var input = InputBox.Text ?? string.Empty;
                // Allow only printable ASCII and common whitespace
                var stripped = Regex.Replace(input, "[^\x20-\x7E\r\n\t]", "");
                OutputBox.Text = stripped;
            };
        }
    }
}