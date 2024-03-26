using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LTI_RouterOS.Controller
{
    internal class GET
    {
        private readonly HttpClient httpClient;
        private readonly string credentials;

        public GET(string username, string password)
        {
            httpClient = new HttpClient();
            credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        }

        public async Task<string> Retrieve(string baseUrl, string endpoint)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                HttpResponseMessage response = await httpClient.GetAsync(baseUrl + endpoint);
                response.EnsureSuccessStatusCode(); // Ensure success status code
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // Handle request errors
                throw new Exception("Error retrieving data: " + ex.Message);
            }
        }

        public async Task<string> GetBridges(string baseUrl, string endpoint)
        {
            try
            {
                string response = await Retrieve(baseUrl, endpoint);
                List<string> bridgeNames = ParseNamesFromJsonArray(response, "name");

                return bridgeNames.Count > 0 ? string.Join(Environment.NewLine, bridgeNames) : "No Bridges Found";
            }
            catch (Exception ex)
            {
                // Handle request errors
                throw new Exception("Error retrieving bridge data: " + ex.Message);
            }
        }

        public async Task TestConnection(string baseUrl)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(baseUrl);
                response.EnsureSuccessStatusCode(); // Ensure success status code
            }
            catch (Exception ex)
            {
                // Handle request errors
                throw new Exception("Error testing connection: " + ex.Message);
            }
        }

        private List<string> ParseNamesFromJsonArray(string json, string propertyName)
        {
            List<string> names = new List<string>();
            JArray jsonArray = JArray.Parse(json);
            foreach (JObject jsonObject in jsonArray)
            {
                string name = (string)jsonObject[propertyName];
                names.Add(name);
            }
            return names;
        }
    }
}
