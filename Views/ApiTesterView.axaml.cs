
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using Avalonia.VisualTree;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Linq;
using ProgrammersToolKit.Database;
using System.Text.Json;
using System.Xml.Linq;

using Microsoft.EntityFrameworkCore;

namespace ProgrammersToolKit.Views
{
    public partial class ApiTesterView : UserControl
    {
        private ComboBox? _methodCombo;
        private TextBox? _urlBox;
        private Button? _sendBtn;
        private Button? _saveCallBtn;
        private Button? _deleteCallBtn;
        private Avalonia.Controls.DataGrid? _headersGrid;
        private Button? _addHeaderBtn;
        private TextBox? _jsonBodyBox;
        private TextBox? _responseBox;
        private ListBox? _savedCallsList;
        private TextBox? _expectedStatusBox;
        private TextBox? _expectedJsonBox;
        private TextBox? _expectedXmlBox;
        private TextBlock? _testResultBlock;

        private ObservableCollection<HeaderItem> _headers = new();
        // CTF tools are now in CtfToolsView
        private static readonly List<(string Name, string Value)> _commonHeaders = new()
        {
            ("Content-Type", "application/json"),
            ("Accept", "application/json"),
            ("User-Agent", "ProgrammersToolKit/1.0"),
            ("Authorization", "Bearer <token>"),
            ("Cache-Control", "no-cache"),
        };
        private string? _headerValidationResult;
        private ObservableCollection<ApiCall> _savedCalls = new();
        private TextBox? _testNameBox;
        private Button? _exportTestsBtn;
        private Button? _importTestsBtn;
        private ObservableCollection<ValueAssertion> _valueAssertions = new();
        private ApiTesterDbContext _db = new ApiTesterDbContext();

        public ApiTesterView()
        {
            AvaloniaXamlLoader.Load(this);
            _testNameBox = this.FindControl<TextBox>("TestNameBox");
            _exportTestsBtn = this.FindControl<Button>("ExportTestsBtn");
            _importTestsBtn = this.FindControl<Button>("ImportTestsBtn");
        if (_exportTestsBtn != null)
            _exportTestsBtn.Click += async (s, e) => await ExportTestsAsync();
        if (_importTestsBtn != null)
            _importTestsBtn.Click += async (s, e) => await ImportTestsAsync();
            _methodCombo = this.FindControl<ComboBox>("MethodCombo");
            _urlBox = this.FindControl<TextBox>("UrlBox");
            _sendBtn = this.FindControl<Button>("SendBtn");
            _saveCallBtn = this.FindControl<Button>("SaveCallBtn");
            _deleteCallBtn = this.FindControl<Button>("DeleteCallBtn");
            _headersGrid = this.FindControl<Avalonia.Controls.DataGrid>("HeadersGrid");
            _jsonBodyBox = this.FindControl<TextBox>("JsonBodyBox");
            _responseBox = this.FindControl<TextBox>("ResponseBox");
            _savedCallsList = this.FindControl<ListBox>("SavedCallsList");
            _expectedStatusBox = this.FindControl<TextBox>("ExpectedStatusBox");
            _expectedJsonBox = this.FindControl<TextBox>("ExpectedJsonBox");
            _expectedXmlBox = this.FindControl<TextBox>("ExpectedXmlBox");
            _testResultBlock = this.FindControl<TextBlock>("TestResultBlock");
            var valueAssertionsGrid = this.FindControl<Avalonia.Controls.DataGrid>("ValueAssertionsGrid");
            var addAssertionBtn = this.FindControl<Button>("AddAssertionBtn");
            var addCommonHeaderBtn = this.FindControl<Button>("AddCommonHeaderBtn");
            var headerValidationBlock = this.FindControl<TextBlock>("HeaderValidationBlock");
            if (valueAssertionsGrid != null)
                valueAssertionsGrid.ItemsSource = _valueAssertions;
            if (addAssertionBtn != null)
                addAssertionBtn.Click += (s, e) => _valueAssertions.Add(new ValueAssertion());

            if (_headersGrid != null)
                _headersGrid.ItemsSource = _headers;
            if (_addHeaderBtn != null)
                _addHeaderBtn.Click += (s, e) => _headers.Add(new HeaderItem());
            if (addCommonHeaderBtn != null)
                addCommonHeaderBtn.Click += (s, e) => ShowCommonHeadersMenu();
            if (_headersGrid != null)
                _headersGrid.CellEditEnded += (s, e) => ValidateHeaders();
            if (headerValidationBlock != null)
                headerValidationBlock.Text = _headerValidationResult;


            if (_sendBtn != null)
                _sendBtn.Click += async (s, e) => await SendRequestAsync();
            if (_saveCallBtn != null)
                _saveCallBtn.Click += (s, e) => SaveCall();
            if (_deleteCallBtn != null)
                _deleteCallBtn.Click += (s, e) => DeleteCall();
            if (_savedCallsList != null)
            {
                _savedCallsList.ItemsSource = _savedCalls;
                _savedCallsList.SelectionChanged += (s, e) => LoadSelectedCall();
            }
            _db.Database.EnsureCreated();
            var runAllBtn = this.FindControl<Button>("RunAllTestsBtn");
            if (runAllBtn != null)
                runAllBtn.Click += async (s, e) => await RunAllTestsAsync();
            LoadSavedCalls();
        }

        // Show a menu to pick common headers and add to the grid
        private void ShowCommonHeadersMenu()
        {
            var menu = new ContextMenu();
            var items = new List<MenuItem>();
            foreach (var (name, value) in _commonHeaders)
            {
                var item = new MenuItem { Header = $"{name}: {value}" };
            // (removed accidental dlg.ShowDialog)
                items.Add(item);
            }
            menu.ItemsSource = items;
            menu.Open(_addHeaderBtn);
        }

        // Validate headers for duplicates and empty keys
        private void ValidateHeaders()
        {
            var errors = new List<string>();
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var h in _headers)
            {
                if (string.IsNullOrWhiteSpace(h.Key))
                    errors.Add("Header key cannot be empty.");
                else if (!keys.Add(h.Key))
                    errors.Add($"Duplicate header: {h.Key}");
            }
            _headerValidationResult = errors.Count == 0 ? "Headers OK" : string.Join("; ", errors);
            var headerValidationBlock = this.FindControl<TextBlock>("HeaderValidationBlock");
            if (headerValidationBlock != null)
                headerValidationBlock.Text = _headerValidationResult;
        }

        private async Task SendRequestAsync()
        {
            if (_urlBox == null || _methodCombo == null || _responseBox == null) return;
            var url = _urlBox.Text ?? string.Empty;
            var method = (_methodCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "GET";
            var client = new HttpClient();
            var req = new HttpRequestMessage(new HttpMethod(method), url);
            foreach (var h in _headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
                req.Headers.TryAddWithoutValidation(h.Key, h.Value);
            if (method is "POST" or "PUT" or "PATCH")
            {
                var json = _jsonBodyBox?.Text ?? string.Empty;
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
            try
            {
                var resp = await client.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();
                // Set last response headers on CTF tools view if available
                if (CtfToolsViewInstance != null)
                    CtfToolsViewInstance.SetLastResponseHeaders(
                        resp.Headers.Concat(resp.Content.Headers)
                            .ToDictionary(h => h.Key, h => string.Join(", ", h.Value)));
                _responseBox.Text = $"Status: {(int)resp.StatusCode} {resp.ReasonPhrase}\n\n{body}";
                TestResponse((int)resp.StatusCode, body);
            }
            catch (Exception ex)
            {
                _responseBox.Text = $"Error: {ex.Message}";
                if (_testResultBlock != null) _testResultBlock.Text = "Test: Error";
            }
        }


        // Reference to CTF tools view for header inspection
        public CtfToolsView? CtfToolsViewInstance { get; set; }

        private void TestResponse(int status, string body)
        {
            bool pass = true;
            var expectedStatus = 0;
            if (_expectedStatusBox != null && int.TryParse(_expectedStatusBox.Text, out var es))
                expectedStatus = es;
            if (expectedStatus > 0 && status != expectedStatus)
                pass = false;
            if (_expectedJsonBox != null && !string.IsNullOrWhiteSpace(_expectedJsonBox.Text))
            {
                try
                {
                    var expectedJson = JsonDocument.Parse(_expectedJsonBox.Text);
                    var actualJson = JsonDocument.Parse(body);
                    if (!JsonEquals(expectedJson.RootElement, actualJson.RootElement))
                        pass = false;
                }
                catch { pass = false; }
            }
            if (_expectedXmlBox != null && !string.IsNullOrWhiteSpace(_expectedXmlBox.Text))
            {
                try
                {
                    var expectedXml = XDocument.Parse(_expectedXmlBox.Text);
                    var actualXml = XDocument.Parse(body);
                    if (!XNode.DeepEquals(expectedXml, actualXml))
                        pass = false;
                }
                catch { pass = false; }
            }
            if (_testResultBlock != null)
                _testResultBlock.Text = pass ? "Test: PASS" : "Test: FAIL";
        }

        private bool JsonEquals(JsonElement a, JsonElement b)
        {
            if (a.ValueKind != b.ValueKind) return false;
            switch (a.ValueKind)
            {
                case JsonValueKind.Object:
                    var aProps = a.EnumerateObject().OrderBy(p => p.Name).ToList();
                    var bProps = b.EnumerateObject().OrderBy(p => p.Name).ToList();
                    if (aProps.Count != bProps.Count) return false;
                    for (int i = 0; i < aProps.Count; i++)
                        if (aProps[i].Name != bProps[i].Name || !JsonEquals(aProps[i].Value, bProps[i].Value))
                            return false;
                    return true;
                case JsonValueKind.Array:
                    var aArr = a.EnumerateArray().ToList();
                    var bArr = b.EnumerateArray().ToList();
                    if (aArr.Count != bArr.Count) return false;
                    for (int i = 0; i < aArr.Count; i++)
                        if (!JsonEquals(aArr[i], bArr[i])) return false;
                    return true;
                default:
                    return a.ToString() == b.ToString();
            }
        }

        private void SaveCall()
        {
            if (_urlBox == null || _methodCombo == null) return;
            var call = new ApiCallEntity
            {
                Method = (_methodCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "GET",
                Url = _urlBox.Text ?? string.Empty,
                HeadersJson = JsonSerializer.Serialize(_headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)).ToList()),
                JsonBody = _jsonBodyBox?.Text ?? string.Empty,
                ExpectedStatus = _expectedStatusBox?.Text,
                ExpectedJson = _expectedJsonBox?.Text,
                ExpectedXml = _expectedXmlBox?.Text,
                ValueAssertionsJson = JsonSerializer.Serialize(_valueAssertions.ToList()),
                TestName = _testNameBox?.Text ?? string.Empty
            };
            _db.ApiCalls.Add(call);
            _db.SaveChanges();
            LoadSavedCalls();
        }

        private void DeleteCall()
        {
            if (_savedCallsList?.SelectedItem is ApiCall call)
            {
                var entity = _db.ApiCalls.FirstOrDefault(x => x.Id == call.Id);
                if (entity != null)
                {
                    _db.ApiCalls.Remove(entity);
                    _db.SaveChanges();
                }
                LoadSavedCalls();
            }
        }

        private void LoadSavedCalls()
        {
            _savedCalls.Clear();
            foreach (var call in _db.ApiCalls.ToList())
            {
                var apiCall = new ApiCall
                {
                    Id = call.Id,
                    Method = call.Method,
                    Url = call.Url,
                    Headers = JsonSerializer.Deserialize<List<HeaderItem>>(call.HeadersJson) ?? new List<HeaderItem>(),
                    JsonBody = call.JsonBody,
                    ExpectedStatus = call.ExpectedStatus,
                    ExpectedJson = call.ExpectedJson,
                    ExpectedXml = call.ExpectedXml,
                    ValueAssertions = JsonSerializer.Deserialize<List<ValueAssertion>>(call.ValueAssertionsJson ?? "[]") ?? new List<ValueAssertion>(),
                    TestName = call.TestName
                };
                _savedCalls.Add(apiCall);
            }
        }
        // Modern test runner: run all saved API calls and show results
        public async Task RunAllTestsAsync()
        {
            var results = new List<string>();
            foreach (var call in _db.ApiCalls.ToList())
            {
                var headers = JsonSerializer.Deserialize<List<HeaderItem>>(call.HeadersJson) ?? new List<HeaderItem>();
                var valueAssertions = JsonSerializer.Deserialize<List<ValueAssertion>>(call.ValueAssertionsJson ?? "[]") ?? new List<ValueAssertion>();
                var req = new HttpRequestMessage(new HttpMethod(call.Method), call.Url);
                foreach (var h in headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
                    req.Headers.TryAddWithoutValidation(h.Key, h.Value);
                if (call.Method is "POST" or "PUT" or "PATCH")
                    req.Content = new StringContent(call.JsonBody ?? string.Empty, Encoding.UTF8, "application/json");
                try
                {
                    var client = new HttpClient();
                    var resp = await client.SendAsync(req);
                    var body = await resp.Content.ReadAsStringAsync();
                    bool pass = true;
                    var assertionResults = new List<string>();
                    if (!string.IsNullOrWhiteSpace(call.ExpectedStatus) && int.TryParse(call.ExpectedStatus, out var es))
                    {
                        bool statusPass = ((int)resp.StatusCode == es);
                        assertionResults.Add($"Status {(int)resp.StatusCode} == {es}: {(statusPass ? "PASS" : "FAIL")}");
                        pass &= statusPass;
                    }
                    if (!string.IsNullOrWhiteSpace(call.ExpectedJson))
                    {
                        try
                        {
                            var expectedJson = JsonDocument.Parse(call.ExpectedJson);
                            var actualJson = JsonDocument.Parse(body);
                            bool jsonPass = JsonEquals(expectedJson.RootElement, actualJson.RootElement);
                            assertionResults.Add($"JSON match: {(jsonPass ? "PASS" : "FAIL")}");
                            pass &= jsonPass;
                        }
                        catch { assertionResults.Add("JSON match: FAIL"); pass = false; }
                    }
                    if (!string.IsNullOrWhiteSpace(call.ExpectedXml))
                    {
                        try
                        {
                            var expectedXml = XDocument.Parse(call.ExpectedXml);
                            var actualXml = XDocument.Parse(body);
                            bool xmlPass = XNode.DeepEquals(expectedXml, actualXml);
                            assertionResults.Add($"XML match: {(xmlPass ? "PASS" : "FAIL")}");
                            pass &= xmlPass;
                        }
                        catch { assertionResults.Add("XML match: FAIL"); pass = false; }
                    }
                    // Value assertions (JSONPath, expected value, type)
                    foreach (var va in valueAssertions)
                    {
                        try
                        {
                            var actual = ExtractJsonValue(body, va.JsonPath);
                            bool valuePass = actual == va.ExpectedValue;
                            assertionResults.Add($"{va.JsonPath} == {va.ExpectedValue}: {(valuePass ? "PASS" : $"FAIL (actual: {actual})")}");
                            pass &= valuePass;
                        }
                        catch (Exception ex)
                        {
                            assertionResults.Add($"{va.JsonPath} == {va.ExpectedValue}: ERROR {ex.Message}");
                            pass = false;
                        }
                    }
                    results.Add($"{call.Method} {call.Url}: {(pass ? "PASS" : "FAIL")}\n  " + string.Join("\n  ", assertionResults));
                }
                catch (Exception ex)
                {
                    results.Add($"{call.Method} {call.Url}: ERROR {ex.Message}");
                }
            }
            var resultText = string.Join("\n\n", results);
            if (_responseBox != null)
                _responseBox.Text = resultText;
            if (_testResultBlock != null)
                _testResultBlock.Text = results.All(r => r.Contains("PASS")) ? "All Tests PASS" : "Some Tests Failed";
        }
        // Extract value from JSON by simple path (dot notation, e.g. data.token)
        private string? ExtractJsonValue(string json, string path)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var parts = path.Split('.');
                JsonElement elem = doc.RootElement;
                foreach (var part in parts)
                {
                    if (elem.ValueKind == JsonValueKind.Object && elem.TryGetProperty(part, out var next))
                        elem = next;
                    else
                        return null;
                }
                return elem.ToString();
            }
            catch { return null; }
        }

        private void LoadSelectedCall()
        {
            if (_savedCallsList?.SelectedItem is ApiCall call)
            {
                if (_methodCombo != null)
                    _methodCombo.SelectedIndex = _methodCombo.Items.OfType<ComboBoxItem>().ToList().FindIndex(i => (i.Content?.ToString() ?? "GET") == call.Method);
                if (_urlBox != null)
                    _urlBox.Text = call.Url;
                _headers.Clear();
                foreach (var h in call.Headers)
                    _headers.Add(new HeaderItem { Key = h.Key, Value = h.Value });
                if (_jsonBodyBox != null)
                    _jsonBodyBox.Text = call.JsonBody;
                if (_expectedStatusBox != null)
                    _expectedStatusBox.Text = call.ExpectedStatus ?? string.Empty;
                if (_expectedJsonBox != null)
                    _expectedJsonBox.Text = call.ExpectedJson ?? string.Empty;
                if (_expectedXmlBox != null)
                    _expectedXmlBox.Text = call.ExpectedXml ?? string.Empty;
            }
        }

        public class HeaderItem
        {
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }
        public class ValueAssertion
        {
            public string JsonPath { get; set; } = string.Empty; // e.g. data.token
            public string ExpectedValue { get; set; } = string.Empty;
        }

        public class ApiCall
        {
            public int Id { get; set; }
            public string Method { get; set; } = "GET";
            public string Url { get; set; } = string.Empty;
            public List<HeaderItem> Headers { get; set; } = new();
            public string JsonBody { get; set; } = string.Empty;
            public string? ExpectedStatus { get; set; }
            public string? ExpectedJson { get; set; }
            public string? ExpectedXml { get; set; }
            public List<ValueAssertion> ValueAssertions { get; set; } = new();
            public string? TestName { get; set; }
            public override string ToString() => !string.IsNullOrWhiteSpace(TestName) ? TestName : $"{Method} {Url}";
        }

        // Export all API tests to JSON using Avalonia StorageProvider (Avalonia 11+)
        private async Task ExportTestsAsync()
        {
            var all = _db.ApiCalls.ToList();
            var json = JsonSerializer.Serialize(all, new JsonSerializerOptions { WriteIndented = true });
            var visualRoot = Avalonia.VisualTree.VisualExtensions.GetVisualRoot(this);
            var window = visualRoot as Window;
            if (window == null && Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                window = desktop.MainWindow;
            if (window?.StorageProvider == null) return;
            var file = await window.StorageProvider.SaveFilePickerAsync(new Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Export API Tests",
                SuggestedFileName = "apitests.json",
                FileTypeChoices = new[] { new Avalonia.Platform.Storage.FilePickerFileType("JSON File") { Patterns = new[] { "*.json" } } }
            });
            if (file != null)
            {
                using var stream = await file.OpenWriteAsync();
                using var writer = new System.IO.StreamWriter(stream);
                writer.Write(json);
            }
        }

        // Import API tests from JSON using Avalonia StorageProvider (Avalonia 11+)
        private async Task ImportTestsAsync()
        {
            var visualRoot = Avalonia.VisualTree.VisualExtensions.GetVisualRoot(this);
            var window = visualRoot as Window;
            if (window == null && Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
                window = desktop.MainWindow;
            if (window?.StorageProvider == null) return;
            var files = await window.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Import API Tests",
                AllowMultiple = false,
                FileTypeFilter = new[] { new Avalonia.Platform.Storage.FilePickerFileType("JSON File") { Patterns = new[] { "*.json" } } }
            });
            var file = files?.FirstOrDefault();
            if (file != null)
            {
                using var stream = await file.OpenReadAsync();
                using var reader = new System.IO.StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var imported = System.Text.Json.JsonSerializer.Deserialize<List<ApiCallEntity>>(json);
                if (imported != null)
                {
                    foreach (var call in imported)
                    {
                        // Avoid duplicates by URL+Method+TestName
                        if (!_db.ApiCalls.Any(x => x.Url == call.Url && x.Method == call.Method && x.TestName == call.TestName))
                        {
                            _db.ApiCalls.Add(call);
                        }
                    }
                    _db.SaveChanges();
                    LoadSavedCalls();
                }
            }
        }
    }
}
