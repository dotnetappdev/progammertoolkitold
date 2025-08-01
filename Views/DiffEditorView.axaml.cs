using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Avalonia.Media;

namespace ProgrammersToolKit.Views
{
    public partial class DiffEditorView : UserControl
    {
        private TextEditor? _editor1;
        private TextEditor? _editor2;
        private Button? _loadFile1Btn;
        private Button? _loadFile2Btn;
        private Button? _compareBtn;
        private TextBlock? _diffSummary;
        private string? _file1Path;
        private string? _file2Path;
        private TextBox? _findBox;
        private TextBox? _replaceBox;
        private Button? _findBtn;
        private Button? _replaceBtn;
        private TextBlock? _file1Stats;
        private TextBlock? _file2Stats;
        private TextBlock? _file1Encoding;
        private TextBlock? _file2Encoding;
        private Button? _saveFile1Btn;
        private Button? _saveFile2Btn;

        public DiffEditorView()
        {
            AvaloniaXamlLoader.Load(this);
            _editor1 = this.FindControl<TextEditor>("Editor1");
            _editor2 = this.FindControl<TextEditor>("Editor2");
            _loadFile1Btn = this.FindControl<Button>("LoadFile1Btn");
            _loadFile2Btn = this.FindControl<Button>("LoadFile2Btn");
            _compareBtn = this.FindControl<Button>("CompareBtn");
            _diffSummary = this.FindControl<TextBlock>("DiffSummary");
            _findBox = this.FindControl<TextBox>("FindBox");
            _replaceBox = this.FindControl<TextBox>("ReplaceBox");
            _findBtn = this.FindControl<Button>("FindBtn");
            _replaceBtn = this.FindControl<Button>("ReplaceBtn");
            _file1Stats = this.FindControl<TextBlock>("File1Stats");
            _file2Stats = this.FindControl<TextBlock>("File2Stats");
            _file1Encoding = this.FindControl<TextBlock>("File1Encoding");
            _file2Encoding = this.FindControl<TextBlock>("File2Encoding");
            _saveFile1Btn = this.FindControl<Button>("SaveFile1Btn");
            _saveFile2Btn = this.FindControl<Button>("SaveFile2Btn");

            if (_loadFile1Btn != null)
                _loadFile1Btn.Click += async (s, e) => await LoadFileAsync(_editor1, 1);
            if (_loadFile2Btn != null)
                _loadFile2Btn.Click += async (s, e) => await LoadFileAsync(_editor2, 2);
            if (_compareBtn != null)
                _compareBtn.Click += (s, e) => CompareEditors();
            if (_findBtn != null)
                _findBtn.Click += (s, e) => FindInEditors();
            if (_replaceBtn != null)
                _replaceBtn.Click += (s, e) => ReplaceInEditors();
            if (_saveFile1Btn != null)
                _saveFile1Btn.Click += async (s, e) => await SaveFileAsync(_editor1, _file1Path);
            if (_saveFile2Btn != null)
                _saveFile2Btn.Click += async (s, e) => await SaveFileAsync(_editor2, _file2Path);
            if (_editor1 != null) _editor1.TextChanged += (s, e) => UpdateStatsAndEncoding();
            if (_editor2 != null) _editor2.TextChanged += (s, e) => UpdateStatsAndEncoding();
        }

        private async System.Threading.Tasks.Task LoadFileAsync(TextEditor? editor, int fileNum)
        {
            if (editor == null) return;
            var visualRoot = Avalonia.VisualTree.VisualExtensions.GetVisualRoot(this);
            var window = visualRoot as Window;
            if (window?.StorageProvider == null) return;
            var files = await window.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = $"Open File {fileNum}",
                AllowMultiple = false
            });
            var file = files?.FirstOrDefault();
            if (file != null)
            {
                using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream, true);
                var text = await reader.ReadToEndAsync();
                editor.Text = text;
                var encoding = reader.CurrentEncoding;
                if (fileNum == 1) {
                    _file1Path = file.Path.LocalPath;
                    if (_file1Encoding != null) _file1Encoding.Text = $"Encoding: {encoding.EncodingName}";
                } else {
                    _file2Path = file.Path.LocalPath;
                    if (_file2Encoding != null) _file2Encoding.Text = $"Encoding: {encoding.EncodingName}";
                }
                UpdateStatsAndEncoding();
            }
        }
        private async System.Threading.Tasks.Task SaveFileAsync(TextEditor? editor, string? path)
        {
            if (editor == null) return;
            var visualRoot = Avalonia.VisualTree.VisualExtensions.GetVisualRoot(this);
            var window = visualRoot as Window;
            if (window?.StorageProvider == null) return;
            var file = await window.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Save File",
                SuggestedFileName = path != null ? System.IO.Path.GetFileName(path) : "untitled.txt",
                FileTypeChoices = new[] { new Avalonia.Platform.Storage.FilePickerFileType("Text File") { Patterns = new[] { "*.txt", "*.*" } } }
            });
            if (file != null)
            {
                using var stream = await file.OpenWriteAsync();
                using var writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
                await writer.WriteAsync(editor.Text ?? "");
            }
        }

        private void FindInEditors()
        {
            var find = _findBox?.Text ?? string.Empty;
            if (string.IsNullOrEmpty(find)) return;
            if (_editor1 != null) HighlightFind(_editor1, find);
            if (_editor2 != null) HighlightFind(_editor2, find);
        }

        private void ReplaceInEditors()
        {
            var find = _findBox?.Text ?? string.Empty;
            var replace = _replaceBox?.Text ?? string.Empty;
            if (string.IsNullOrEmpty(find)) return;
            if (_editor1 != null && !string.IsNullOrEmpty(_editor1.Text))
                _editor1.Text = _editor1.Text.Replace(find, replace);
            if (_editor2 != null && !string.IsNullOrEmpty(_editor2.Text))
                _editor2.Text = _editor2.Text.Replace(find, replace);
            UpdateStatsAndEncoding();
        }

        private void HighlightFind(TextEditor editor, string find)
        {
            // For demo: just select first occurrence
            var idx = editor.Text?.IndexOf(find, StringComparison.OrdinalIgnoreCase) ?? -1;
            if (idx >= 0)
            {
                editor.SelectionStart = idx;
                editor.SelectionLength = find.Length;
                editor.ScrollToLine(editor.Document.GetLineByOffset(idx).LineNumber);
            }
        }

        private void UpdateStatsAndEncoding()
        {
            if (_editor1 != null && _file1Stats != null)
                _file1Stats.Text = GetStats(_editor1.Text);
            if (_editor2 != null && _file2Stats != null)
                _file2Stats.Text = GetStats(_editor2.Text);
        }

        private string GetStats(string? text)
        {
            if (string.IsNullOrEmpty(text)) return "0 words, 0 chars";
            var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            var chars = text.Length;
            return $"{words} words, {chars} chars";
        }

        private void CompareEditors()
        {
            if (_editor1 == null || _editor2 == null) return;
            var lines1 = _editor1.Text?.Split('\n') ?? Array.Empty<string>();
            var lines2 = _editor2.Text?.Split('\n') ?? Array.Empty<string>();
            var diffs = DiffLines(lines1, lines2);
            HighlightDiffs(_editor1, diffs.LeftDiffs);
            HighlightDiffs(_editor2, diffs.RightDiffs);
            if (_diffSummary != null)
                _diffSummary.Text = $"{diffs.LeftDiffs.Count} changes in File 1, {diffs.RightDiffs.Count} in File 2.";
        }

        private (List<int> LeftDiffs, List<int> RightDiffs) DiffLines(string[] left, string[] right)
        {
            var leftDiffs = new List<int>();
            var rightDiffs = new List<int>();
            int max = Math.Max(left.Length, right.Length);
            for (int i = 0; i < max; i++)
            {
                var l = i < left.Length ? left[i] : null;
                var r = i < right.Length ? right[i] : null;
                if (l != r)
                {
                    if (l != null) leftDiffs.Add(i);
                    if (r != null) rightDiffs.Add(i);
                }
            }
            return (leftDiffs, rightDiffs);
        }

        private void HighlightDiffs(TextEditor editor, List<int> diffLines)
        {
            editor.TextArea.TextView.LineTransformers.Clear();
            if (diffLines.Count == 0) return;
            editor.TextArea.TextView.LineTransformers.Add(new DiffLineColorizer(diffLines));
            editor.TextArea.TextView.InvalidateLayer(AvaloniaEdit.Rendering.KnownLayer.Background);
        }
    }

    public class DiffLineColorizer : AvaloniaEdit.Rendering.DocumentColorizingTransformer
    {
        private readonly HashSet<int> _diffLines;
        public DiffLineColorizer(IEnumerable<int> diffLines)
        {
            _diffLines = new HashSet<int>(diffLines);
        }
        protected override void ColorizeLine(AvaloniaEdit.Document.DocumentLine line)
        {
            if (_diffLines.Contains(line.LineNumber - 1))
            {
                ChangeLinePart(line.Offset, line.EndOffset, (element) =>
                {
                    // Use dynamic to avoid direct IBrush reference and avoid missing assembly error
                    var brush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.LightPink);
                    dynamic props = element.TextRunProperties;
                    props.BackgroundBrush = brush;
                });
            }
        }
    }
}
