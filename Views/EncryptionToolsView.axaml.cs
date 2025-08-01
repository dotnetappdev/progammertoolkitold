using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Text.Json;

namespace ProgrammersToolKit.Views
{
    public partial class EncryptionToolsView : UserControl
    {
        private TextBox? _jwtInput;
        private Button? _decodeJwtBtn;
        private TextBlock? _jwtDecoded;
        private TextBox? _hashInput;
        private ComboBox? _hashAlgoCombo;
        private Button? _hashBtn;
        private TextBlock? _hashResult;
        private TextBox? _encryptInput;
        private TextBox? _encryptKey;
        private Button? _aesEncryptBtn;
        private Button? _aesDecryptBtn;
        private TextBlock? _encryptResult;

        public EncryptionToolsView()
        {
            AvaloniaXamlLoader.Load(this);
            _jwtInput = this.FindControl<TextBox>("JwtInput");
            _decodeJwtBtn = this.FindControl<Button>("DecodeJwtBtn");
            _jwtDecoded = this.FindControl<TextBlock>("JwtDecoded");
            _hashInput = this.FindControl<TextBox>("HashInput");
            _hashAlgoCombo = this.FindControl<ComboBox>("HashAlgoCombo");
            _hashBtn = this.FindControl<Button>("HashBtn");
            _hashResult = this.FindControl<TextBlock>("HashResult");
            _encryptInput = this.FindControl<TextBox>("EncryptInput");
            _encryptKey = this.FindControl<TextBox>("EncryptKey");
            _aesEncryptBtn = this.FindControl<Button>("AesEncryptBtn");
            _aesDecryptBtn = this.FindControl<Button>("AesDecryptBtn");
            _encryptResult = this.FindControl<TextBlock>("EncryptResult");

            if (_decodeJwtBtn != null)
                _decodeJwtBtn.Click += DecodeJwtBtn_Click;
            if (_hashBtn != null)
                _hashBtn.Click += HashBtn_Click;
            if (_aesEncryptBtn != null)
                _aesEncryptBtn.Click += AesEncryptBtn_Click;
            if (_aesDecryptBtn != null)
                _aesDecryptBtn.Click += AesDecryptBtn_Click;
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
                _jwtDecoded.Text = $"Header: {Decode(parts[0])}\nPayload: {Decode(parts[1])}";
            }
            catch { _jwtDecoded.Text = "Invalid JWT format."; }
        }
        private string PadBase64(string s) => s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');

        private void HashBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_hashInput == null || _hashResult == null || _hashAlgoCombo == null) return;
            var text = _hashInput.Text ?? string.Empty;
            var algo = _hashAlgoCombo.SelectedIndex == 1 ? "MD5" : "SHA256";
            if (algo == "SHA256")
            {
                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                _hashResult.Text = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
            else
            {
                using var md5 = MD5.Create();
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                _hashResult.Text = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
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
                aes.IV = new byte[16];
                var encryptor = aes.CreateEncryptor();
                var inputBytes = Encoding.UTF8.GetBytes(_encryptInput.Text ?? string.Empty);
                var enc = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                _encryptResult.Text = Convert.ToBase64String(enc);
            }
            catch { _encryptResult.Text = "Encryption error."; }
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
                aes.IV = new byte[16];
                var decryptor = aes.CreateDecryptor();
                var encInput = _encryptInput.Text ?? string.Empty;
                var encBytes = Convert.FromBase64String(encInput);
                var dec = decryptor.TransformFinalBlock(encBytes, 0, encBytes.Length);
                _encryptResult.Text = Encoding.UTF8.GetString(dec);
            }
            catch { _encryptResult.Text = "Decryption error."; }
        }
    }
}
