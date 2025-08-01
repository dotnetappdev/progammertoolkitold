using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ProgrammersToolKit.Views
{
    public partial class ApiTesterView_Extended : UserControl
    {
        private ListBox? _savedApiCallsListBox;
        private TextBox? _apiUrlBox;
        private ComboBox? _httpMethodCombo;
        private DataGrid? _headersGrid;
        private TextBox? _requestBodyBox;
        private TextBox? _expectedStatusBox;
        private DataGrid? _assertionsGrid;
        private TextBox? _responseBox;
        private TextBox? _responseHeadersBox;
        private TextBox? _testResultsBox;
        private Button? _newApiCallBtn;
        private Button? _saveApiCallBtn;
        private Button? _deleteApiCallBtn;
        private Button? _sendApiCallBtn;

        public ApiTesterView_Extended()
        {
            AvaloniaXamlLoader.Load(this);
            _savedApiCallsListBox = this.FindControl<ListBox>("SavedApiCallsListBox");
            _apiUrlBox = this.FindControl<TextBox>("ApiUrlBox");
            _httpMethodCombo = this.FindControl<ComboBox>("HttpMethodCombo");
            _headersGrid = this.FindControl<DataGrid>("HeadersGrid");
            _requestBodyBox = this.FindControl<TextBox>("RequestBodyBox");
            _expectedStatusBox = this.FindControl<TextBox>("ExpectedStatusBox");
            _assertionsGrid = this.FindControl<DataGrid>("AssertionsGrid");
            _responseBox = this.FindControl<TextBox>("ResponseBox");
            _responseHeadersBox = this.FindControl<TextBox>("ResponseHeadersBox");
            _testResultsBox = this.FindControl<TextBox>("TestResultsBox");
            _newApiCallBtn = this.FindControl<Button>("NewApiCallBtn");
            _saveApiCallBtn = this.FindControl<Button>("SaveApiCallBtn");
            _deleteApiCallBtn = this.FindControl<Button>("DeleteApiCallBtn");
            _sendApiCallBtn = this.FindControl<Button>("SendApiCallBtn");

            if (_newApiCallBtn != null) _newApiCallBtn.Click += (s, e) => NewApiCall();
            if (_saveApiCallBtn != null) _saveApiCallBtn.Click += (s, e) => SaveApiCall();
            if (_deleteApiCallBtn != null) _deleteApiCallBtn.Click += (s, e) => DeleteApiCall();
            if (_sendApiCallBtn != null) _sendApiCallBtn.Click += async (s, e) => await SendApiCall();
        }

        // Stubs for CRUD and send logic
        private void NewApiCall()
        {
            // Clear all fields for a new API call
            _apiUrlBox!.Text = string.Empty;
            _httpMethodCombo!.SelectedIndex = 0;
            _headersGrid!.ItemsSource = new ObservableCollection<HeaderItem>();
            _requestBodyBox!.Text = string.Empty;
            _expectedStatusBox!.Text = string.Empty;
            _assertionsGrid!.ItemsSource = new ObservableCollection<AssertionItem>();
            _responseBox!.Text = string.Empty;
            _responseHeadersBox!.Text = string.Empty;
            _testResultsBox!.Text = string.Empty;
        }

        private void SaveApiCall()
        {
            // TODO: Implement save logic (to DB or file)
        }

        private void DeleteApiCall()
        {
            // TODO: Implement delete logic
        }

        private async Task SendApiCall()
        {
            if (_apiUrlBox == null || _httpMethodCombo == null) return;
            var url = _apiUrlBox.Text ?? string.Empty;
            var method = (_httpMethodCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "GET";
            var client = new HttpClient();
            var request = new HttpRequestMessage(new HttpMethod(method), url);
            // TODO: Add headers and body
            try
            {
                var response = await client.SendAsync(request);
                _responseBox!.Text = await response.Content.ReadAsStringAsync();
                _responseHeadersBox!.Text = response.Headers.ToString();
                _testResultsBox!.Text = $"Status: {(int)response.StatusCode}";
                // TODO: Run assertions
            }
            catch (Exception ex)
            {
                _responseBox!.Text = $"Error: {ex.Message}";
            }
        }

        // Simple models for headers and assertions
        public class HeaderItem { public string Key { get; set; } = ""; public string Value { get; set; } = ""; }
        public class AssertionItem { public string Path { get; set; } = ""; public string ExpectedValue { get; set; } = ""; }
    }
}
