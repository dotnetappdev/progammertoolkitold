using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Text.Json;

namespace ProgrammersToolKit.Views
{
    public sealed partial class EncryptionToolsView : UserControl
    {
        public EncryptionToolsView()
        {
            this.InitializeComponent();
            
            DecodeJwtBtn.Click += DecodeJwtBtn_Click;
            HashBtn.Click += HashBtn_Click;
            AesEncryptBtn.Click += AesEncryptBtn_Click;
            AesDecryptBtn.Click += AesDecryptBtn_Click;
        }

        private void DecodeJwtBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var text = JwtInput.Text ?? string.Empty;
                var parts = text.Split('.');
                if (parts.Length < 2) { JwtDecoded.Text = "Invalid JWT"; return; }
                string Decode(string s) => Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(s ?? string.Empty)));
                JwtDecoded.Text = $"Header: {Decode(parts[0])}\nPayload: {Decode(parts[1])}";
            }
            catch { JwtDecoded.Text = "Invalid JWT format."; }
        }
        
        private string PadBase64(string s) => s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');

        private void HashBtn_Click(object sender, RoutedEventArgs e)
        {
            var text = HashInput.Text ?? string.Empty;
            var algo = HashAlgoCombo.SelectedIndex == 1 ? "MD5" : "SHA256";
            if (algo == "SHA256")
            {
                using var sha = SHA256.Create();
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                HashResult.Text = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
            else
            {
                using var md5 = MD5.Create();
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                HashResult.Text = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private void AesEncryptBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var keyText = EncryptKey.Text ?? string.Empty;
                var key = Encoding.UTF8.GetBytes(keyText.PadRight(32).Substring(0, 32));
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = new byte[16];
                var encryptor = aes.CreateEncryptor();
                var inputBytes = Encoding.UTF8.GetBytes(EncryptInput.Text ?? string.Empty);
                var enc = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
                EncryptResult.Text = Convert.ToBase64String(enc);
            }
            catch { EncryptResult.Text = "Encryption error."; }
        }
        
        private void AesDecryptBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var keyText = EncryptKey.Text ?? string.Empty;
                var key = Encoding.UTF8.GetBytes(keyText.PadRight(32).Substring(0, 32));
                using var aes = Aes.Create();
                aes.Key = key;
                aes.IV = new byte[16];
                var decryptor = aes.CreateDecryptor();
                var encInput = EncryptInput.Text ?? string.Empty;
                var encBytes = Convert.FromBase64String(encInput);
                var dec = decryptor.TransformFinalBlock(encBytes, 0, encBytes.Length);
                EncryptResult.Text = Encoding.UTF8.GetString(dec);
            }
            catch { EncryptResult.Text = "Decryption error."; }
        }
    }
}