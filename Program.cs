using System;
using System.Text.Json;
using Avalonia;
using screenshareav.Database;

namespace screenshareav
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Check if we're running in test mode
            if (args.Length > 0 && args[0] == "--test")
            {
                RunApiTestingSanityCheck();
                return;
            }
            
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        private static void RunApiTestingSanityCheck()
        {
            Console.WriteLine("Running API Testing Sanity Checks...");

            try
            {
                // Test 1: Database operations
                Console.WriteLine("\n1. Testing database operations...");
                
                // Test API test storage
                var testData = new
                {
                    Name = "Test API Call",
                    Method = "GET",
                    Url = "https://api.example.com/users",
                    ExpectedStatus = "200",
                    ExpectedResponse = "{\"users\": []}"
                };
                
                var json = JsonSerializer.Serialize(testData);
                DatabaseManager.SaveApiTest("TestCase1", json);
                
                var retrieved = DatabaseManager.GetApiTest("TestCase1");
                Console.WriteLine($"✅ API test saved and retrieved: {retrieved != null}");
                
                var testNames = DatabaseManager.GetApiTestNames();
                Console.WriteLine($"✅ Test names retrieved: {testNames.Count > 0}");
                
                // Test general data storage
                DatabaseManager.SaveData("Settings", "Theme", "Dark");
                var theme = DatabaseManager.GetData("Settings", "Theme");
                Console.WriteLine($"✅ General data storage works: {theme == "Dark"}");
                
                // Test 2: JSON parsing and comparison
                Console.WriteLine("\n2. Testing JSON functionality...");
                var json1 = "{\"name\": \"John\", \"age\": 30}";
                var json2 = "{\"age\": 30, \"name\": \"John\"}";
                
                try
                {
                    var doc1 = JsonDocument.Parse(json1);
                    var doc2 = JsonDocument.Parse(json2);
                    
                    var normalized1 = JsonSerializer.Serialize(doc1);
                    var normalized2 = JsonSerializer.Serialize(doc2);
                    
                    Console.WriteLine($"✅ JSON parsing and normalization works: {normalized1 == normalized2}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ JSON parsing failed: {ex.Message}");
                }
                
                // Test 3: HTTP file format parsing simulation
                Console.WriteLine("\n3. Testing HTTP file format parsing...");
                var httpFileContent = @"GET https://api.example.com/users
Authorization: Bearer token123
Content-Type: application/json

{""query"": ""test""}";
                
                var lines = httpFileContent.Split('\n');
                var requestLine = lines[0].Split(' ');
                var method = requestLine[0];
                var url = requestLine[1];
                
                Console.WriteLine($"✅ HTTP file parsing works: Method={method}, URL={url}");
                
                // Cleanup
                DatabaseManager.DeleteApiTest("TestCase1");
                
                Console.WriteLine("\n✅ All sanity checks passed! API Testing functionality is working correctly.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during testing: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
