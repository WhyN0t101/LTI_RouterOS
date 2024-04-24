using LTI_RouterOS.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

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
                response.EnsureSuccessStatusCode();
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
        public async Task DeactivateBridge(string selectedID)
        {
            try
            {
                string apiUrl = $"{baseUrl}/rest/interface/bridge/port/{selectedID}";

                HttpResponseMessage response = await httpClient.DeleteAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Bridge deactivated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error deactivating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task CreateBridge(JObject payload)
        {
            try
            {
                string apiUrl = baseUrl + "/rest/interface/bridge/add";

                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Bridge created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error creating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task EditBridge(string bridgeId, JObject payload)
        {
            try
            {
                string apiUrl = $"{baseUrl}/rest/interface/bridge/{bridgeId}";

                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Bridge updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error updating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task AssociateBridge(string selectedInterface, string selectedBridge, int horizonValue, string learnOption, bool unknownUnicastFlood, bool broadcastFlood, bool hardwareOffload, bool unknownMulticastFlood, bool trusted, string multicastRouter, bool fastLeave)
        {
            try
            {
                string apiUrl = $"{baseUrl}/rest/interface/bridge/port/{selectedInterface}";

                JObject payload = new JObject
                {
                    ["bridge"] = selectedBridge,
                    ["horizon"] = horizonValue,
                    ["learn"] = learnOption,
                    ["multicast-router"] = multicastRouter,
                    ["unknown-unicast-flood"] = unknownUnicastFlood ? "true" : "false",
                    ["broadcast-flood"] = broadcastFlood ? "true" : "false",
                    ["hw"] = hardwareOffload ? "true" : "false",
                    ["unknown-multicast-flood"] = unknownMulticastFlood ? "true" : "false",
                    ["trusted"] = trusted ? "true" : "false",
                    ["fast-leave"] = fastLeave ? "true" : "false",
                };

                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Bridge associated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error associating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task CreatePortToBridgeConnection(string selectedInterface, string selectedBridge, int horizonValue, string learnOption, bool unknownUnicastFlood, bool broadcastFlood, bool hardwareOffload, bool unknownMulticastFlood, bool trusted, string multicastRouter, bool fastLeave)
        {
            try
            {
                string apiUrl = $"{baseUrl}/rest/interface/bridge/port/add";

                JObject payload = new JObject
                {
                    ["interface"] = selectedInterface,
                    ["bridge"] = selectedBridge
                };

                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                string res = await Retrieve("/rest/interface/bridge/port?interface=" + selectedInterface + "");
                JArray jsonArray = JArray.Parse(res);
                List<string> list = ParseNamesFromJsonArray(res, ".id");
                string id = list[0].ToString();


                await AssociateBridge(id, selectedBridge, horizonValue, learnOption, unknownUnicastFlood, broadcastFlood, hardwareOffload, unknownMulticastFlood, trusted, multicastRouter, fastLeave);
                MessageBox.Show("Port-to-bridge connection created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error creating port-to-bridge connection: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

 



        public async Task DeleteBridge(string bridgeName)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/interface/bridge/{bridgeName}";

                // Send a DELETE request to delete the bridge
                HttpResponseMessage response = await httpClient.DeleteAsync(apiUrl);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Bridge deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error deleting bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public async Task DeleteRoute(string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/ip/route/{id}";

                // Send a DELETE request to delete the bridge
                HttpResponseMessage response = await httpClient.DeleteAsync(apiUrl);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Bridge deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error deleting bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        public async Task DeactivateWirelessInterface(string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/interface/wireless/{id}";

                JObject payload = new JObject
                {
                    ["disabled"] = "true"
                };

                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Wireless Interface Deactivated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error deactivating wireless Interface: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        public async Task ActivateWirelessInterface(string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/interface/wireless/{id}";

                JObject payload = new JObject
                {
                    ["disabled"] = "false"
                };

                // Send the PATCH request
                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Wireless Interface Activated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Activating wireless Interface: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        public async Task<HttpResponseMessage> SendPatchRequest(string apiUrl, JObject payload)
        {
            string jsonPayload = payload.ToString();
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), apiUrl);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            return await httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> SendPostRequest(string apiUrl, JObject payload)
        {
            string jsonPayload = payload.ToString();
            return await httpClient.PostAsync(apiUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
        }
        public async Task<HttpResponseMessage> SendPutRequest(string apiUrl, JObject payload)
        {
            string jsonPayload = payload.ToString();
            return await httpClient.PutAsync(apiUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
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

        public async Task ConfigureWirelessSettings(string id, JObject payload)
        {
            try
            {
                
                string apiUrl = baseUrl + $"/rest/interface/wireless/{id}";

                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Wireless Interface Configured successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Configuring wireless Interface: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public async Task CreateRoute(string destAddress, string gateway, string checkgateway, string distance, string scope, string targetscope, string vrfInt, string table, string prefsource, bool hw)
        {
            try
            {
                string apiUrl = baseUrl + "/rest/ip/route/add";

                JObject payload = new JObject
                {
                    ["dst-address"] = destAddress,
                    ["gateway"] = gateway,
                    ["check-gateway"] = checkgateway,
                    ["distance"] = distance,
                    ["scope"] = scope,
                    ["target-scope"] = targetscope,
                    ["vrf-interface"] = vrfInt,
                    ["routing-table"] = table,
                    ["pref-src"] = prefsource != null ? prefsource : "",
                    ["suppress-hw-offload"] = hw ? "true" : "false",
                };

                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Route created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex) { 
            
                MessageBox.Show("Error creating route: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    

        public async Task CreateWirelessSecurity(JObject payload)
        {
            try
            {
                string apiUrl = baseUrl + "/rest/interface/wireless/security-profiles";


                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Security profile created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error creating security profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task EditWirelessSecurity(JObject payload, string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/interface/wireless/security-profiles/{id}";


                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Security profile Edited successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Editing security profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task DeleteSecProfile(string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/interface/wireless/security-profiles/{id}";

                // Send a DELETE request to delete the bridge
                HttpResponseMessage response = await httpClient.DeleteAsync(apiUrl);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("Security Profile deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error deleting Security Profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public async Task UpdateRoute(string routeId, string destAddress, string gateway, string checkGateway, string distance, string scope, string targetScope, string vrfInterface, string routingTable, string prefSource, bool hw)
        {
            try
            {
                string apiUrl = baseUrl + "/rest/ip/route/" + routeId;

                JObject payload = new JObject
                {
                    ["dst-address"] = destAddress,
                    ["gateway"] = gateway,
                    ["check-gateway"] = checkGateway,
                    ["distance"] = distance,
                    ["scope"] = scope,
                    ["target-scope"] = targetScope,
                    ["vrf-interface"] = vrfInterface,
                    ["routing-table"] = routingTable,
                    ["pref-src"] = prefSource != null ? prefSource : "",
                    ["suppress-hw-offload"] = hw ? "true" : "false",
                };

                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Route updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error updating route: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task CreateIp(string address, string network, string inter)
        {
            try
            {
                string apiUrl = baseUrl + "/rest/ip/address";

                JObject payload = new JObject
                {
                    ["address"] = address,
                    ["network"] = network,
                    ["interface"] = inter,
                   
                };

                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);
                response.EnsureSuccessStatusCode();

                MessageBox.Show("Address created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("Error creating address: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task DeleteDHCPServer(string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/ip/dhcp-server/{id}";

                // Send a DELETE request to delete the bridge
                HttpResponseMessage response = await httpClient.DeleteAsync(apiUrl);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("DHCP Server deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle exceptions
                MessageBox.Show("Error deleting DHCP Server: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task CreateDHCPServer(JObject payload)
        {
            try
            {
                string apiUrl = baseUrl + "/rest/ip/dhcp-server/add";


                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("DHCP Server created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error creating DHCP Server: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task EditDHCPServer(JObject payload, string id)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/ip/dhcp-server/{id}";


                HttpResponseMessage response = await SendPatchRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("DHCP Server Edited successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Editing DHCP Server: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task DeactivateDNS()
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/ip/dns/set";

                JObject payload = new JObject
                {
                    ["servers"] = "",
                    ["allow-remote-requests"] = "false"

                };

                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("DNS Disabled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Disabling DNS Server: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task ActivateDNS()
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/ip/dns/set";

                JObject payload = new JObject
                {
                    ["servers"] = "8.8.8.8",
                    ["allow-remote-requests"] = "true"

                };

                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("DNS Enabled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Enabling DNS Server: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task EditDNS(JObject payload)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/ip/dns/set";


                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show("DNS Edited successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Editing DNS: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task ActivateDNSStatic(string id, string name)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/ip/dns/static/enable";

                JObject payload = new JObject
                {
                    [".id"] = id,
                };

                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show($"Static DNS {name} Enabled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Enabling Static DNS {name}: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task DeactivateDNSStatic(string id, string name)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/ip/dns/static/disable";

                JObject payload = new JObject
                {
                    [".id"] = id,
                };

                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show($"Static DNS {name} Disabled successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Disabling Static DNS {name}: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task RemoveDNSStatic(string id, string name)
        {
            try
            {
                string apiUrl = baseUrl + $"/rest/ip/dns/static/remove";

                JObject payload = new JObject
                {
                    [".id"] = id,
                };

                HttpResponseMessage response = await SendPostRequest(apiUrl, payload);

                // Check if the request was successful
                response.EnsureSuccessStatusCode();

                // Display success message
                MessageBox.Show($"Static DNS {name} Removed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Removing Static DNS {name}: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}



