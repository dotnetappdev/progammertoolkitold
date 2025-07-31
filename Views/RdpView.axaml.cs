using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using screenshareav.Database;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia;
using System.Linq;

namespace screenshareav.Views
{
    public partial class RdpView : UserControl
    {
        private ListBox? _connectionsList;
        private TextBox? _ipTextBox;
        private TextBox? _userTextBox;
        private TextBox? _passwordTextBox;
        private Button? _connectBtn;
        private Button? _deleteBtn;
        private TextBlock? _statusText;
        private const string EncKey = "ScreenShareAV2025"; // For demo only

        public RdpView()
        {
            AvaloniaXamlLoader.Load(this);
            _connectionsList = this.FindControl<ListBox>("ConnectionsList");
            _ipTextBox = this.FindControl<TextBox>("IpTextBox");
            _userTextBox = this.FindControl<TextBox>("UserTextBox");
            _passwordTextBox = this.FindControl<TextBox>("PasswordTextBox");
            _connectBtn = this.FindControl<Button>("ConnectBtn");
            _deleteBtn = this.FindControl<Button>("DeleteBtn");

            // Add status text
            _statusText = new TextBlock { Foreground = Brushes.Gray, Margin = new Thickness(0,8,0,0) };
            var parent = this.Content as Panel;
            parent?.Children.Add(_statusText);

            if (_connectBtn != null)
                _connectBtn.Click += ConnectBtn_Click;
            if (_deleteBtn != null)
                _deleteBtn.Click += DeleteBtn_Click;
            if (_connectionsList != null)
                _connectionsList.DoubleTapped += ConnectionsList_DoubleTapped;

            LoadConnections();
        
        }

        private void LoadConnections()
        {
            var connections = DatabaseManager.GetRdpConnections(EncKey);
            if (_connectionsList != null)
                _connectionsList.ItemsSource = connections;
        }

        private void ConnectBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_ipTextBox == null || _userTextBox == null || _passwordTextBox == null) return;
            var ip = _ipTextBox.Text;
            var user = _userTextBox.Text;
            var pass = _passwordTextBox.Text;
            if (!string.IsNullOrWhiteSpace(ip) && !string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
            {
                // If a connection with this IP exists, update it; else, add new
                var connections = DatabaseManager.GetRdpConnections(EncKey);
                if (connections.Any(c => c.Ip == ip))
                {
                    DatabaseManager.DeleteRdpConnection(ip);
                }
                DatabaseManager.SaveRdpConnection($"{user}@{ip}", ip, user, pass, EncKey);
                LoadConnections();
                SetStatus($"Saved and ready to connect to {ip} as {user}.", Brushes.Blue);
                // TODO: Integrate .NET RDP library here
                // On success: SetStatus("Connected!", Brushes.Green);
                // On error: SetStatus("Connection failed.", Brushes.Red);
            }
        }

        private void ConnectionsList_DoubleTapped(object? sender, RoutedEventArgs e)
        {
            if (_connectionsList?.SelectedItem is RdpConnection conn)
            {
                _ipTextBox!.Text = conn.Ip;
                _userTextBox!.Text = conn.Username;
                _passwordTextBox!.Text = conn.Password;
                SetStatus($"Ready to connect to {conn.Ip} as {conn.Username}.", Brushes.Gray);
            }
        }

        private void DeleteBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_connectionsList?.SelectedItem is RdpConnection conn)
            {
                DatabaseManager.DeleteRdpConnection(conn.Ip);
                LoadConnections();
                SetStatus($"Deleted connection for {conn.Ip}.", Brushes.Gray);
            }
        }

        private void SetStatus(string msg, IBrush color)
        {
            if (_statusText != null)
            {
                _statusText.Text = msg;
                _statusText.Foreground = color;
            }
        }
    }
}
