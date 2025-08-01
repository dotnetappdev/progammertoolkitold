using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using screenshareav.Database;

namespace screenshareav.Views
{
    public partial class APITestingView : UserControl
    {
        private ComboBox? _httpMethodCombo;
        private TextBox? _urlTextBox;
        private Button? _sendRequestBtn;
        private TextBox? _headerKeyInput;
        private TextBox? _headerValueInput;
        private Button? _addHeaderBtn;
        private ListBox? _headersList;
        private Button? _clearHeadersBtn;
        private TextBox? _requestBodyTextBox;
        private Button? _formatJsonBtn;
        private Button? _clearBodyBtn;
        private TextBox? _testCaseNameInput;
        private Button? _saveTestBtn;
        private ComboBox? _loadTestCombo;
        private Button? _loadTestBtn;
        private Button? _deleteTestBtn;
        private Button? _importHttpFileBtn;
        private Button? _exportHttpFileBtn;
        private TextBlock? _statusCodeText;
        private TextBlock? _responseTimeText;
        private TextBlock? _responseSizeText;
        private Button? _formatResponseBtn;
        private Button? _copyResponseBtn;
        private TextBox? _responseBodyTextBox;
        private TextBox? _responseHeadersTextBox;
        private TextBox? _expectedResponseTextBox;
        private TextBox? _expectedStatusCodeInput;
        private Button? _validateResponseBtn;
        private TextBlock? _validationResultText;

        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _requestHeaders;

        public APITestingView()
        {
            AvaloniaXamlLoader.Load(this);
            _httpClient = new HttpClient();
            _requestHeaders = new Dictionary<string, string>();
            
            InitializeControls();
            AttachEventHandlers();
            LoadSavedTests();
        }

        private void InitializeControls()
        {
            _httpMethodCombo = this.FindControl<ComboBox>("HttpMethodCombo");
            _urlTextBox = this.FindControl<TextBox>("UrlTextBox");
            _sendRequestBtn = this.FindControl<Button>("SendRequestBtn");
            _headerKeyInput = this.FindControl<TextBox>("HeaderKeyInput");
            _headerValueInput = this.FindControl<TextBox>("HeaderValueInput");
            _addHeaderBtn = this.FindControl<Button>("AddHeaderBtn");
            _headersList = this.FindControl<ListBox>("HeadersList");
            _clearHeadersBtn = this.FindControl<Button>("ClearHeadersBtn");
            _requestBodyTextBox = this.FindControl<TextBox>("RequestBodyTextBox");
            _formatJsonBtn = this.FindControl<Button>("FormatJsonBtn");
            _clearBodyBtn = this.FindControl<Button>("ClearBodyBtn");
            _testCaseNameInput = this.FindControl<TextBox>("TestCaseNameInput");
            _saveTestBtn = this.FindControl<Button>("SaveTestBtn");
            _loadTestCombo = this.FindControl<ComboBox>("LoadTestCombo");
            _loadTestBtn = this.FindControl<Button>("LoadTestBtn");
            _deleteTestBtn = this.FindControl<Button>("DeleteTestBtn");
            _importHttpFileBtn = this.FindControl<Button>("ImportHttpFileBtn");
            _exportHttpFileBtn = this.FindControl<Button>("ExportHttpFileBtn");
            _statusCodeText = this.FindControl<TextBlock>("StatusCodeText");
            _responseTimeText = this.FindControl<TextBlock>("ResponseTimeText");
            _responseSizeText = this.FindControl<TextBlock>("ResponseSizeText");
            _formatResponseBtn = this.FindControl<Button>("FormatResponseBtn");
            _copyResponseBtn = this.FindControl<Button>("CopyResponseBtn");
            _responseBodyTextBox = this.FindControl<TextBox>("ResponseBodyTextBox");
            _responseHeadersTextBox = this.FindControl<TextBox>("ResponseHeadersTextBox");
            _expectedResponseTextBox = this.FindControl<TextBox>("ExpectedResponseTextBox");
            _expectedStatusCodeInput = this.FindControl<TextBox>("ExpectedStatusCodeInput");
            _validateResponseBtn = this.FindControl<Button>("ValidateResponseBtn");
            _validationResultText = this.FindControl<TextBlock>("ValidationResultText");
        }

        private void AttachEventHandlers()
        {
            if (_sendRequestBtn != null) _sendRequestBtn.Click += SendRequestBtn_Click;
            if (_addHeaderBtn != null) _addHeaderBtn.Click += AddHeaderBtn_Click;
            if (_clearHeadersBtn != null) _clearHeadersBtn.Click += ClearHeadersBtn_Click;
            if (_formatJsonBtn != null) _formatJsonBtn.Click += FormatJsonBtn_Click;
            if (_clearBodyBtn != null) _clearBodyBtn.Click += ClearBodyBtn_Click;
            if (_saveTestBtn != null) _saveTestBtn.Click += SaveTestBtn_Click;
            if (_loadTestBtn != null) _loadTestBtn.Click += LoadTestBtn_Click;
            if (_deleteTestBtn != null) _deleteTestBtn.Click += DeleteTestBtn_Click;
            if (_importHttpFileBtn != null) _importHttpFileBtn.Click += ImportHttpFileBtn_Click;
            if (_exportHttpFileBtn != null) _exportHttpFileBtn.Click += ExportHttpFileBtn_Click;
            if (_formatResponseBtn != null) _formatResponseBtn.Click += FormatResponseBtn_Click;
            if (_copyResponseBtn != null) _copyResponseBtn.Click += CopyResponseBtn_Click;
            if (_validateResponseBtn != null) _validateResponseBtn.Click += ValidateResponseBtn_Click;
        }

        private async void SendRequestBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_httpMethodCombo == null || _urlTextBox == null) return;

            try
            {
                var method = GetSelectedHttpMethod();
                var url = _urlTextBox.Text?.Trim();
                
                if (string.IsNullOrWhiteSpace(url))
                {
                    UpdateStatus("Error: URL is required", "-", "-");
                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                var request = new HttpRequestMessage(method, url);

                // Add headers
                foreach (var header in _requestHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                // Add request body for POST/PUT/PATCH
                if ((method == HttpMethod.Post || method == HttpMethod.Put || method.Method == "PATCH") 
                    && !string.IsNullOrWhiteSpace(_requestBodyTextBox?.Text))
                {
                    request.Content = new StringContent(_requestBodyTextBox.Text, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);
                stopwatch.Stop();

                var responseBody = await response.Content.ReadAsStringAsync();
                var responseHeaders = GetResponseHeadersString(response);

                UpdateStatus($"Status: {(int)response.StatusCode} {response.StatusCode}", 
                            $"Time: {stopwatch.ElapsedMilliseconds}ms", 
                            $"Size: {Encoding.UTF8.GetByteCount(responseBody)} bytes");

                if (_responseBodyTextBox != null) _responseBodyTextBox.Text = responseBody;
                if (_responseHeadersTextBox != null) _responseHeadersTextBox.Text = responseHeaders;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}", "-", "-");
                if (_responseBodyTextBox != null) _responseBodyTextBox.Text = $"Request failed: {ex.Message}";
            }
        }

        private HttpMethod GetSelectedHttpMethod()
        {
            return _httpMethodCombo?.SelectedIndex switch
            {
                0 => HttpMethod.Get,
                1 => HttpMethod.Post,
                2 => HttpMethod.Put,
                3 => HttpMethod.Delete,
                4 => new HttpMethod("PATCH"),
                5 => HttpMethod.Head,
                6 => HttpMethod.Options,
                _ => HttpMethod.Get
            };
        }

        private string GetResponseHeadersString(HttpResponseMessage response)
        {
            var sb = new StringBuilder();
            foreach (var header in response.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            foreach (var header in response.Content.Headers)
            {
                sb.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            return sb.ToString();
        }

        private void UpdateStatus(string status, string time, string size)
        {
            if (_statusCodeText != null) _statusCodeText.Text = status;
            if (_responseTimeText != null) _responseTimeText.Text = time;
            if (_responseSizeText != null) _responseSizeText.Text = size;
        }

        private void AddHeaderBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_headerKeyInput == null || _headerValueInput == null || _headersList == null) return;

            var key = _headerKeyInput.Text?.Trim();
            var value = _headerValueInput.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
            {
                _requestHeaders[key] = value;
                RefreshHeadersList();
                _headerKeyInput.Text = "";
                _headerValueInput.Text = "";
            }
        }

        private void ClearHeadersBtn_Click(object? sender, RoutedEventArgs e)
        {
            _requestHeaders.Clear();
            RefreshHeadersList();
        }

        private void RefreshHeadersList()
        {
            if (_headersList == null) return;

            var items = new List<string>();
            foreach (var header in _requestHeaders)
            {
                items.Add($"{header.Key}: {header.Value}");
            }
            _headersList.ItemsSource = items;
        }

        private void FormatJsonBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_requestBodyTextBox == null) return;

            try
            {
                var json = _requestBodyTextBox.Text;
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var parsed = JsonDocument.Parse(json);
                    _requestBodyTextBox.Text = JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });
                }
            }
            catch
            {
                // Could add error display here
            }
        }

        private void ClearBodyBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_requestBodyTextBox != null) _requestBodyTextBox.Text = "";
        }

        private void FormatResponseBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_responseBodyTextBox == null) return;

            try
            {
                var json = _responseBodyTextBox.Text;
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var parsed = JsonDocument.Parse(json);
                    _responseBodyTextBox.Text = JsonSerializer.Serialize(parsed, new JsonSerializerOptions { WriteIndented = true });
                }
            }
            catch
            {
                // Response might not be JSON
            }
        }

        private async void CopyResponseBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_responseBodyTextBox?.Text != null)
            {
                try
                {
                    var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                    if (clipboard != null)
                        await clipboard.SetTextAsync(_responseBodyTextBox.Text);
                }
                catch
                {
                    // Handle clipboard error
                }
            }
        }

        private void ValidateResponseBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_validationResultText == null) return;

            try
            {
                var results = new List<string>();

                // Validate status code
                if (!string.IsNullOrWhiteSpace(_expectedStatusCodeInput?.Text))
                {
                    if (int.TryParse(_expectedStatusCodeInput.Text, out var expectedCode))
                    {
                        var actualStatusText = _statusCodeText?.Text ?? "";
                        if (actualStatusText.Contains($"Status: {expectedCode}"))
                        {
                            results.Add("✓ Status code matches expected");
                        }
                        else
                        {
                            results.Add("✗ Status code does not match expected");
                        }
                    }
                }

                // Validate JSON response
                if (!string.IsNullOrWhiteSpace(_expectedResponseTextBox?.Text) && 
                    !string.IsNullOrWhiteSpace(_responseBodyTextBox?.Text))
                {
                    try
                    {
                        var expected = JsonDocument.Parse(_expectedResponseTextBox.Text);
                        var actual = JsonDocument.Parse(_responseBodyTextBox.Text);
                        
                        var expectedJson = JsonSerializer.Serialize(expected, new JsonSerializerOptions { WriteIndented = false });
                        var actualJson = JsonSerializer.Serialize(actual, new JsonSerializerOptions { WriteIndented = false });
                        
                        if (expectedJson == actualJson)
                        {
                            results.Add("✓ JSON response matches expected");
                        }
                        else
                        {
                            results.Add("✗ JSON response does not match expected");
                        }
                    }
                    catch
                    {
                        results.Add("✗ Invalid JSON in expected or actual response");
                    }
                }

                _validationResultText.Text = results.Count > 0 ? string.Join("\n", results) : "No validation criteria specified";
            }
            catch (Exception ex)
            {
                _validationResultText.Text = $"Validation error: {ex.Message}";
            }
        }

        // Test case management methods
        private void SaveTestBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_testCaseNameInput == null) return;

            var testName = _testCaseNameInput.Text?.Trim();
            if (string.IsNullOrWhiteSpace(testName)) return;

            var testData = new
            {
                Method = _httpMethodCombo?.SelectedIndex ?? 0,
                Url = _urlTextBox?.Text ?? "",
                Headers = _requestHeaders,
                Body = _requestBodyTextBox?.Text ?? "",
                ExpectedResponse = _expectedResponseTextBox?.Text ?? "",
                ExpectedStatusCode = _expectedStatusCodeInput?.Text ?? ""
            };

            try
            {
                var jsonData = JsonSerializer.Serialize(testData, new JsonSerializerOptions { WriteIndented = true });
                DatabaseManager.SaveApiTest(testName, jsonData);
                LoadSavedTests();
                _testCaseNameInput.Text = "";
            }
            catch
            {
                // Handle error
            }
        }

        private void LoadTestBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_loadTestCombo?.SelectedItem == null) return;

            var testName = _loadTestCombo.SelectedItem.ToString();
            if (string.IsNullOrWhiteSpace(testName)) return;

            try
            {
                var jsonData = DatabaseManager.GetApiTest(testName);
                if (!string.IsNullOrWhiteSpace(jsonData))
                {
                    var testData = JsonSerializer.Deserialize<dynamic>(jsonData);
                    // Load the test data back into the UI
                    // Implementation would involve parsing the JSON and setting UI controls
                }
            }
            catch
            {
                // Handle error
            }
        }

        private void DeleteTestBtn_Click(object? sender, RoutedEventArgs e)
        {
            if (_loadTestCombo?.SelectedItem == null) return;

            var testName = _loadTestCombo.SelectedItem.ToString();
            if (!string.IsNullOrWhiteSpace(testName))
            {
                DatabaseManager.DeleteApiTest(testName);
                LoadSavedTests();
            }
        }

        private void LoadSavedTests()
        {
            if (_loadTestCombo == null) return;

            try
            {
                var testNames = DatabaseManager.GetApiTestNames();
                _loadTestCombo.ItemsSource = testNames;
            }
            catch
            {
                // Handle error
            }
        }

        // HTTP File operations
        private void ImportHttpFileBtn_Click(object? sender, RoutedEventArgs e)
        {
            // Implementation for importing .http files
        }

        private void ExportHttpFileBtn_Click(object? sender, RoutedEventArgs e)
        {
            // Implementation for exporting .http files
        }
    }
}