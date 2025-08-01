using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Text.RegularExpressions;

namespace ProgrammersToolKit.Views
{
    public partial class TextStripperView : UserControl
    {
        public TextStripperView()
        {
            AvaloniaXamlLoader.Load(this);
            var inputBox = this.FindControl<TextBox>("InputBox");
            var outputBox = this.FindControl<TextBox>("OutputBox");
            var stripBtn = this.FindControl<Button>("StripBtn");
            if (stripBtn != null && inputBox != null && outputBox != null)
            {
                stripBtn.Click += (s, e) =>
                {
                    // Remove all non-printable, illegal, or unwanted characters
                    var input = inputBox.Text ?? string.Empty;
                    // Allow only printable ASCII and common whitespace
                    var stripped = Regex.Replace(input, "[^\x20-\x7E\r\n\t]", "");
                    outputBox.Text = stripped;
                };
            }
        }
    }
}
