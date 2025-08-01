using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using screenshareav.Database;

namespace screenshareav.Views
{
    public partial class EncryptionToolsView : UserControl
    {
        public EncryptionToolsView()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // JWT Decoding
        private void DecodeJwtBtn_Click(object? sender, RoutedEventArgs e)
        {
            var jwtInput = this.FindControl<TextBox>("JwtInput");
            var jwtDecoded = this.FindControl<TextBlock>("JwtDecoded");
            
            if (jwtInput == null || jwtDecoded == null) return;
            
            try
            {
                var text = jwtInput.Text ?? string.Empty;
                var parts = text.Split('.');
                if (parts.Length < 2) 
                { 
                    jwtDecoded.Text = "Invalid JWT - must have at least 2 parts separated by dots"; 
                    return; 
                }
                
                string DecodeBase64(string s) => 
                    Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(s ?? string.Empty)));
                
                var header = DecodeBase64(parts[0]);
                var payload = DecodeBase64(parts[1]);
                
                // Pretty print JSON
                try
                {
                    var headerJson = JsonSerializer.Serialize(JsonDocument.Parse(header), new JsonSerializerOptions { WriteIndented = true });
                    var payloadJson = JsonSerializer.Serialize(JsonDocument.Parse(payload), new JsonSerializerOptions { WriteIndented = true });
                    jwtDecoded.Text = $"Header:\n{headerJson}\n\nPayload:\n{payloadJson}";
                }
                catch
                {
                    jwtDecoded.Text = $"Header: {header}\nPayload: {payload}";
                }
            }
            catch (Exception ex)
            { 
                jwtDecoded.Text = $"Error decoding JWT: {ex.Message}"; 
            }
        }

        private string PadBase64(string s) => s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');

        // Hash Generation
        private void HashBtn_Click(object? sender, RoutedEventArgs e)
        {
            var hashInput = this.FindControl<TextBox>("HashInput");
            var hashResult = this.FindControl<TextBlock>("HashResult");
            var hashAlgoCombo = this.FindControl<ComboBox>("HashAlgoCombo");
            
            if (hashInput == null || hashResult == null || hashAlgoCombo == null) return;
            
            var text = hashInput.Text ?? string.Empty;
            var algo = hashAlgoCombo.SelectedIndex;
            
            try
            {
                byte[] hash;
                switch (algo)
                {
                    case 1: // MD5
                        using (var md5 = MD5.Create())
                            hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                        break;
                    case 2: // SHA1
                        using (var sha1 = SHA1.Create())
                            hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(text));
                        break;
                    case 3: // SHA512
                        using (var sha512 = SHA512.Create())
                            hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(text));
                        break;
                    default: // SHA256
                        using (var sha256 = SHA256.Create())
                            hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
                        break;
                }
                
                hashResult.Text = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
            catch (Exception ex)
            {
                hashResult.Text = $"Error: {ex.Message}";
            }
        }

        // AES Encryption
        private void AesEncryptBtn_Click(object? sender, RoutedEventArgs e)
        {
            var encryptInput = this.FindControl<TextBox>("EncryptInput");
            var encryptKey = this.FindControl<TextBox>("EncryptKey");
            var encryptResult = this.FindControl<TextBlock>("EncryptResult");
            
            if (encryptInput == null || encryptKey == null || encryptResult == null) return;
            
            try
            {
                var keyText = encryptKey.Text ?? string.Empty;
                if (keyText.Length < 16)
                {
                    encryptResult.Text = "Key must be at least 16 characters long";
                    return;
                }
                
                var key = Encoding.UTF8.GetBytes(keyText.PadRight(32).Substring(0, 32));
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = new byte[16]; // Using zero IV for simplicity - in production use random IV
                
                var encryptor = aes.CreateEncryptor();
                var inputBytes = Encoding.UTF8.GetBytes(encryptInput.Text ?? string.Empty);
                var encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                encryptResult.Text = Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                encryptResult.Text = $"Encryption error: {ex.Message}";
            }
        }

        // AES Decryption
        private void AesDecryptBtn_Click(object? sender, RoutedEventArgs e)
        {
            var encryptInput = this.FindControl<TextBox>("EncryptInput");
            var encryptKey = this.FindControl<TextBox>("EncryptKey");
            var encryptResult = this.FindControl<TextBlock>("EncryptResult");
            
            if (encryptInput == null || encryptKey == null || encryptResult == null) return;
            
            try
            {
                var keyText = encryptKey.Text ?? string.Empty;
                if (keyText.Length < 16)
                {
                    encryptResult.Text = "Key must be at least 16 characters long";
                    return;
                }
                
                var key = Encoding.UTF8.GetBytes(keyText.PadRight(32).Substring(0, 32));
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = new byte[16]; // Using zero IV for simplicity
                
                var decryptor = aes.CreateDecryptor();
                var encryptedBytes = Convert.FromBase64String(encryptInput.Text ?? string.Empty);
                var decrypted = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                encryptResult.Text = Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex)
            {
                encryptResult.Text = $"Decryption error: {ex.Message}";
            }
        }

        // Password Analysis
        private void AnalyzePasswordBtn_Click(object? sender, RoutedEventArgs e)
        {
            var passwordInput = this.FindControl<TextBox>("PasswordInput");
            var passwordStrengthResult = this.FindControl<TextBlock>("PasswordStrengthResult");
            
            if (passwordInput == null || passwordStrengthResult == null) return;
            
            var password = passwordInput.Text ?? string.Empty;
            var score = 0;
            var feedback = new StringBuilder();
            
            // Length check
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;
            if (password.Length >= 16) score++;
            
            // Character variety
            if (Regex.IsMatch(password, @"[a-z]")) score++;
            if (Regex.IsMatch(password, @"[A-Z]")) score++;
            if (Regex.IsMatch(password, @"[0-9]")) score++;
            if (Regex.IsMatch(password, @"[!@#$%^&*(),.?]")) score++;
            
            // Strength assessment
            var strength = score switch
            {
                >= 6 => "🟢 Strong",
                >= 4 => "🟡 Medium",
                >= 2 => "🟠 Weak",
                _ => "🔴 Very Weak"
            };
            
            feedback.AppendLine($"Password Strength: {strength} (Score: {score}/7)");
            feedback.AppendLine($"Length: {password.Length} characters");
            
            if (password.Length < 8) feedback.AppendLine("❌ Too short (minimum 8 characters)");
            if (!Regex.IsMatch(password, @"[a-z]")) feedback.AppendLine("❌ Missing lowercase letters");
            if (!Regex.IsMatch(password, @"[A-Z]")) feedback.AppendLine("❌ Missing uppercase letters");
            if (!Regex.IsMatch(password, @"[0-9]")) feedback.AppendLine("❌ Missing numbers");
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?]")) feedback.AppendLine("❌ Missing special characters");
            
            passwordStrengthResult.Text = feedback.ToString();
        }

        // Password Generation
        private void GeneratePasswordBtn_Click(object? sender, RoutedEventArgs e)
        {
            var passwordLengthSlider = this.FindControl<Slider>("PasswordLengthSlider");
            var generatedPassword = this.FindControl<TextBox>("GeneratedPassword");
            
            if (passwordLengthSlider == null || generatedPassword == null) return;
            
            var length = (int)passwordLengthSlider.Value;
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
            var password = new StringBuilder();
            var random = new Random();
            
            for (int i = 0; i < length; i++)
            {
                password.Append(chars[random.Next(chars.Length)]);
            }
            
            generatedPassword.Text = password.ToString();
        }

        // Base64 Encoding
        private void Base64EncodeBtn_Click(object? sender, RoutedEventArgs e)
        {
            var base64Input = this.FindControl<TextBox>("Base64Input");
            var base64Result = this.FindControl<TextBox>("Base64Result");
            
            if (base64Input == null || base64Result == null) return;
            
            try
            {
                var text = base64Input.Text ?? string.Empty;
                var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
                base64Result.Text = encoded;
            }
            catch (Exception ex)
            {
                base64Result.Text = $"Error: {ex.Message}";
            }
        }

        // Base64 Decoding
        private void Base64DecodeBtn_Click(object? sender, RoutedEventArgs e)
        {
            var base64Input = this.FindControl<TextBox>("Base64Input");
            var base64Result = this.FindControl<TextBox>("Base64Result");
            
            if (base64Input == null || base64Result == null) return;
            
            try
            {
                var text = base64Input.Text ?? string.Empty;
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(text));
                base64Result.Text = decoded;
            }
            catch (Exception ex)
            {
                base64Result.Text = $"Error: {ex.Message}";
            }
        }

        // URL Encoding
        private void UrlEncodeBtn_Click(object? sender, RoutedEventArgs e)
        {
            var urlInput = this.FindControl<TextBox>("UrlInput");
            var urlResult = this.FindControl<TextBox>("UrlResult");
            
            if (urlInput == null || urlResult == null) return;
            
            try
            {
                var text = urlInput.Text ?? string.Empty;
                var encoded = HttpUtility.UrlEncode(text);
                urlResult.Text = encoded;
            }
            catch (Exception ex)
            {
                urlResult.Text = $"Error: {ex.Message}";
            }
        }

        // URL Decoding
        private void UrlDecodeBtn_Click(object? sender, RoutedEventArgs e)
        {
            var urlInput = this.FindControl<TextBox>("UrlInput");
            var urlResult = this.FindControl<TextBox>("UrlResult");
            
            if (urlInput == null || urlResult == null) return;
            
            try
            {
                var text = urlInput.Text ?? string.Empty;
                var decoded = HttpUtility.UrlDecode(text);
                urlResult.Text = decoded;
            }
            catch (Exception ex)
            {
                urlResult.Text = $"Error: {ex.Message}";
            }
        }

        // Port Scanning
        private async void PortScanBtn_Click(object? sender, RoutedEventArgs e)
        {
            var portScanHost = this.FindControl<TextBox>("PortScanHost");
            var portScanResult = this.FindControl<TextBox>("PortScanResult");
            
            if (portScanHost == null || portScanResult == null) return;
            
            var host = portScanHost.Text ?? "localhost";
            var commonPorts = new[] { 21, 22, 23, 25, 53, 80, 110, 143, 443, 993, 995, 3389, 5432, 3306 };
            
            portScanResult.Text = "Scanning common ports...\n";
            
            foreach (var port in commonPorts)
            {
                try
                {
                    using var client = new TcpClient();
                    var connectTask = client.ConnectAsync(host, port);
                    var timeoutTask = Task.Delay(1000);
                    
                    if (await Task.WhenAny(connectTask, timeoutTask) == connectTask && client.Connected)
                    {
                        portScanResult.Text += $"Port {port}: OPEN\n";
                        client.Close();
                    }
                    else
                    {
                        portScanResult.Text += $"Port {port}: CLOSED/FILTERED\n";
                    }
                }
                catch
                {
                    portScanResult.Text += $"Port {port}: CLOSED/FILTERED\n";
                }
            }
            
            portScanResult.Text += "\nScan complete.";
        }

        // SQL Injection Payload Copy
        private async void CopySqlPayloadBtn_Click(object? sender, RoutedEventArgs e)
        {
            var sqlPayloadCombo = this.FindControl<ComboBox>("SqlPayloadCombo");
            if (sqlPayloadCombo?.SelectedItem != null)
            {
                var payload = sqlPayloadCombo.SelectedItem.ToString();
                var actualPayload = payload?.Split(": ", 2)[1] ?? "";
                
                try
                {
                    await TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(actualPayload)!;
                }
                catch
                {
                    // Clipboard access might fail in some environments
                }
            }
        }

        // XSS Payload Copy
        private async void CopyXssPayloadBtn_Click(object? sender, RoutedEventArgs e)
        {
            var xssPayloadCombo = this.FindControl<ComboBox>("XssPayloadCombo");
            if (xssPayloadCombo?.SelectedItem != null)
            {
                var payload = xssPayloadCombo.SelectedItem.ToString();
                var actualPayload = payload?.Split(": ", 2)[1] ?? "";
                
                try
                {
                    await TopLevel.GetTopLevel(this)?.Clipboard?.SetTextAsync(actualPayload)!;
                }
                catch
                {
                    // Clipboard access might fail in some environments
                }
            }
        }
    }
}
