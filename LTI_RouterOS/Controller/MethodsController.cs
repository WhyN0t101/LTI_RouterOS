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
        private readonly string baseUrl;

        public MethodsController(string username, string password, string baseUrl)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentException("Base URL cannot be null or empty.");
            }

            this.baseUrl = baseUrl.StartsWith("https://") ? baseUrl : "https://" + baseUrl;

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
        }

        public async Task<string> Retrieve(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(baseUrl + endpoint);
                response.EnsureSuccessStatusCode(); // Ensure success status code
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
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
                throw new Exception("Error retrieving bridge data: " + ex.Message);
            }
        }

        public async Task TestConnection()
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(baseUrl);
                response.EnsureSuccessStatusCode(); // Ensure success status code
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error testing connection: " + ex.Message);
            }
        }

        public async Task CreateBridge(string bridgeName, int mtu, string arpEnabled, string arpTimeout, string ageingTime, bool igmpSnooping, bool dhcpSnooping, bool fastForward)
        {
            try
            {
                string apiUrl = baseUrl + "/api/bridge/add";

                // Construct the JSON payload for creating the bridge
                JObject payload = new JObject
                {
                    ["name"] = bridgeName,
                    ["mtu"] = mtu,
                    ["arp"] = arpEnabled,
                    ["arp-timeout"] = arpTimeout,
                    ["ageing-time"] = ageingTime,
                    ["igmp-snooping"] = igmpSnooping,
                    ["dhcp-snooping"] = dhcpSnooping,
                    ["fastforward"] = fastForward
                };

                // Serialize the JSON payload
                string jsonPayload = payload.ToString();

                // Send a POST request to create the bridge
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Bridge created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error creating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private List<string> ParseNamesFromJsonArray(string json, string propertyName)
        {
            var names = new List<string>();
            JArray jsonArray = JArray.Parse(json);
            foreach (JObject jsonObject in jsonArray)
            {
                if (jsonObject.TryGetValue(propertyName, out var value) && value.Type == JTokenType.String)
                {
                    names.Add(value.ToString());
                }
            }
            return names;
        }
    }
}
