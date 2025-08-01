using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ProgrammersToolKit.Views
{
    public sealed partial class ApiTesterView : UserControl
    {
        private ObservableCollection<HeaderItem> _headers = new();
        private ObservableCollection<AssertionItem> _assertions = new();
        private readonly HttpClient _httpClient = new();

        public ApiTesterView()
        {
            this.InitializeComponent();
            
            // Initialize collections
            HeadersList.ItemsSource = _headers;
            AssertionsList.ItemsSource = _assertions;
            
            // Wire up events
            SendBtn.Click += SendBtn_Click;
            AddHeaderBtn.Click += AddHeaderBtn_Click;
            AddAssertionBtn.Click += AddAssertionBtn_Click;
            SaveCallBtn.Click += SaveCallBtn_Click;
            DeleteCallBtn.Click += DeleteCallBtn_Click;
            RunAllTestsBtn.Click += RunAllTestsBtn_Click;
            
            // Add default headers
            _headers.Add(new HeaderItem { Key = "Content-Type", Value = "application/json" });
            _headers.Add(new HeaderItem { Key = "Accept", Value = "application/json" });
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var method = ((ComboBoxItem)MethodCombo.SelectedItem)?.Content?.ToString() ?? "GET";
                var url = UrlBox.Text;
                
                if (string.IsNullOrWhiteSpace(url))
                {
                    ResponseBox.Text = "Error: URL is required";
                    return;
                }

                var request = new HttpRequestMessage(new HttpMethod(method), url);
                
                // Add headers
                foreach (var header in _headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
                {
                    try
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                    catch
                    {
                        // Some headers need to be added to content headers
                        if (request.Content != null)
                            request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
                
                // Add body for POST/PUT/PATCH
                if (method is "POST" or "PUT" or "PATCH" && !string.IsNullOrWhiteSpace(JsonBodyBox.Text))
                {
                    request.Content = new StringContent(JsonBodyBox.Text, Encoding.UTF8, "application/json");
                }
                
                ResponseBox.Text = "Sending request...";
                
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                var responseText = $"Status: {(int)response.StatusCode} {response.StatusCode}\n\n";
                responseText += $"Headers:\n";
                foreach (var header in response.Headers)
                {
                    responseText += $"{header.Key}: {string.Join(", ", header.Value)}\n";
                }
                responseText += $"\nBody:\n{responseContent}";
                
                ResponseBox.Text = responseText;
                
                // Check expected status
                if (int.TryParse(ExpectedStatusBox.Text, out var expectedStatus))
                {
                    if ((int)response.StatusCode == expectedStatus)
                        TestResultBlock.Text = "✅ Status code matches expected";
                    else
                        TestResultBlock.Text = $"❌ Status code mismatch. Expected: {expectedStatus}, Got: {(int)response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ResponseBox.Text = $"Error: {ex.Message}";
                TestResultBlock.Text = "❌ Request failed";
            }
        }
        
        private void AddHeaderBtn_Click(object sender, RoutedEventArgs e)
        {
            _headers.Add(new HeaderItem { Key = "", Value = "" });
        }
        
        private void AddAssertionBtn_Click(object sender, RoutedEventArgs e)
        {
            _assertions.Add(new AssertionItem { JsonPath = "", ExpectedValue = "" });
        }
        
        private void SaveCallBtn_Click(object sender, RoutedEventArgs e)
        {
            // Simplified save functionality
            TestResultBlock.Text = "Call saved (simplified implementation)";
        }
        
        private void DeleteCallBtn_Click(object sender, RoutedEventArgs e)
        {
            // Simplified delete functionality
            TestResultBlock.Text = "Call deleted (simplified implementation)";
        }
        
        private void RunAllTestsBtn_Click(object sender, RoutedEventArgs e)
        {
            // Simplified test runner
            TestResultBlock.Text = "Running all tests (simplified implementation)";
        }
    }
    
    public class HeaderItem
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
    
    public class AssertionItem
    {
        public string JsonPath { get; set; } = "";
        public string ExpectedValue { get; set; } = "";
    }
}