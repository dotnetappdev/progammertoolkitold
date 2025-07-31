using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace screenshareav.Views
{
    public partial class CodeEditorView : UserControl
    {
        private ComboBox? _languageCombo;
        private ComboBox? _encodingCombo;
        private CheckBox? _showHiddenChars;
        private Button? _openFileBtn;
        private Button? _saveFileBtn;
        private Button? _runBtn;
        private TextBlock? _editorStatus;
        private TextBox? _outputBox;
        // TODO: Integrate Monaco/AvaloniaEdit editor control

        public CodeEditorView()
        {
            AvaloniaXamlLoader.Load(this);
            _languageCombo = this.FindControl<ComboBox>("LanguageCombo");
            _encodingCombo = this.FindControl<ComboBox>("EncodingCombo");
            _showHiddenChars = this.FindControl<CheckBox>("ShowHiddenChars");
            _openFileBtn = this.FindControl<Button>("OpenFileBtn");
            _saveFileBtn = this.FindControl<Button>("SaveFileBtn");
            _runBtn = this.FindControl<Button>("RunBtn");
            _editorStatus = this.FindControl<TextBlock>("EditorStatus");
            _outputBox = this.FindControl<TextBox>("OutputBox");

            if (_openFileBtn != null)
                _openFileBtn.Click += OpenFileBtn_Click;
            if (_saveFileBtn != null)
                _saveFileBtn.Click += SaveFileBtn_Click;
            if (_runBtn != null)
                _runBtn.Click += RunBtn_Click;
        }

        private async void OpenFileBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = (Window)this.VisualRoot!;
            var files = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false
            });
            if (files != null && files.Count > 0)
            {
                var encoding = GetSelectedEncoding();
                using var stream = await files[0].OpenReadAsync();
                using var reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true);
                var text = await reader.ReadToEndAsync();
                // TODO: Set text in Monaco/AvaloniaEdit editor
                SetStatus($"Opened {files[0].Name} ({encoding.EncodingName})", Brushes.Green);
            }
        }

        private async void SaveFileBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var window = (Window)this.VisualRoot!;
            var file = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedFileName = "code.txt"
            });
            if (file != null)
            {
                var encoding = GetSelectedEncoding();
                // TODO: Get text from Monaco/AvaloniaEdit editor
                var text = ""; // Placeholder
                using var stream = await file.OpenWriteAsync();
                using var writer = new StreamWriter(stream, encoding);
                await writer.WriteAsync(text);
                SetStatus($"Saved {file.Name} ({encoding.EncodingName})", Brushes.Blue);
            }
        }

        private async void RunBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // TODO: Get code from Monaco/AvaloniaEdit editor
            var code = ""; // Placeholder
            var lang = _languageCombo?.SelectedIndex ?? 0;
            _outputBox!.Text = "Running...";
            switch (lang)
            {
                case 0: // C#
                    _outputBox.Text = await RunCSharpAsync(code);
                    break;
                case 1: // Python
                    _outputBox.Text = await RunPythonAsync(code);
                    break;
                case 2: // JavaScript
                    _outputBox.Text = await RunJavaScriptAsync(code);
                    break;
                default:
                    _outputBox.Text = "Run not supported for this language.";
                    break;
            }
        }

        // Stubs for code execution (to be implemented)
        private Task<string> RunCSharpAsync(string code)
        {
            // TODO: Use Roslyn scripting or external process
            return Task.FromResult("[C# execution not yet implemented]");
        }
        private Task<string> RunPythonAsync(string code)
        {
            // TODO: Use embedded Python or external process
            return Task.FromResult("[Python execution not yet implemented]");
        }
        private Task<string> RunJavaScriptAsync(string code)
        {
            // TODO: Use Jint or external process
            return Task.FromResult("[JavaScript execution not yet implemented]");
        }

        private Encoding GetSelectedEncoding()
        {
            var idx = _encodingCombo?.SelectedIndex ?? 0;
            return idx switch
            {
                1 => Encoding.Unicode, // UTF-16
                2 => Encoding.UTF32,
                _ => Encoding.UTF8
            };
        }

        private void SetStatus(string msg, IBrush color)
        {
            if (_editorStatus != null)
            {
                _editorStatus.Text = msg;
                _editorStatus.Foreground = color;
            }
        }
    }
}
