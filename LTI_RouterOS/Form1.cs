using LTI_RouterOS.Controller;
using LTI_RouterOS.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LTI_RouterOS
{
    public partial class Form1 : Form
    {
        private readonly HttpClient httpClient;
        private string baseUrl;
        private MethodsController getController; // Declaration of getData variable
        private bool isConnected = false;
        private WifiSecurityProfile wifiProfile;

        public Form1()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            InitializeComponent();
            httpClient = new HttpClient();
            wifiProfile = new WifiSecurityProfile();

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            string ipAddress = textBox1.Text.Trim();
            string username = textBox9.Text.Trim();
            string password = textBox10.Text;

            try
            {
                baseUrl = "https://" + ipAddress;
                getController = new MethodsController(username, password,ipAddress); // Instantiate GET class after user provides credentials
                await Connect(ipAddress, username, password);
                MessageBox.Show("Connected to " + ipAddress, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to router: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async Task Connect(string ipAddress, string username, string password)
        {
            baseUrl = "https://" + ipAddress;
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            await getController.TestConnection(); // Test connection asynchronously
            isConnected = true;
            MessageBox.Show("Connected to router successfully!");
        }


        private async void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                string response = await getController.Retrieve("/rest/interface");
                List<string> interfaceNames = ParseNamesFromJsonArray(response, "default-name");

                InterfacesBox.Text = interfaceNames.Count > 0 ? string.Join(Environment.NewLine, interfaceNames) : "No interface names found.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving interface data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button3_Click_1(object sender, EventArgs e)
        {
            try
            {
                textBox2.Text = await getController.GetBridges("/rest/interface/bridge");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving bridge data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabControl1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // Check if connected to router before allowing access to other tabs
            if (!isConnected && tabControl1.SelectedIndex != 0)
            {
                MessageBox.Show("Please connect to the router first.");
                tabControl1.SelectedIndex = 0; // Switch back to the connection tab
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

        private async void comboBox1_Enter(object sender, EventArgs e)
        {
            try
            {
                comboBox1.Text = await getController.GetBridges("/rest/interface/bridge");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving bridge data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateWifiProfileFromForm()
        {
            // Update wifiProfile object with values from the form controls
            wifiProfile.Name = textBox3.Text;
            wifiProfile.Mode = comboBox3.SelectedItem.ToString();

            if (checkedListBox1.SelectedItem != null)
            {
                wifiProfile.AuthenticationType = checkedListBox1.SelectedItem.ToString();
            }
            else
            {
              wifiProfile.AuthenticationType = string.Empty; 
            }
            wifiProfile.UnicastCiphers = checkedListBox2.Text;
            wifiProfile.GroupCiphers = checkedListBox3.Text;
            wifiProfile.WpaPresharedKey = textBox4.Text;
            wifiProfile.Wpa2PresharedKey = textBox5.Text;
            wifiProfile.SupplicantIdentity = textBox6.Text;
            wifiProfile.GroupKeyUpdate = textBox7.Text;
            if (comboBox4.SelectedItem != null)
            {
                wifiProfile.AuthenticationType = comboBox4.SelectedItem.ToString();
            }
            else
            {
                wifiProfile.AuthenticationType = string.Empty;
            }
            wifiProfile.ManagementProtectionKey = textBox7.Text;
        }
        private async Task CreateWifiSecurityProfile(WifiSecurityProfile profile)
        {
            try
            {
                // Construct the JSON payload for the new security profile
                JObject payload = new JObject();
                payload["name"] = profile.Name;
                payload["mode"] = profile.Mode;
                payload["authentication-type"] = profile.AuthenticationType;
                payload["unicast-ciphers"] = profile.UnicastCiphers;
                payload["group-ciphers"] = profile.GroupCiphers;
                payload["wpa-pre-shared-key"] = profile.WpaPresharedKey;
                payload["wpa2-pre-shared-key"] = profile.Wpa2PresharedKey;
                payload["supplicant-identity"] = profile.SupplicantIdentity;
                payload["group-key-update"] = profile.GroupKeyUpdate.ToString().ToLower();

                // Check if ManagementProtection is not null before adding it to the payload
                if (profile.ManagementProtection != null)
                {
                    payload["management-protection"] = profile.ManagementProtection.ToString().ToLower();
                }
                else
                {
                    // Handle the case where ManagementProtection is null
                    // For example, set a default value or log a warning
                    payload["management-protection"] = "default_value";
                }

                payload["management-protection-key"] = profile.ManagementProtectionKey;

                // Serialize the JSON payload
                string jsonPayload = payload.ToString();

                // Define the API endpoint URL for creating security profiles
                string apiUrl = baseUrl + "/api/security-profiles";

                // Send a POST request to create the new security profile
                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

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



        private async void button8_Click(object sender, EventArgs e)
        {
            // Update wifiProfile object with values from the form before creating the security profile
            UpdateWifiProfileFromForm();

            // Create the security profile
            await CreateWifiSecurityProfile(wifiProfile);
        }

    }
}
