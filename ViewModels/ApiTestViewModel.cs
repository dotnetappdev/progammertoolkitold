using ReactiveUI;
using System.Collections.Generic;
using System.ComponentModel;

namespace screenshareav.ViewModels
{
    public class ApiTestViewModel : ReactiveObject
    {
        private string _method = "GET";
        private string _url = "";
        private string _requestBody = "";
        private string _response = "";
        private string _expectedResponse = "";
        private string _expectedStatus = "200";
        private string _testResults = "";

        public string Method
        {
            get => _method;
            set => this.RaiseAndSetIfChanged(ref _method, value);
        }

        public string Url
        {
            get => _url;
            set => this.RaiseAndSetIfChanged(ref _url, value);
        }

        public string RequestBody
        {
            get => _requestBody;
            set => this.RaiseAndSetIfChanged(ref _requestBody, value);
        }

        public string Response
        {
            get => _response;
            set => this.RaiseAndSetIfChanged(ref _response, value);
        }

        public string ExpectedResponse
        {
            get => _expectedResponse;
            set => this.RaiseAndSetIfChanged(ref _expectedResponse, value);
        }

        public string ExpectedStatus
        {
            get => _expectedStatus;
            set => this.RaiseAndSetIfChanged(ref _expectedStatus, value);
        }

        public string TestResults
        {
            get => _testResults;
            set => this.RaiseAndSetIfChanged(ref _testResults, value);
        }

        public List<string> HttpMethods { get; } = new List<string>
        {
            "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS"
        };
    }
}