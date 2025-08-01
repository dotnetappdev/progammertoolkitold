using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using screenshareav.Database;
using screenshareav.ViewModels;

namespace screenshareav.Views
{
    public partial class ApiTestView : UserControl
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, string> _headers;
        private ApiTestViewModel? _viewModel;

        public ApiTestView()
        {
            AvaloniaXamlLoader.Load(this);
            _httpClient = new HttpClient();
            _headers = new Dictionary<string, string>();
            _viewModel = new ApiTestViewModel();
            DataContext = _viewModel;
            
            LoadSavedTests();
        }

        private async void SendButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var methodComboBox = this.FindControl<ComboBox>("MethodComboBox");
                var urlTextBox = this.FindControl<TextBox>("UrlTextBox");
                var requestBodyTextBox = this.FindControl<TextBox>("RequestBodyTextBox");
                var responseTextBox = this.FindControl<TextBox>("ResponseTextBox");
                var statusLabel = this.FindControl<TextBlock>("StatusLabel");

                if (urlTextBox?.Text == null || string.IsNullOrWhiteSpace(urlTextBox.Text))
                {
                    statusLabel!.Text = "Status: Please enter a URL";
                    return;
                }

                statusLabel!.Text = "Status: Sending request...";

                var method = methodComboBox?.SelectedItem?.ToString() ?? "GET";
                var url = urlTextBox.Text;
                var requestBody = requestBodyTextBox?.Text ?? "";

                var request = new HttpRequestMessage(new HttpMethod(method), url);

                // Add headers
                foreach (var header in _headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                // Add request body for POST, PUT, PATCH
                if (!string.IsNullOrWhiteSpace(requestBody) && 
                    (method == "POST" || method == "PUT" || method == "PATCH"))
                {
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                }

                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                // Format JSON response if it's JSON
                try
                {
                    var jsonDoc = JsonDocument.Parse(responseContent);
                    responseContent = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    });
                }
                catch
                {
                    // Not JSON, keep as is
                }

                responseTextBox!.Text = responseContent;
                statusLabel.Text = $"Status: {(int)response.StatusCode} {response.StatusCode}";
            }
            catch (Exception ex)
            {
                var responseTextBox = this.FindControl<TextBox>("ResponseTextBox");
                var statusLabel = this.FindControl<TextBlock>("StatusLabel");
                
                responseTextBox!.Text = $"Error: {ex.Message}";
                statusLabel!.Text = "Status: Error occurred";
            }
        }

        private void AddHeaderButton_Click(object? sender, RoutedEventArgs e)
        {
            var headerKeyTextBox = this.FindControl<TextBox>("HeaderKeyTextBox");
            var headerValueTextBox = this.FindControl<TextBox>("HeaderValueTextBox");
            var headersList = this.FindControl<ListBox>("HeadersList");

            var key = headerKeyTextBox?.Text?.Trim();
            var value = headerValueTextBox?.Text?.Trim();

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                _headers[key] = value;
                RefreshHeadersList();
                
                headerKeyTextBox!.Text = "";
                headerValueTextBox!.Text = "";
            }
        }

        private void RefreshHeadersList()
        {
            var headersList = this.FindControl<ListBox>("HeadersList");
            if (headersList != null)
            {
                var items = new List<string>();
                foreach (var header in _headers)
                {
                    items.Add($"{header.Key}: {header.Value}");
                }
                headersList.ItemsSource = items;
            }
        }

        private async void RunTestButton_Click(object? sender, RoutedEventArgs e)
        {
            var responseTextBox = this.FindControl<TextBox>("ResponseTextBox");
            var expectedResponseTextBox = this.FindControl<TextBox>("ExpectedResponseTextBox");
            var expectedStatusTextBox = this.FindControl<TextBox>("ExpectedStatusTextBox");
            var testResultsTextBox = this.FindControl<TextBox>("TestResultsTextBox");
            var statusLabel = this.FindControl<TextBlock>("StatusLabel");

            var results = new List<string>();

            // Test status code
            if (int.TryParse(expectedStatusTextBox?.Text, out int expectedStatus))
            {
                var statusText = statusLabel?.Text ?? "";
                if (statusText.Contains(expectedStatus.ToString()))
                {
                    results.Add($"✅ Status Code: Expected {expectedStatus}, Got {expectedStatus}");
                }
                else
                {
                    results.Add($"❌ Status Code: Expected {expectedStatus}, Got different status");
                }
            }

            // Test JSON response
            var actualResponse = responseTextBox?.Text ?? "";
            var expectedResponse = expectedResponseTextBox?.Text ?? "";

            if (!string.IsNullOrWhiteSpace(expectedResponse))
            {
                try
                {
                    var actualJson = JsonDocument.Parse(actualResponse);
                    var expectedJson = JsonDocument.Parse(expectedResponse);

                    var actualNormalized = JsonSerializer.Serialize(actualJson);
                    var expectedNormalized = JsonSerializer.Serialize(expectedJson);

                    if (actualNormalized == expectedNormalized)
                    {
                        results.Add("✅ JSON Response: Matches expected response");
                    }
                    else
                    {
                        results.Add("❌ JSON Response: Does not match expected response");
                        results.Add($"Expected: {expectedNormalized}");
                        results.Add($"Actual: {actualNormalized}");
                    }
                }
                catch (Exception ex)
                {
                    results.Add($"❌ JSON Parsing Error: {ex.Message}");
                }
            }

            testResultsTextBox!.Text = string.Join("\n", results);
        }

        private void SaveTestButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var methodComboBox = this.FindControl<ComboBox>("MethodComboBox");
                var urlTextBox = this.FindControl<TextBox>("UrlTextBox");
                var requestBodyTextBox = this.FindControl<TextBox>("RequestBodyTextBox");
                var expectedResponseTextBox = this.FindControl<TextBox>("ExpectedResponseTextBox");
                var expectedStatusTextBox = this.FindControl<TextBox>("ExpectedStatusTextBox");

                var testName = $"{methodComboBox?.SelectedItem} {urlTextBox?.Text} - {DateTime.Now:yyyy-MM-dd HH:mm}";

                var testData = new
                {
                    Name = testName,
                    Method = methodComboBox?.SelectedItem?.ToString(),
                    Url = urlTextBox?.Text,
                    Headers = _headers,
                    RequestBody = requestBodyTextBox?.Text,
                    ExpectedResponse = expectedResponseTextBox?.Text,
                    ExpectedStatus = expectedStatusTextBox?.Text,
                    CreatedAt = DateTime.Now
                };

                var json = JsonSerializer.Serialize(testData, new JsonSerializerOptions { WriteIndented = true });
                DatabaseManager.SaveApiTest(testName, json);
                LoadSavedTests();
            }
            catch (Exception ex)
            {
                // Handle error
                var testResultsTextBox = this.FindControl<TextBox>("TestResultsTextBox");
                testResultsTextBox!.Text = $"Error saving test: {ex.Message}";
            }
        }

        private void LoadTestButton_Click(object? sender, RoutedEventArgs e)
        {
            var savedTestsComboBox = this.FindControl<ComboBox>("SavedTestsComboBox");
            var selectedTest = savedTestsComboBox?.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedTest))
            {
                try
                {
                    var testJson = DatabaseManager.GetApiTest(selectedTest);
                    if (!string.IsNullOrEmpty(testJson))
                    {
                        var testData = JsonSerializer.Deserialize<JsonElement>(testJson);
                        
                        // Load test data into UI
                        var methodComboBox = this.FindControl<ComboBox>("MethodComboBox");
                        var urlTextBox = this.FindControl<TextBox>("UrlTextBox");
                        var requestBodyTextBox = this.FindControl<TextBox>("RequestBodyTextBox");
                        var expectedResponseTextBox = this.FindControl<TextBox>("ExpectedResponseTextBox");
                        var expectedStatusTextBox = this.FindControl<TextBox>("ExpectedStatusTextBox");

                        if (testData.TryGetProperty("Method", out var method))
                        {
                            var methodStr = method.GetString();
                            for (int i = 0; i < methodComboBox!.ItemCount; i++)
                            {
                                if (methodComboBox.Items[i]?.ToString() == methodStr)
                                {
                                    methodComboBox.SelectedIndex = i;
                                    break;
                                }
                            }
                        }

                        if (testData.TryGetProperty("Url", out var url))
                            urlTextBox!.Text = url.GetString();

                        if (testData.TryGetProperty("RequestBody", out var requestBody))
                            requestBodyTextBox!.Text = requestBody.GetString();

                        if (testData.TryGetProperty("ExpectedResponse", out var expectedResponse))
                            expectedResponseTextBox!.Text = expectedResponse.GetString();

                        if (testData.TryGetProperty("ExpectedStatus", out var expectedStatus))
                            expectedStatusTextBox!.Text = expectedStatus.GetString();

                        // Load headers
                        _headers.Clear();
                        if (testData.TryGetProperty("Headers", out var headers))
                        {
                            foreach (var header in headers.EnumerateObject())
                            {
                                _headers[header.Name] = header.Value.GetString() ?? "";
                            }
                        }
                        RefreshHeadersList();
                    }
                }
                catch (Exception ex)
                {
                    var testResultsTextBox = this.FindControl<TextBox>("TestResultsTextBox");
                    testResultsTextBox!.Text = $"Error loading test: {ex.Message}";
                }
            }
        }

        private void DeleteTestButton_Click(object? sender, RoutedEventArgs e)
        {
            var savedTestsComboBox = this.FindControl<ComboBox>("SavedTestsComboBox");
            var selectedTest = savedTestsComboBox?.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedTest))
            {
                DatabaseManager.DeleteApiTest(selectedTest);
                LoadSavedTests();
            }
        }

        private void LoadSavedTests()
        {
            var savedTestsComboBox = this.FindControl<ComboBox>("SavedTestsComboBox");
            if (savedTestsComboBox != null)
            {
                var tests = DatabaseManager.GetApiTestNames();
                savedTestsComboBox.ItemsSource = tests;
            }
        }

        private async void LoadHttpFileButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Select HTTP File",
                    Filters = { new FileDialogFilter { Name = "HTTP Files", Extensions = { "http", "rest" } } }
                };

                var result = await openFileDialog.ShowAsync(TopLevel.GetTopLevel(this) as Window);
                if (result != null && result.Length > 0)
                {
                    var content = await File.ReadAllTextAsync(result[0]);
                    var httpFileContentTextBox = this.FindControl<TextBox>("HttpFileContentTextBox");
                    httpFileContentTextBox!.Text = content;

                    // Parse and load into UI
                    ParseHttpFile(content);
                }
            }
            catch (Exception ex)
            {
                var testResultsTextBox = this.FindControl<TextBox>("TestResultsTextBox");
                testResultsTextBox!.Text = $"Error loading HTTP file: {ex.Message}";
            }
        }

        private async void SaveHttpFileButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "Save HTTP File",
                    DefaultExtension = "http",
                    Filters = { new FileDialogFilter { Name = "HTTP Files", Extensions = { "http", "rest" } } }
                };

                var result = await saveFileDialog.ShowAsync(TopLevel.GetTopLevel(this) as Window);
                if (!string.IsNullOrEmpty(result))
                {
                    var content = GenerateHttpFileContent();
                    await File.WriteAllTextAsync(result, content);
                    
                    var httpFileContentTextBox = this.FindControl<TextBox>("HttpFileContentTextBox");
                    httpFileContentTextBox!.Text = content;
                }
            }
            catch (Exception ex)
            {
                var testResultsTextBox = this.FindControl<TextBox>("TestResultsTextBox");
                testResultsTextBox!.Text = $"Error saving HTTP file: {ex.Message}";
            }
        }

        private void ParseHttpFile(string content)
        {
            var lines = content.Split('\n');
            var methodComboBox = this.FindControl<ComboBox>("MethodComboBox");
            var urlTextBox = this.FindControl<TextBox>("UrlTextBox");
            var requestBodyTextBox = this.FindControl<TextBox>("RequestBodyTextBox");

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Parse request line (METHOD URL)
                if (trimmedLine.StartsWith("GET ") || trimmedLine.StartsWith("POST ") || 
                    trimmedLine.StartsWith("PUT ") || trimmedLine.StartsWith("DELETE ") ||
                    trimmedLine.StartsWith("PATCH ") || trimmedLine.StartsWith("HEAD ") ||
                    trimmedLine.StartsWith("OPTIONS "))
                {
                    var parts = trimmedLine.Split(' ', 2);
                    if (parts.Length == 2)
                    {
                        // Set method
                        for (int i = 0; i < methodComboBox!.ItemCount; i++)
                        {
                            if (methodComboBox.Items[i]?.ToString() == parts[0])
                            {
                                methodComboBox.SelectedIndex = i;
                                break;
                            }
                        }
                        
                        // Set URL
                        urlTextBox!.Text = parts[1];
                    }
                }
                
                // Parse headers
                else if (trimmedLine.Contains(':') && !trimmedLine.StartsWith('{'))
                {
                    var headerParts = trimmedLine.Split(':', 2);
                    if (headerParts.Length == 2)
                    {
                        _headers[headerParts[0].Trim()] = headerParts[1].Trim();
                    }
                }
            }
            
            RefreshHeadersList();
            
            // Extract JSON body (everything after headers)
            var bodyStart = content.IndexOf('{');
            if (bodyStart != -1)
            {
                var jsonBody = content.Substring(bodyStart);
                requestBodyTextBox!.Text = jsonBody.Trim();
            }
        }

        private string GenerateHttpFileContent()
        {
            var methodComboBox = this.FindControl<ComboBox>("MethodComboBox");
            var urlTextBox = this.FindControl<TextBox>("UrlTextBox");
            var requestBodyTextBox = this.FindControl<TextBox>("RequestBodyTextBox");

            var sb = new StringBuilder();
            
            // Request line
            sb.AppendLine($"{methodComboBox?.SelectedItem} {urlTextBox?.Text}");
            
            // Headers
            foreach (var header in _headers)
            {
                sb.AppendLine($"{header.Key}: {header.Value}");
            }
            
            // Empty line before body
            sb.AppendLine();
            
            // Request body
            if (!string.IsNullOrWhiteSpace(requestBodyTextBox?.Text))
            {
                sb.AppendLine(requestBodyTextBox.Text);
            }
            
            return sb.ToString();
        }
    }
}