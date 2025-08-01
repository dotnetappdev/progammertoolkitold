using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using Avalonia.VisualTree;
using System.Text;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ProgrammersToolKit.Views
{
    public partial class CtfToolsView : UserControl
    {
        private Button? _inspectHeadersBtn;
        private Button? _jwtDecodeBtn;
        private Button? _hashCrackBtn;
        private Button? _base64Btn;
        private Button? _urlBtn;
        private Button? _uuidBtn;
        private Button? _regexBtn;
        private Button? _quickHashBtn;
        private Button? _jwtBruteBtn;
        private TextBlock? _ctfStatusBlock;
        private Dictionary<string, string>? _lastResponseHeaders;

        public CtfToolsView()
        {
            AvaloniaXamlLoader.Load(this);
            _inspectHeadersBtn = this.FindControl<Button>("InspectHeadersBtn");
            _jwtDecodeBtn = this.FindControl<Button>("JwtDecodeBtn");
            _hashCrackBtn = this.FindControl<Button>("HashCrackBtn");
            _base64Btn = this.FindControl<Button>("Base64Btn");
            _urlBtn = this.FindControl<Button>("UrlBtn");
            _uuidBtn = this.FindControl<Button>("UuidBtn");
            _regexBtn = this.FindControl<Button>("RegexBtn");
            _quickHashBtn = this.FindControl<Button>("QuickHashBtn");
            _jwtBruteBtn = this.FindControl<Button>("JwtBruteBtn");
            _ctfStatusBlock = this.FindControl<TextBlock>("CtfStatusBlock");

            // These buttons are now handled by new dialog methods or removed
            if (_base64Btn != null)
                _base64Btn.Click += (s, e) => ShowBase64Dialog();
            if (_urlBtn != null)
                _urlBtn.Click += (s, e) => ShowUrlDialog();
            if (_uuidBtn != null)
                _uuidBtn.Click += (s, e) => ShowUuidDialog();
            if (_regexBtn != null)
                _regexBtn.Click += (s, e) => ShowRegexDialog();
            if (_quickHashBtn != null)
                _quickHashBtn.Click += (s, e) => ShowQuickHashDialog();
            if (_jwtBruteBtn != null)
                _jwtBruteBtn.Click += (s, e) => ShowJwtBruteDialog();
        }
        // --- New CTF Tool Dialogs ---
        private void ShowBase64Dialog()
        {
            var btnPanel = new StackPanel();
            btnPanel.Orientation = Avalonia.Layout.Orientation.Horizontal;
            btnPanel.Margin = new Thickness(0, 0, 0, 0);
            btnPanel.Spacing = 6;
            var encodeBtn = new Button { Content = "Encode", Name = "Base64EncodeBtn" };
            var decodeBtn = new Button { Content = "Decode", Name = "Base64DecodeBtn" };
            var copyBtn = new Button { Content = "Copy", Name = "Base64CopyBtn" };
            btnPanel.Children.Add(encodeBtn);
            btnPanel.Children.Add(decodeBtn);
            btnPanel.Children.Add(copyBtn);
            var input = new TextBox { Name = "Base64InputBox", AcceptsReturn = true, Height = 60 };
            var result = new TextBox { Name = "Base64ResultBox", AcceptsReturn = true, Height = 60, IsReadOnly = true };
            var dlg = new Window
            {
                Title = "Base64 Encode/Decode",
                Width = 500,
                Height = 300,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock { Text = "Enter text or base64:", FontWeight = Avalonia.Media.FontWeight.Bold },
                        input,
                        btnPanel,
                        result
                    }
                }
            };
            encodeBtn.Click += (_,__) => {
                if (input != null && result != null)
                {
                    try { result.Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(input.Text ?? "")); } catch { result.Text = "Error encoding."; }
                }
            };
            decodeBtn.Click += (_,__) => {
                if (input != null && result != null)
                {
                    try { result.Text = Encoding.UTF8.GetString(Convert.FromBase64String(input.Text ?? "")); } catch { result.Text = "Error decoding."; }
                }
            };
            copyBtn.Click += async (_,__) => {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard != null && result != null)
                    await clipboard.SetTextAsync(result.Text ?? "");
            };
            var owner = this.GetVisualRoot() as Window;
            if (owner != null) dlg.ShowDialog(owner); else dlg.Show();
        }

        private void ShowUrlDialog()
        {
            var btnPanel = new StackPanel();
            btnPanel.Orientation = Avalonia.Layout.Orientation.Horizontal;
            btnPanel.Margin = new Thickness(0, 0, 0, 0);
            btnPanel.Spacing = 6;
            var encodeBtn = new Button { Content = "Encode", Name = "UrlEncodeBtn" };
            var decodeBtn = new Button { Content = "Decode", Name = "UrlDecodeBtn" };
            var copyBtn = new Button { Content = "Copy", Name = "UrlCopyBtn" };
            btnPanel.Children.Add(encodeBtn);
            btnPanel.Children.Add(decodeBtn);
            btnPanel.Children.Add(copyBtn);
            var input = new TextBox { Name = "UrlInputBox", AcceptsReturn = true, Height = 60 };
            var result = new TextBox { Name = "UrlResultBox", AcceptsReturn = true, Height = 60, IsReadOnly = true };
            var dlg = new Window
            {
                Title = "URL Encode/Decode",
                Width = 500,
                Height = 300,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Spacing = 8,
                    Children =
                    {
                        new TextBlock { Text = "Enter text or URL:", FontWeight = Avalonia.Media.FontWeight.Bold },
                        input,
                        btnPanel,
                        result
                    }
                }
            };
            encodeBtn.Click += (_,__) => { if (input != null && result != null) result.Text = Uri.EscapeDataString(input.Text ?? ""); };
            decodeBtn.Click += (_,__) => { if (input != null && result != null) { try { result.Text = Uri.UnescapeDataString(input.Text ?? ""); } catch { result.Text = "Error decoding."; } } };
            copyBtn.Click += async (_,__) => {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard != null && result != null)
                    await clipboard.SetTextAsync(result.Text ?? "");
            };
            var owner = this.GetVisualRoot() as Window;
            if (owner != null) dlg.ShowDialog(owner); else dlg.Show();
        }

        private void ShowUuidDialog()
        {
            var genBtn = new Button { Content = "Generate", Name = "UuidGenBtn" };
            var result = new TextBox { Name = "UuidResultBox", IsReadOnly = true };
            var dlg = new Window
            {
                Title = "UUID/GUID Generator",
                Width = 400,
                Height = 200,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Spacing = 8,
                    Children =
                    {
                        genBtn,
                        result
                    }
                }
            };
            genBtn.Click += (_,__) => { if (result != null) result.Text = Guid.NewGuid().ToString(); };
            var owner = this.GetVisualRoot() as Window;
            if (owner != null) dlg.ShowDialog(owner); else dlg.Show();
        }

        private void ShowRegexDialog()
        {
            var pattern = new TextBox { Name = "RegexPatternBox", Watermark = "Pattern (e.g. \\d+)" };
            var input = new TextBox { Name = "RegexInputBox", Watermark = "Input text..." };
            var result = new TextBox { Name = "RegexResultBox", IsReadOnly = true };
            var testBtn = new Button { Content = "Test", Name = "RegexTestBtn" };
            var dlg = new Window
            {
                Title = "Regex Tester",
                Width = 500,
                Height = 300,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Spacing = 8,
                    Children =
                    {
                        pattern,
                        input,
                        testBtn,
                        result
                    }
                }
            };
            testBtn.Click += (_,__) => {
                if (pattern != null && input != null && result != null)
                {
                    try {
                        var matches = System.Text.RegularExpressions.Regex.Matches(input.Text ?? "", pattern.Text ?? "");
                        result.Text = string.Join(", ", matches.Cast<System.Text.RegularExpressions.Match>().Select(m => m.Value));
                    } catch { result.Text = "Invalid pattern."; }
                }
            };
            var owner = this.GetVisualRoot() as Window;
            if (owner != null) dlg.ShowDialog(owner); else dlg.Show();
        }

        private void ShowQuickHashDialog()
        {
            var btnPanel = new StackPanel();
            btnPanel.Orientation = Avalonia.Layout.Orientation.Horizontal;
            btnPanel.Margin = new Thickness(0, 0, 0, 0);
            btnPanel.Spacing = 6;
            var md5Btn = new Button { Content = "MD5", Name = "HashMd5Btn" };
            var sha1Btn = new Button { Content = "SHA1", Name = "HashSha1Btn" };
            var sha256Btn = new Button { Content = "SHA256", Name = "HashSha256Btn" };
            var copyBtn = new Button { Content = "Copy", Name = "HashCopyBtn" };
            btnPanel.Children.Add(md5Btn);
            btnPanel.Children.Add(sha1Btn);
            btnPanel.Children.Add(sha256Btn);
            btnPanel.Children.Add(copyBtn);
            var input = new TextBox { Name = "HashInputBox", Watermark = "Enter text..." };
            var result = new TextBox { Name = "HashResultBox", IsReadOnly = true };
            var dlg = new Window
            {
                Title = "Quick Hash Generator",
                Width = 500,
                Height = 300,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Spacing = 8,
                    Children =
                    {
                        input,
                        btnPanel,
                        result
                    }
                }
            };
            md5Btn.Click += (_,__) => { if (input != null && result != null) result.Text = HashString(input.Text ?? "", "MD5"); };
            sha1Btn.Click += (_,__) => { if (input != null && result != null) result.Text = HashString(input.Text ?? "", "SHA1"); };
            sha256Btn.Click += (_,__) => { if (input != null && result != null) result.Text = HashString(input.Text ?? "", "SHA256"); };
            copyBtn.Click += async (_,__) => {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                if (clipboard != null && result != null)
                    await clipboard.SetTextAsync(result.Text ?? "");
            };
            var owner = this.GetVisualRoot() as Window;
            if (owner != null) dlg.ShowDialog(owner); else dlg.Show();
        }

        private void ShowJwtBruteDialog()
        {
            var dlg = new Window
            {
                Title = "JWT Signature Brute-force",
                Width = 600,
                Height = 400,
                Content = new StackPanel
                {
                    Margin = new Thickness(16),
                    Spacing = 8,
                    Children =
                    {
                        new TextBox { Name = "JwtBruteTokenBox", Watermark = "JWT token..." },
                        new TextBox { Name = "JwtBruteWordlistBox", Watermark = "Wordlist (comma or newline separated)...", AcceptsReturn = true, Height = 60 },
                        new Button { Content = "Brute-force", Name = "JwtBruteBtn" },
                        new TextBox { Name = "JwtBruteResultBox", IsReadOnly = true }
                    }
                }
            };
            dlg.Opened += (s, e) => {
                var tokenBox = dlg.FindControl<TextBox>("JwtBruteTokenBox");
                var wordlistBox = dlg.FindControl<TextBox>("JwtBruteWordlistBox");
                var result = dlg.FindControl<TextBox>("JwtBruteResultBox");
                var bruteBtn = dlg.FindControl<Button>("JwtBruteBtn");
            if (bruteBtn != null && result != null && tokenBox != null && wordlistBox != null)
            {
                bruteBtn.Click += async (_,__) => {
                    result.Text = "Running...";
                    await System.Threading.Tasks.Task.Run(() => {
                        try {
                            var parts = (tokenBox.Text ?? "").Split('.');
                            if (parts.Length != 3) { result.Text = "Invalid JWT"; return; }
                            var wordlist = (wordlistBox.Text ?? "").Split(new[]{'\n',',',';'}, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var key in wordlist) {
                                var sig = Base64UrlEncode(HmacSha256(parts[0] + "." + parts[1], key.Trim()));
                                if (sig == parts[2]) { result.Text = $"Key: {key.Trim()}"; return; }
                            }
                            result.Text = "No match.";
                        } catch { result.Text = "Error."; }
                    });
                };
            }
            };
            var owner = this.GetVisualRoot() as Window;
            if (owner != null) dlg.ShowDialog(owner); else dlg.Show();
        }

        // --- Helper methods for new tools ---
        private static string HashString(string input, string algo)
        {
            using System.Security.Cryptography.HashAlgorithm hash = algo switch {
                "MD5" => System.Security.Cryptography.MD5.Create(),
                "SHA1" => System.Security.Cryptography.SHA1.Create(),
                _ => System.Security.Cryptography.SHA256.Create()
            };
            var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-","").ToLower();
        }

        private static byte[] HmacSha256(string data, string key)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(key));
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        private static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input).TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        // This should be set externally by the API tester after a request
        public void SetLastResponseHeaders(Dictionary<string, string>? headers)
        {
            _lastResponseHeaders = headers;
        }
    }
}


