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
using System.Globalization;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace LTI_RouterOS
{
    public partial class Form1 : Form
    {
        private readonly HttpClient httpClient;
        private string baseUrl;
        private MethodsController Controller; // Declaration of getData variable
        private bool isConnected = false;
        private WifiSecurityProfile wifiProfile;
        private WirelessSettings wirelessSettings;

        public Form1()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            InitializeComponent();
            httpClient = new HttpClient();
            wifiProfile = new WifiSecurityProfile();
            wirelessSettings = new WirelessSettings();
            InitializeComboBoxes();
        }
        private void InitializeComboBoxes()
        {
            textBox4.Enabled = false;
            textBox5.Enabled = false;
            textBox6.Enabled = false;
            textBox7.Enabled = false;
            comboBoxARP.SelectedIndex = 1;
            checkedListBox3.Enabled = false;
            checkedListBox1.Enabled = false;
            checkedListBox2.Enabled = false;
            comboBoxInterfaces.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxBridgeInterfaces.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxARP.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox17.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            PopulateCountryNamesComboBox();
        }

        private async void connectButton_Click(object sender, EventArgs e)
        {
            string ipAddress = textBox1.Text.Trim();
            string username = textBox9.Text.Trim();
            string password = textBox10.Text;

            try
            {
                baseUrl = "https://" + ipAddress;
                Controller = new MethodsController(username, password, ipAddress); // Instantiate GET class after user provides credentials
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
            await Controller.TestConnection(); // Test connection asynchronously
            isConnected = true;
            MessageBox.Show("Connected to router successfully!");
        }

        private async void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                string response = await Controller.Retrieve("/rest/interface");
                List<string> interfaceNames = ParseNamesFromJsonArray(response, "default-name");
                InterfacesBox.Text = interfaceNames.Count > 0 ? string.Join(Environment.NewLine, interfaceNames) : "No interface names found.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving interface data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string response = await Controller.Retrieve("/rest/interface/wireless");
                List<string> wirelessNames = ParseNamesFromJsonArray(response, "default-name");

                InterfacesBox.Text = wirelessNames.Count > 0 ? string.Join(Environment.NewLine, wirelessNames) : "No Wireless interface names found.";
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
                textBox2.Text = await Controller.GetBridges("/rest/interface/bridge");
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
                // Check if the JSON object contains the specified property
                if (jsonObject.ContainsKey(propertyName))
                {
                    string name = (string)jsonObject[propertyName];
                    names.Add(name);
                }
            }
            return names;
        }



        private async void comboBox1_Enter(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of bridges
                string response = await Controller.Retrieve("/rest/interface/bridge");
                List<string> bridgeList = ParseNamesFromJsonArray(response, "name");
                // Clear existing items in the ComboBox
                comboBox1.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string bridgeName in bridgeList)
                {
                    comboBox1.Items.Add(bridgeName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving bridge data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void comboBox2_Enter(object sender, EventArgs e)
        {
            try
            {
                comboBox1.Text = await Controller.GetBridges("/rest/interface/bridge");
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

        private async void button5_Click(object sender, EventArgs e)
        {
            //Defaults
            string bridgeName = textBoxBridgeName.Text;
            int mtu = (int)numericUpDown1.Value;
            string arpEnabled = comboBoxARP.SelectedItem.ToString();
            string arpTimeout = string.IsNullOrWhiteSpace(textBoxArpTimeoutBridge.Text) ? null : ParseTimeFormat(textBoxArpTimeoutBridge.Text);
            string ageingTime = string.IsNullOrWhiteSpace(textBoxAgeingTime.Text) ? null : ParseTimeFormat(textBoxAgeingTime.Text);
            bool dhcpSnooping = checkBoxDHCPSnooping.Checked;
            bool igmpSnooping = checkBoxIGMP.Checked;
            bool fastForward = checkBoxFF.Checked;

            try
            {
                await Controller.CreateBridge(bridgeName, mtu, arpEnabled, arpTimeout, ageingTime, igmpSnooping, dhcpSnooping, fastForward);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check the selected item in the combo box
            string selectedItem = comboBox3.SelectedItem.ToString();

            // Enable/disable text boxes or checkboxes based on the selected item
            if (selectedItem == "none")
            {
                checkedListBox1.SetItemChecked(0, false);
                checkedListBox1.SetItemChecked(1, false);
                checkedListBox1.SetItemChecked(2, false);
                checkedListBox1.SetItemChecked(3, false);
                // Disable certain text boxes or checkboxes
                checkedListBox1.Enabled = false;
                checkedListBox2.SetItemChecked(0, true);
                checkedListBox2.Enabled = false;
                checkedListBox3.SetItemChecked(0, true);
                checkedListBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;

            }
            else if (selectedItem == "dynamic keys")
            {
                checkedListBox1.Enabled = true;
                checkedListBox2.SetItemChecked(0, true);
                checkedListBox3.SetItemChecked(0, true);
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;

            }
            else if (selectedItem == "static keys optional")
            {
                checkedListBox1.SetItemChecked(0, false);
                checkedListBox1.SetItemChecked(1, false);
                checkedListBox1.SetItemChecked(2, false);
                checkedListBox1.SetItemChecked(3, false);


                checkedListBox1.Enabled = false;
                checkedListBox2.SetItemChecked(0, true);
                checkedListBox2.Enabled = false;
                checkedListBox3.SetItemChecked(0, true);
                checkedListBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = false;

            }
            else if (selectedItem == "static keys required")
            {
                checkedListBox1.SetItemChecked(0, false);
                checkedListBox1.SetItemChecked(1, false);
                checkedListBox1.SetItemChecked(2, false);
                checkedListBox1.SetItemChecked(3, false);
                checkedListBox1.Enabled = false;
                checkedListBox2.SetItemChecked(0, true);
                checkedListBox2.Enabled = false;
                checkedListBox3.SetItemChecked(0, true);
                checkedListBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = false;
            }
        }
    

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = comboBox4.SelectedItem.ToString();
        }

        private void PopulateCountryNamesComboBox()
        {
            // Clear any existing items in the ComboBox
            comboBoxCountryCodes.Items.Clear();

            // Get all countries
            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo culture in cultures)
            {
                RegionInfo region = new RegionInfo(culture.Name);
                // Check if the country name is not already in the ComboBox
                if (!comboBoxCountryCodes.Items.Contains(region.DisplayName))
                {
                    // Add the country name to the ComboBox
                    comboBoxCountryCodes.Items.Add(region.DisplayName);
                }
            }

            // Optionally, sort the items in the ComboBox
            comboBoxCountryCodes.Sorted = true;
        }

        private void comboBoxCountryCodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCountryName = comboBoxCountryCodes.SelectedItem.ToString();
            // Do something with the selected country name
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = comboBox1.SelectedItem.ToString();
            // Enable/disable text boxes or checkboxes based on the selected item
            if (selectedItem == "disabled")
            {
                textBox8.Enabled = false;
            }
            else if (selectedItem == "allowed")
            {

                textBox8.Enabled = true;
            }
            else if (selectedItem == "required")
            {
                textBox8.Enabled = true;
            }
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Get the index of the checked item
            int index = e.Index;

            // Determine which item is being checked
            switch (index)
            {
                case 0: // First item checked
                    textBox4.Enabled = e.NewValue == CheckState.Checked;
                    break;
                case 1: // Second item checked
                    textBox5.Enabled = e.NewValue == CheckState.Checked;
                    break;
                case 2: // Third item checked
                    textBox6.Enabled = e.NewValue == CheckState.Checked;
                    break;
                case 3: // Fourth item checked
                    textBox6.Enabled = e.NewValue == CheckState.Checked;
                    break;
                default:
                    break;
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            string bridge = comboBox1.SelectedItem.ToString();
            try
            {
                await Controller.DeleteBridge(bridge);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                textBox2.Text = await Controller.GetBridges("/rest/interface/bridge");
            }

        }

        private async void button6_Click(object sender, EventArgs e)
        {
            //Defaults
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Select a bridge");
                return;
            }
            string bridgeName = comboBox1.SelectedItem.ToString();
            int mtu = (int)numericUpDown1.Value;
            string arpEnabled = comboBoxARP.SelectedItem.ToString();
            string arpTimeout = string.IsNullOrWhiteSpace(textBoxArpTimeoutBridge.Text) ? null : ParseTimeFormat(textBoxArpTimeoutBridge.Text);
            string ageingTime = string.IsNullOrWhiteSpace(textBoxAgeingTime.Text) ? null : ParseTimeFormat(textBoxAgeingTime.Text);
            bool dhcpSnooping = checkBoxDHCPSnooping.Checked;
            bool igmpSnooping = checkBoxIGMP.Checked;
            bool fastForward = checkBoxFF.Checked;

            try
            {
                await Controller.UpdateBridge(bridgeName, mtu, arpEnabled, arpTimeout, ageingTime, igmpSnooping, dhcpSnooping, fastForward);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                textBox2.Text = await Controller.GetBridges("/rest/interface/bridge");
            }
        }

        private async void comboBoxInterfaces_Enter(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of bridges
                string response = await Controller.Retrieve("/rest/interface");
                List<string> intList = ParseNamesFromJsonArray(response, "default-name");
                // Clear existing items in the ComboBox
                comboBoxInterfaces.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string intName in intList)
                {
                    comboBoxInterfaces.Items.Add(intName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving interfaces data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void comboBoxBridgeInterfaces_Enter(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of bridges
                string response = await Controller.Retrieve("/rest/interface/bridge");
                List<string> bridgeList = ParseNamesFromJsonArray(response, "name");
                // Clear existing items in the ComboBox
                comboBoxBridgeInterfaces.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string bridgeName in bridgeList)
                {
                    comboBoxBridgeInterfaces.Items.Add(bridgeName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving bridge data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button17_Click(object sender, EventArgs e)
        {
            // Check if options are checked
            bool unknownUnicastFlood = checkBoxUnicastFlood.Checked;
            bool broadcastFlood = checkBoxBroadcast.Checked;
            bool hardwareOffload = checkBoxHardwareOffload.Checked;
            bool unknownMulticastFlood = checkBoxMulticast.Checked;
            bool trusted = checkBoxTrusted.Checked;

            bool fastLeave = checkBoxFastLeave.Checked;

            try
            {
                // Get selected interface from ComboBox
                string selectedInterface = comboBoxInterfaces.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedInterface))
                {
                    MessageBox.Show("Please select an interface.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string response = await Controller.Retrieve("/rest/interface/bridge/port?interface=" + selectedInterface + "");
                List<string> list = ParseNamesFromJsonArray(response, ".id");
                string id = list[0].ToString();

                // Get selected bridge from ComboBox
                string selectedBridge = comboBoxBridgeInterfaces.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedBridge))
                {
                    MessageBox.Show("Please select a bridge.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string multicastRouter = comboBox17.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(multicastRouter))
                {
                    MessageBox.Show("Please select an option.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Get horizon value
                int horizonValue;
                if (!int.TryParse(numericUpDownHorizon.Text, out horizonValue))
                {
                    MessageBox.Show("Invalid horizon value. Please enter a valid integer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Get learn option
                string learnOption = comboBox2.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(learnOption))
                {
                    MessageBox.Show("Please select a learn option.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Perform operations using the method controller
                await Controller.AssociateBridge(id, selectedBridge, horizonValue, learnOption, unknownUnicastFlood, broadcastFlood, hardwareOffload, unknownMulticastFlood, trusted, multicastRouter, fastLeave);

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {
                    
        }

        private WirelessSettings RetrieveWirelessSettings(string name)
        {
            try
            {
                // Make an HTTP GET request to the specified endpoint ("/rest/interface/wireless")
                HttpResponseMessage response = httpClient.GetAsync("https://192.168.79.1/rest/interface/wireless").Result;
                response.EnsureSuccessStatusCode(); // Throw an exception if the response is not successful

                // Read the response content as a string
                string responseBody = response.Content.ReadAsStringAsync().Result;

                // Deserialize the JSON response into a list of WirelessSettings objects
                List<WirelessSettings> settingsList = JsonConvert.DeserializeObject<List<WirelessSettings>>(responseBody);

                // Find the WirelessSettings object with the matching name
                return settingsList.FirstOrDefault(s => s.Name == name);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async void WirelessInterfaceCombobox_Enter(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of wirelessInterfaces
                string response = await Controller.Retrieve("/rest/interface/wireless");
                List<string> wirelessList = ParseNamesFromJsonArray(response, "name");

                // Clear existing items in the ComboBox
                comboBox1.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string wirelessName in wirelessList)
                {
                    WirelessInterfaceCombobox.Items.Add(wirelessName);
                }
            }
            catch (Exception ex) { 
                MessageBox.Show("Error retrieving Wireless Interface data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ParseTimeFormat(string time)
        {
            // Assuming time is in the format HH:MM:SS
            string[] parts = time.Split(':');
            if (parts.Length != 3)
            {
                MessageBox.Show("Time format hh:mm:ss");
            }

            // Convert each part to an integer
            int hours = int.Parse(parts[0]);
            int minutes = int.Parse(parts[1]);
            int seconds = int.Parse(parts[2]);

            // Return the formatted time string
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
        }


        private async void WirelessInterfaceCombobox_IndexChanged(object sender, EventArgs e)
        {
            string name = WirelessInterfaceCombobox.SelectedItem.ToString();
            WirelessSettings settings = RetrieveWirelessSettings(name);

            if (settings != null)
            {
                // Update the TextBox controls with the corresponding properties
                textBoxWirelessName.Text = settings.Name;
                textBoxWirelessMTU.Text = settings.Mtu.ToString();
                textBoxL2MTU.Text = settings.L2Mtu.ToString();
                textBoxMACAddr.Text = settings.MacAddress;
                comboBoxWirelessARP.SelectedItem = settings.Arp;
                comboBoxWirelessMode.SelectedItem = settings.Mode;
                comboBoxWirelessBand.SelectedItem = settings.Band;
                comboBoxChannelWidth.SelectedItem = settings.ChannelWidth;
                comboBoxFrequency.SelectedItem = settings.Frequency.ToString();
                textBoxSSID.Text = settings.Ssid;
                comboBoxSecProfile.SelectedItem = settings.SecurityProfile;
                comboBoxCountryCodes.SelectedItem = settings.Country;
                comboBoxFreqMode.SelectedItem = settings.FrequencyMode;
                comboBox14.SelectedItem = settings.Installation;
                checkBox1.Checked = settings.DefaultAuthentication;
                textBox14.Text = settings.ArpTimeout;
            }

        }


    }
}
