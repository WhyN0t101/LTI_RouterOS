using System;
using System.Net.Http;
using System.Text;

namespace LTI_RouterOS.Controller
{
    internal class GET
    {
        private readonly HttpClient httpClient;
        private readonly string credentials;

        public GET()
        {
            httpClient = new HttpClient();
            credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("admin:"));
        }

        public string RetrieveData(string baseUrl, string endpoint)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                HttpResponseMessage response = httpClient.GetAsync(baseUrl + endpoint).Result;
                response.EnsureSuccessStatusCode(); // Ensure success status code
                return response.Content.ReadAsStringAsync().Result;
            }
            catch (HttpRequestException ex)
            {
                // Handle request errors
                throw new Exception("Error retrieving data: " + ex.Message);
            }
        }
    }
}
