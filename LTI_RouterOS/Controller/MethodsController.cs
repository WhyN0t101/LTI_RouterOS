using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LTI_RouterOS.Controller
{
    internal class MethodsController
    {
        private readonly HttpClient httpClient;
        private readonly string credentials;
        private readonly string baseUrl;

        public MethodsController(string username, string password, string baseUrl)
        {
            httpClient = new HttpClient();
            // Check if baseUrl is provided and not empty
            if (!string.IsNullOrEmpty(baseUrl))
            {
                // Ensure the URL starts with "https://"
                if (!baseUrl.StartsWith("https://"))
                {
                    baseUrl = "https://" + baseUrl;
                }
            }
            else
            {
                throw new ArgumentException("Base URL cannot be null or empty.");
            }

            this.baseUrl = baseUrl;
            credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        }

        public async Task<string> Retrieve(string endpoint)
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

        public async Task<string> GetBridges(string endpoint)
        {
            try
            {
                string response = await Retrieve(endpoint);
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
        public async Task CreateBridge(string bridgeName)
        {
            try
            {
                // Construct the JSON payload for creating the bridge
                JObject payload = new JObject();
                payload["name"] = bridgeName;

                // Serialize the JSON payload
                string jsonPayload = payload.ToString();

                // Define the API endpoint URL for creating bridges
                string apiUrl = baseUrl + "/api/bridge/add";

                // Send a POST request to create the bridge
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Bridge created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error creating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
