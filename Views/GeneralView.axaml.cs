using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ProgrammersToolKit.Database;
using Avalonia.Media;
using System;
using Avalonia;

namespace ProgrammersToolKit.Views
{
    public partial class GeneralView : UserControl
    {
        private TextBlock? _accessCodeText;
        private Button? _copyCodeBtn;
        private TextBlock? _connectionStatusText;
        private TextBlock? _securityStatusText;

        public GeneralView()
        {
            AvaloniaXamlLoader.Load(this);
            _accessCodeText = this.FindControl<TextBlock>("AccessCodeText");
            _copyCodeBtn = this.FindControl<Button>("CopyCodeBtn");
            _connectionStatusText = this.FindControl<TextBlock>("ConnectionStatusText");

            // Security badge (add to status row if present)
            if (_connectionStatusText?.Parent is StackPanel statusPanel)
            {
                _securityStatusText = new TextBlock
                {
                    Text = "Secure",
                    Foreground = Brushes.Green,
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    Margin = new Thickness(8,0,0,0)
                };
                statusPanel.Children.Add(_securityStatusText);
            }

            // Show access code
            var code = DatabaseManager.GetLatestAccessCode() ?? DatabaseManager.GenerateAndStoreAccessCode();
            if (_accessCodeText != null)
                _accessCodeText.Text = code;

            if (_copyCodeBtn != null)
                _copyCodeBtn.Click += (s, e) =>
                {
                    if (_accessCodeText != null)
                    {
                        var topLevel = TopLevel.GetTopLevel(this);
                        topLevel?.Clipboard?.SetTextAsync(_accessCodeText.Text);
                    }
                };

            // Simulate connection status (replace with real logic)
            SetConnectionStatus("Disconnected", Brushes.Red);
        }

        public void SetConnectionStatus(string status, IBrush color)
        {
            if (_connectionStatusText != null)
            {
                _connectionStatusText.Text = status;
                _connectionStatusText.Foreground = color;
            }
        }
    }
}
