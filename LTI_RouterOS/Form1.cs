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
using System.Xml.Linq;
using LTI_RouterOS.Properties;
using System.Text.RegularExpressions;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

namespace LTI_RouterOS
{
    public partial class Form1 : Form
    {
        private readonly HttpClient httpClient;
        private string baseUrl;
        private MethodsController Controller;
        private bool isConnected = false;
        private WifiSecurityProfile wifiProfile;
        private WirelessSettings wirelessSettings;
        private DHCPServer dhcpServer;
        private DNS dns;
        private Bridge bridge;
        private DNSStatic dnsStatic;
        private WireguardInterface wgInterface;
        private WireguardPeers wgPeer;
        private Json Parser = new Json();



        public Form1()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            InitializeComponent();
            httpClient = new HttpClient();
            wifiProfile = new WifiSecurityProfile();
            wirelessSettings = new WirelessSettings();
            dhcpServer = new DHCPServer();
            dns = new DNS();
            bridge = new Bridge();
            dnsStatic = new DNSStatic();
            wgInterface = new WireguardInterface();
            wgPeer = new WireguardPeers();
            InitializeComboBoxes();

        }
        private void InitializeComboBoxes()
        {
            textBox4.Enabled = false;
            textBox5.Enabled = false;
            textBox6.Enabled = false;
            textBox7.Enabled = false;
            textBox11.Enabled = false;
            textBoxWireguardPublicKey.Enabled = true;
            comboBoxARP.SelectedIndex = 1;
            //checkedListBox2.SetItemChecked(0, true);
            //checkedListBox3.SetItemChecked(0, true);
            checkedListBox3.Enabled = false;
            checkedListBox1.Enabled = false;
            checkedListBox2.Enabled = false;
            comboBoxInterfaces.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxBridgeInterfaces.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxARP.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox17.DropDownStyle = ComboBoxStyle.DropDownList;
            PopulateCountryNamesComboBox();
            comboBox17.SelectedIndex = 2;
            comboBox2.SelectedIndex = 0;
            comboBox15.SelectedIndex = 0;
            comboBoxCheckGateway.SelectedIndex = 2;

        }

        #region BaseConfigs

        private void Form1_Load_1(object sender, EventArgs e)
        {
            textBox1.Text = "192.168.79.1";
            textBox9.Text = "admin";
        }
        private async void tabControl1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // Check if connected to router before allowing access to other tabs
            if (!isConnected && tabControl1.SelectedIndex != 0)
            {
                MessageBox.Show("Please connect to the router first.");
                tabControl1.SelectedIndex = 0; // Switch back to the connection tab
            }
            TabControl tabControl = (TabControl)sender;
            switch (tabControl.SelectedIndex)
            {
                case 6:
                    PopulateDNSTab();
                    break;
                case 7:
                    await PopulatecomboBoxPeerInterface();
                    break;
            }
        }
        private async Task PopulatecomboBoxSecProfile()
        {
            // Clear any existing items in the ComboBox
            comboBoxSecProfile.Items.Clear();

            try
            {
                // Retrieve the list of wirelessInterfaces
                string response = await Controller.Retrieve("/rest/interface/wireless/security-profiles");
                List<string> SecProfiles = Parser.ParseNamesFromJsonArray(response, "name");

                // Clear existing items in the ComboBox
                comboBoxSecProfile.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string SecProfile in SecProfiles)
                {
                    comboBoxSecProfile.Items.Add(SecProfile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Security Profiles data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private async void connectButton_Click(object sender, EventArgs e)
        {
            string ipAddress = textBox1.Text.Trim();
            string username = textBox9.Text.Trim();
            string password = textBox10.Text;

            // Determine the protocol (HTTP or HTTPS) based on user selection
            string protocol = (HTTPs.Checked) ? "https://" : "http://";

            try
            {
                // Construct the base URL using the determined protocol and IP address
                string baseUrl = protocol + ipAddress;

                // Instantiate MethodsController class with user credentials and base URL
                Controller = new MethodsController(username, password, ipAddress);

                // Attempt to connect to the router
                await Connect(ipAddress, username, password,protocol);

                // If connection successful, display success message
                MessageBox.Show("Connected to " + ipAddress, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Proceed with populating DHCP tab or other necessary actions
            }
            catch (Exception ex)
            {
                // If an error occurs during connection, display error message
                MessageBox.Show("Error connecting to router: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task Connect(string ipAddress, string username, string password, string protocol)
        {
            baseUrl = protocol + ipAddress;
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            await Controller.TestConnection(); // Test connection asynchronously
            isConnected = true;
            textBox9.Enabled = false; // Username textbox
            textBox10.Enabled = false; // Password textbox
            textBox1.Enabled = false; // IP Address textbox
            MessageBox.Show("Connected to router successfully!");
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
                string countryNameLowerCase = region.EnglishName.ToLower();
                // Check if the country name is not already in the ComboBox
                if (!comboBoxCountryCodes.Items.Contains(countryNameLowerCase))
                {
                    // Add the country name to the ComboBox
                    comboBoxCountryCodes.Items.Add(countryNameLowerCase);
                }
            }
            string etsiCountry = "ETSI";
            comboBoxCountryCodes.Items.Add(etsiCountry.ToLower());

            // Optionally, sort the items in the ComboBox
            comboBoxCountryCodes.Sorted = true;
        }
        public static void ChangeDefaultFontSize(Font newFont)
        {
            // Set the default font for all controls
            typeof(Control).GetProperty("DefaultFont", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)?.SetValue(null, newFont);
        }

        #endregion


        #region Interfaces

        #region Listagens


        private async void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                string response = await Controller.Retrieve("/rest/interface");
                List<string> interfaceNames = Parser.ParseNamesFromJsonArray(response, "name");

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
                List<string> wirelessNames = Parser.ParseNamesFromJsonArray(response, "default-name");

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
        #endregion

        #region PopulateFields
        private async void comboBox1_Enter(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of bridges
                string response = await Controller.Retrieve("/rest/interface/bridge");
                List<string> bridgeList = Parser.ParseNamesFromJsonArray(response, "name");
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

        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear textboxes and comboboxes
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Text = "";
                }
                if (control is ComboBox)
                {
                    ((ComboBox)control).SelectedIndex = -1;
                }
            }

            string name = comboBox1.SelectedItem.ToString();
            Bridge bridgeLocal = RetrieveBridge(name);

            if (bridgeLocal != null)
            {
                bridge.Id = bridgeLocal.Id;
                textBoxBridgeName.Text = bridgeLocal.Name;
                textBoxBridgeMTU.Text = bridgeLocal.Mtu;
                comboBoxARP.SelectedItem = bridgeLocal.Arp;
                textBoxArpTimeoutBridge.Text = ConvertTimeFormat(bridgeLocal.ArpTimeout);
                textBoxAgeingTime.Text = ConvertTimeFormat(bridgeLocal.AgeingTime);
                checkBoxFF.Checked = bridgeLocal.FastForward;
                checkBoxIGMP.Checked = bridgeLocal.IgmpSnooping;
                checkBoxDHCPSnooping.Checked = bridgeLocal.DhcpSnooping;
            }
        }

        #endregion

        #region Add/Edit/Delete Bridge

        //Creates Bridge
        private async void button5_Click(object sender, EventArgs e)
        {
            //verifica time e password lenght
            if (ValidateAndUpdateTimeFormat(textBoxAgeingTime.Text, 3) == "")
            {
                return;
            }

            if (ValidateAndUpdateTimeFormat(textBoxArpTimeoutBridge.Text, 0) == "")
            {
                return;
            }

            if (textBoxBridgeName.Text == "")
            {
                MessageBox.Show("Select a Bridge Name. ");
                return;
            }

            if (textBoxBridgeMTU.Text == "" || !int.TryParse(textBoxBridgeMTU.Text, out int value) || value < 68 || value > 65535)
            {
                // Value is outside the range [32, 2290] or invalid input
                MessageBox.Show("Value of MTU is outside the range [64, 65535] or invalid input. ");
                return;
            }


            // Update bridge object with values from the form before creating the security profile
            UpdateBridgeFromForm();

            // Create the security profile
            await CreateBridge(bridge);
        }

        //Makes Payload and Creates Bridge
        private async Task CreateBridge(Bridge bridge)
        {
            try
            {
                // Construct the JSON payload for the new security profile

                JObject payload = new JObject
                {
                    ["name"] = bridge.Name,
                    ["mtu"] = bridge.Mtu,
                    ["arp"] = bridge.Arp,
                    ["arp-timeout"] = bridge.ArpTimeout,
                    ["ageing-time"] = bridge.AgeingTime,
                    ["igmp-snooping"] = bridge.IgmpSnooping,
                    ["dhcp-snooping"] = bridge.DhcpSnooping,
                    ["fast-forward"] = bridge.FastForward
                };

                await Controller.CreateBridge(payload);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error creating security profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        //Delete Bridge
        private async void button4_Click(object sender, EventArgs e)
        {
            string bridgeName = comboBox1.SelectedItem.ToString();
            try
            {
                await Controller.DeleteBridge(bridgeName);
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

        //Edit Bridge
        private async void button6_Click(object sender, EventArgs e)
        {

            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Select a bridge");
                return;
            }
            //verifica time e password lenght
            if (ValidateAndUpdateTimeFormat(textBoxAgeingTime.Text, 3) == "")
            {
                return;
            }

            if (ValidateAndUpdateTimeFormat(textBoxArpTimeoutBridge.Text, 0) == "")
            {
                return;
            }

            if (textBoxBridgeName.Text == "")
            {
                MessageBox.Show("Select a Bridge Name. ");
                return;
            }

            if (textBoxBridgeMTU.Text == "" || !int.TryParse(textBoxBridgeMTU.Text, out int value) || value < 68 || value > 65535)
            {
                // Value is outside the range [32, 2290] or invalid input
                MessageBox.Show("Value of MTU is outside the range [64, 65535] or invalid input. ");
                return;
            }

            // Update bridge object with values from the form before creating the security profile
            UpdateBridgeFromForm();

            // Create the security profile
            await EditBridge(bridge);

        }

        //Makes Payload and Edits Bridge
        private async Task EditBridge(Bridge bridge)
        {
            try
            {
                // Construct the JSON payload for the new security profile

                JObject payload = new JObject
                {
                    ["name"] = bridge.Name,
                    ["mtu"] = bridge.Mtu,
                    ["arp"] = bridge.Arp,
                    ["arp-timeout"] = bridge.ArpTimeout,
                    ["ageing-time"] = bridge.AgeingTime,
                    ["igmp-snooping"] = bridge.IgmpSnooping,
                    ["dhcp-snooping"] = bridge.DhcpSnooping,
                    ["fast-forward"] = bridge.FastForward
                };

                await Controller.EditBridge(bridge.Id, payload);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error creating security profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region AUX

        private void UpdateBridgeFromForm()
        {
            bridge.Name = textBoxBridgeName.Text;
            bridge.Mtu = textBoxBridgeMTU.Text;
            bridge.Arp = comboBoxARP.SelectedItem.ToString();
            bridge.ArpTimeout = textBoxArpTimeoutBridge.Text;
            bridge.AgeingTime = textBoxAgeingTime.Text;
            bridge.DhcpSnooping = checkBoxDHCPSnooping.Checked;
            bridge.IgmpSnooping = checkBoxIGMP.Checked;
            bridge.FastForward = checkBoxFF.Checked;

        }

        private Bridge RetrieveBridge(string name)
        {
            try
            {
                // Make an HTTP GET request to the specified endpoint ("/rest/interface/bridge")
                HttpResponseMessage response = httpClient.GetAsync(baseUrl + "/rest/interface/bridge").Result;
                response.EnsureSuccessStatusCode(); // Throw an exception if the response is not successful

                // Read the response content as a string
                string responseBody = response.Content.ReadAsStringAsync().Result;

                // Deserialize the JSON response into a list of WirelessSettings objects
                List<Bridge> bridgeList = JsonConvert.DeserializeObject<List<Bridge>>(responseBody);

                // Find the WirelessSettings object with the matching name
                return bridgeList.FirstOrDefault(s => s.Name == name);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }



        #endregion

        #endregion


        #region Wireless

        #region PopulateFields

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check the selected item in the combo box
            string selectedItem = comboBox3.SelectedItem.ToString();

            // Enable/disable text boxes or checkboxes based on the selected item
            if (selectedItem == "none")
            {
                // Disable certain text boxes or checkboxes
                checkedListBox1.Enabled = false;
                checkedListBox2.Enabled = false;
                checkedListBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = false;


            }
            else if (selectedItem == "dynamic keys")
            {
                checkedListBox1.Enabled = true;
                checkedListBox2.Enabled = true;
                checkedListBox3.Enabled = true;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = true;

            }
            else if (selectedItem == "static keys optional")
            {

                checkedListBox1.Enabled = false;
                checkedListBox2.Enabled = false;
                checkedListBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = false;

            }
            else if (selectedItem == "static keys required")
            {
                checkedListBox1.Enabled = false;
                checkedListBox2.Enabled = false;
                checkedListBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = false;
            }
        }

        #endregion

        #region AUX

        private void UpdateWifiProfileFromForm()
        {
            // Update wifiProfile object with values from the form controls
            wifiProfile.Name = textBox3.Text;
            wifiProfile.Mode = comboBox3.SelectedItem.ToString().Replace(" ", "-");
            string selectedItems = "";

            if (checkedListBox1.CheckedIndices.Count > 0)
            {
                selectedItems = GetAllSelectedItems(checkedListBox1);
                wifiProfile.AuthenticationType = selectedItems;

            }
            else
            {
                wifiProfile.AuthenticationType = string.Empty;
            }

            if (checkedListBox2.CheckedIndices.Count > 0)
            {
                selectedItems = GetAllSelectedItems(checkedListBox2);
                wifiProfile.UnicastCiphers = selectedItems;
            }
            else
            {
                wifiProfile.UnicastCiphers = string.Empty;
            }

            if (checkedListBox3.CheckedIndices.Count > 0)
            {
                selectedItems = GetAllSelectedItems(checkedListBox3);
                wifiProfile.GroupCiphers = selectedItems;
            }
            else
            {
                wifiProfile.GroupCiphers = string.Empty;
            }


            wifiProfile.WpaPresharedKey = textBox4.Text;
            wifiProfile.Wpa2PresharedKey = textBox5.Text;
            wifiProfile.SupplicantIdentity = textBox6.Text;
            wifiProfile.GroupKeyUpdate = textBox7.Text;

            if (comboBox4.SelectedItem != null)
            {
                wifiProfile.ManagementProtection = comboBox4.SelectedItem.ToString();
                if (comboBox4.SelectedItem.ToString() != "disabled")
                {
                    wifiProfile.ManagementProtectionKey = textBox8.Text;
                }
                else
                {
                    wifiProfile.ManagementProtectionKey = string.Empty;
                }
            }
            else
            {
                wifiProfile.ManagementProtection = string.Empty;
                wifiProfile.ManagementProtectionKey = string.Empty;
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

        #endregion

        # region Add/Edit/Delete WirelessSecProfile 
        private async Task CreateWifiSecurityProfile(WifiSecurityProfile profile)
        {
            try
            {
                // Construct the JSON payload for the new security profile

                JObject payload = new JObject
                {
                    ["name"] = profile.Name,
                    ["mode"] = profile.Mode,
                    ["authentication-types"] = profile.AuthenticationType,
                    ["unicast-ciphers"] = profile.UnicastCiphers,
                    ["group-ciphers"] = profile.GroupCiphers,
                    ["wpa-pre-shared-key"] = profile.WpaPresharedKey,
                    ["wpa2-pre-shared-key"] = profile.Wpa2PresharedKey,
                    ["supplicant-identity"] = profile.SupplicantIdentity,
                    ["group-key-update"] = profile.GroupKeyUpdate.ToString().ToLower()
                };

                // Check if ManagementProtection is not null before adding it to the payload
                if (profile.ManagementProtection != null)
                {
                    payload["management-protection"] = profile.ManagementProtection.ToString().ToLower();
                }
                else
                {
                    // Handle the case where ManagementProtection is null
                    // For example, set a default value or log a warning
                    payload["management-protection"] = "disabled";
                }

                payload["management-protection-key"] = profile.ManagementProtectionKey;

                await Controller.CreateWirelessSecurity(payload);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error creating security profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private async void button8_Click(object sender, EventArgs e)
        {

            if (!ValidateSelectedItems(checkedListBox1))
            {
                // Halt execution if validation fails
                return;
            }

            //verifica time e password lenght
            if (ValidateAndUpdateTimeFormat(textBox7.Text, 1) == "")
            {
                return;
            }

            if (!ValidateDynamicKeys())
            {
                return;
            }
            if (textBox3.Text == "")
            {
                MessageBox.Show("Please Select a Name.");
                return;
            }

            // Update wifiProfile object with values from the form before creating the security profile
            UpdateWifiProfileFromForm();

            // Create the security profile
            await CreateWifiSecurityProfile(wifiProfile);
        }


        #endregion

        #endregion































        private async void comboBoxInterfaces_Enter(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of bridges
                string response = await Controller.Retrieve("/rest/interface");
                List<string> intList = Parser.ParseNamesFromJsonArray(response, "default-name");
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
                List<string> bridgeList = Parser.ParseNamesFromJsonArray(response, "name");
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
                string response = await Controller.Retrieve("/rest/interface/bridge/port?interface=" + selectedInterface + "");
                JArray jsonArray = JArray.Parse(response);
                if (jsonArray.Count == 0)
                {
                    await Controller.CreatePortToBridgeConnection(selectedInterface, selectedBridge, horizonValue, learnOption, unknownUnicastFlood, broadcastFlood, hardwareOffload, unknownMulticastFlood, trusted, multicastRouter, fastLeave);

                    return;
                }
                List<string> list = Parser.ParseNamesFromJsonArray(response, ".id");
                string id = list[0].ToString();
                // Perform operations using the method controller
                await Controller.AssociateBridge(id, selectedBridge, horizonValue, learnOption, unknownUnicastFlood, broadcastFlood, hardwareOffload, unknownMulticastFlood, trusted, multicastRouter, fastLeave);

            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private WirelessSettings RetrieveWirelessSettings(string name)
        {
            try
            {
                // Make an HTTP GET request to the specified endpoint ("/rest/interface/wireless")
                HttpResponseMessage response = httpClient.GetAsync(baseUrl + "/rest/interface/wireless").Result;
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
                List<string> wirelessList = Parser.ParseNamesFromJsonArray(response, "name");

                // Clear existing items in the ComboBox
                WirelessInterfaceCombobox.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string wirelessName in wirelessList)
                {
                    WirelessInterfaceCombobox.Items.Add(wirelessName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Wireless Interface data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private async void WirelessInterfaceCombobox_IndexChanged(object sender, EventArgs e)
        {
            // Clear textboxes and comboboxes
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Text = "";
                }
                if (control is ComboBox)
                {
                    ((ComboBox)control).SelectedIndex = -1;
                }
            }

            string name = WirelessInterfaceCombobox.SelectedItem.ToString();
            WirelessSettings settings = RetrieveWirelessSettings(name);
            await PopulatecomboBoxSecProfile();

            comboBoxFrequency.Items.Clear();
            comboBoxWirelessBand.Items.Clear();
            comboBoxChannelWidth.Items.Clear();
            comboBoxChannelWidth.Items.AddRange(new string[]
            {
                "20mhz",
                "10mhz",
                "5mhz",
                "20/40mhz ec",
                "20/40mhz ce",
                "20/40mhz xx"
            });
            if (name == "wlan1")
            {
                // Populate with numbers in the range of 2412 to 2472
                for (int i = 2412; i <= 2472; i += 5)
                {
                    comboBoxFrequency.Items.Add(i.ToString());
                }
                comboBoxFrequency.Items.Add("auto");

                // Filter and add values starting with "2"
                foreach (string value in Parser.GetFilteredValues("2"))
                {
                    comboBoxWirelessBand.Items.Add(value);
                }

            }
            else if (name == "wlan2")
            {
                // Populate with numbers in the range of 5180 to 5315
                for (int i = 5180; i <= 5320; i += 5)
                {
                    comboBoxFrequency.Items.Add(i.ToString());
                }

                // Add numbers from 5500 to 5700 and "auto"
                for (int i = 5500; i <= 5700; i += 5)
                {
                    comboBoxFrequency.Items.Add(i.ToString());
                }
                comboBoxFrequency.Items.Add("auto");

                // Filter and add values starting with "5"
                foreach (string value in Parser.GetFilteredValues("5"))
                {
                    comboBoxWirelessBand.Items.Add(value);
                }
                comboBoxChannelWidth.Items.AddRange(new string[]
                {
                    "20/40/80mhz Ceee",
                    "20/40/80mhz eCee",
                    "20/40/80mhz eeCe",
                    "20/40/80mhz eeeC",
                    "20/40/80mhz xxxx"
                });
            }


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
                checkBox4.Checked = settings.disabled;

            }

        }

        private async void button10_Click(object sender, EventArgs e)
        {
            if (WirelessInterfaceCombobox.SelectedItem == null)
            {
                MessageBox.Show("Select a Wireless Interface: ");
                return;
            }
            string name = WirelessInterfaceCombobox.SelectedItem.ToString();
            WirelessSettings settings = RetrieveWirelessSettings(name);

            if (settings.disabled == true)
            {
                MessageBox.Show("Wireless Interface already disabled");
                return;
            }

            try
            {
                await Controller.DeactivateWirelessInterface(settings.Id);
                checkBox4.Checked = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Deactivating Wireless Interface: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void button11_Click(object sender, EventArgs e)
        {

            if (WirelessInterfaceCombobox.SelectedItem == null)
            {
                MessageBox.Show("Select a Wireless Interface: ");
                return;
            }
            string name = WirelessInterfaceCombobox.SelectedItem.ToString();
            WirelessSettings settings = RetrieveWirelessSettings(name);

            if (settings.disabled == false)
            {
                MessageBox.Show("Wireless Interface already enabled");
                return;
            }

            try
            {
                await Controller.ActivateWirelessInterface(settings.Id);
                checkBox4.Checked = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Activating Wireless Interface: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async void button20_Click(object sender, EventArgs e)
        {
            if (comboBoxInterfaces.SelectedItem == null)
            {
                MessageBox.Show("Select a interface");
                return;
            }
            string selectedInterface = comboBoxInterfaces.SelectedItem.ToString();

            try
            {
                string response = await Controller.Retrieve("/rest/interface/bridge/port");

                // Parse the JSON response into a JArray
                JArray portArray = JArray.Parse(response);

                // Iterate through each JSON object in the array
                foreach (JObject portObject in portArray)
                {
                    // Check if the "interface" property matches the selected interface name
                    if (portObject.TryGetValue("interface", out var interfaceToken) && interfaceToken.ToString() == selectedInterface)
                    {
                        // Retrieve the ID from the current JSON object
                        string selectedID = portObject[".id"].ToString();
                        await Controller.DeactivateBridge(selectedID);
                        // Do something with the selected ID
                        MessageBox.Show($"Removed bridge & port connection");
                        return;
                    }
                }


                // If no matching interface is found
                MessageBox.Show($"No matching interface found for {selectedInterface}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving data: {ex.Message}");
            }
        }

        private async void buttonListarRotas_Click(object sender, EventArgs e)
        {
            textBox15.Clear();
            try
            {
                string response = await Controller.Retrieve("/rest/ip/route");
                JArray routesArray = JArray.Parse(response);

                // Initialize a list to store route information
                List<string> routeList = new List<string>();

                // Iterate through each route object
                foreach (JObject routeObject in routesArray)
                {
                    // Extract destination address and gateway from the route object
                    string destinationAddress = routeObject["dst-address"].ToString();
                    string gateway = routeObject["gateway"].ToString();

                    // Combine destination address and gateway
                    string routeInfo = $"{destinationAddress} - {gateway}";

                    // Add route information to the list
                    routeList.Add(routeInfo);
                }

                // Display routes in the textbox
                textBox15.Text = routeList.Count > 0 ? string.Join(Environment.NewLine, routeList) : "No routes found.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving routes: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void button12_Click(object sender, EventArgs e)
        {
            if (WirelessInterfaceCombobox.SelectedItem == null)
            {
                MessageBox.Show("Select a Wireless Interface: ");
                return;
            }

            if (textBoxWirelessName.Text == "")
            {
                MessageBox.Show("Select a Wireless Interface Name. ");
                return;
            }

            if (textBoxWirelessMTU.Text == "" || !int.TryParse(textBoxWirelessMTU.Text, out int value) || value < 32 || value > 2290)
            {
                // Value is outside the range [32, 2290] or invalid input
                MessageBox.Show("Value of MTU is outside the range [32, 2290] or invalid input. ");
                return;
            }
            if (textBoxL2MTU.Text == "" || !int.TryParse(textBoxL2MTU.Text, out value) || value < 32 || value > 2290)
            {
                // Value is outside the range [32, 2290] or invalid input
                MessageBox.Show("Value of L2 MTU is outside the range [32, 2290] or invalid input. ");
                return;
            }

            string name = WirelessInterfaceCombobox.SelectedItem.ToString();
            WirelessSettings settings = RetrieveWirelessSettings(name);


            JObject payload = new JObject
            {
                ["name"] = textBoxWirelessName.Text,
                ["mtu"] = textBoxWirelessMTU.Text,
                ["l2mtu"] = textBoxL2MTU.Text,
                ["mac-address"] = textBoxMACAddr.Text,
                ["arp"] = comboBoxWirelessARP.SelectedItem.ToString(),
                ["mode"] = comboBoxWirelessMode.SelectedItem.ToString(),
                ["band"] = comboBoxWirelessBand.SelectedItem.ToString(),
                ["channel-width"] = comboBoxChannelWidth.SelectedItem.ToString().Replace(" ", "-"),
                ["frequency"] = comboBoxFrequency.SelectedItem.ToString(),
                ["ssid"] = textBoxSSID.Text,
                ["security-profile"] = comboBoxSecProfile.SelectedItem.ToString(),
                ["country"] = comboBoxCountryCodes.SelectedItem.ToString(),
                ["frequency-mode"] = comboBoxFreqMode.SelectedItem.ToString(),
                ["installation"] = comboBox14.SelectedItem.ToString(),
                ["default-authentication"] = checkBox1.Checked,
                ["arp-timeout"] = textBox14.Text
            };

            try
            {
                await Controller.ConfigureWirelessSettings(settings.Id, payload);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Configuring Wireless Interface: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private async void button9_Click(object sender, EventArgs e)
        {
            if (SecProfilesComboBox.SelectedItem == null)
            {
                MessageBox.Show("Select a Security Profile: ");
                return;
            }

            await EraseWifiSecProfile(wifiProfile.Id);

        }
        private async void comboBoxVRF_Enter(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of bridges
                string response = await Controller.Retrieve("/rest/interface");
                List<string> intList = Parser.ParseNamesFromJsonArray(response, "name");
                // Clear existing items in the ComboBox
                comboBoxVRF.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string intName in intList)
                {
                    comboBoxVRF.Items.Add(intName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving interfaces data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void button14_Click(object sender, EventArgs e)
        {
            // Get the destination address from the input
            string destAddress = textBoxDestAddress.Text.Trim();
            string gateway = textBoxGateway.Text.Trim();
            string checkGateway = comboBoxCheckGateway.SelectedItem?.ToString(); // Handle possible null value
            string distanceText = textBox18.Text.Trim();
            string scope = textBox17.Text.Trim();
            string targetScope = textBox19.Text.Trim();
            string vrfInterface = comboBoxVRF.SelectedItem?.ToString(); // Handle possible null value
            string routingTable = comboBox15.SelectedItem?.ToString(); // Handle possible null value
            string prefSource = textBoxPrefSource.Text.Trim();
            bool hw = checkboxHwOffload.Checked;

            // Check if all parameters are null
            if (string.IsNullOrEmpty(destAddress) && string.IsNullOrEmpty(gateway) &&
                string.IsNullOrEmpty(checkGateway) && string.IsNullOrEmpty(distanceText) &&
                string.IsNullOrEmpty(scope) && string.IsNullOrEmpty(targetScope) &&
                string.IsNullOrEmpty(vrfInterface) && string.IsNullOrEmpty(routingTable) &&
                string.IsNullOrEmpty(prefSource))
            {
                MessageBox.Show("Please provide at least one parameter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if a VRF interface is selected
            if (string.IsNullOrEmpty(vrfInterface))
            {
                MessageBox.Show("Please select a VRF interface.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validate the destination address
            if (Parser.IsValidIpAddress(destAddress) && Parser.IsValidIpAddressGateway(gateway))
            {
                // Call the method to create the route
                await Controller.CreateRoute(destAddress, gateway, checkGateway, distanceText, scope, targetScope, vrfInterface, routingTable, prefSource, hw);
            }
            else
            {
                // Destination address or gateway is not valid
                MessageBox.Show("Invalid destination address or gateway. Please enter valid IP addresses.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        private async void button15_Click(object sender, EventArgs e)
        {
            if (comboBox19.SelectedItem != null)
            {
                // Get the selected item from the ComboBox
                string selectedRouteInfo = comboBox19.SelectedItem.ToString();

                // Extract the route ID from the selected item
                string[] parts = selectedRouteInfo.Split('-');
                string routeId = parts[parts.Length - 1].Trim(); // Assuming the route ID is the last part
                await Controller.DeleteRoute(routeId);
                MessageBox.Show("Route Deleted");
            }
            else
            {
                // No item selected in the ComboBox
                MessageBox.Show("Please select a route to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private string TimeSpanToString(TimeSpan timeSpan)
        {
            string result = "";

            // Add hours if present
            if (timeSpan.TotalHours >= 1)
            {
                result += $"{(int)timeSpan.TotalHours}h";
                timeSpan -= TimeSpan.FromHours((int)timeSpan.TotalHours);
            }

            // Add minutes if present
            if (timeSpan.TotalMinutes >= 1)
            {
                result += $"{(int)timeSpan.TotalMinutes}m";
                timeSpan -= TimeSpan.FromMinutes((int)timeSpan.TotalMinutes);
            }

            // Add seconds if present
            if (timeSpan.TotalSeconds >= 1)
            {
                result += $"{(int)timeSpan.TotalSeconds}s";
            }

            return result.Trim();
        }

        private string ValidateAndUpdateTimeFormat(string inputTime, int mode)
        {
            //0 -> hh:mm:ss
            //1 -> sec profile
            //2 -> dhcp
            //3 -> bridge
            //4 -> hh:mm:ss above 0
            string pattern = "";
            string errorMessage = "Invalid time format. Please enter the time in the format 'hh:mm:ss'.";

            switch (mode)
            {
                case 0: // Change the default case label to case 0
                    pattern = @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$"; // Pattern for hh:mm:ss format
                    break;
                case 1:
                    errorMessage = "Invalid time format. Please enter the time in the format 'hh:mm:ss'," +
                        "in the range of [00:30:00, 24:00:00].";
                    pattern = @"^(?:00:(?:[3-5][0-9]|00):[0-5][0-9]|0[1-9]:[0-5][0-9]:[0-5][0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]|24:00:00$";
                    break;
                case 2:
                    errorMessage = "Invalid time format. Please enter the time in the format 'hh:mm:ss' " +
                        "where hh is between 00 and 23, mm and ss are between 00 and 59.";
                    pattern = @"^([0-9]|0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$";
                    break;
                case 3:
                    errorMessage = "Invalid time format. Please enter the time in the format 'hh:mm:ss'" +
                        " where hh is between 00 and 23, mm and ss are between 00 and 59.";
                    pattern = @"^(?:[01]\d|2[0-3]):[0-5]\d:[0-5]\d$";
                    break;
                case 4:
                    errorMessage = "Invalid time format. Please enter the time in the format 'hh:mm:ss', and above 0s.";
                    pattern = @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]:(0[1-9]|[1-5][0-9])$";
                    break;
                default:
                    MessageBox.Show(errorMessage);
                    return "";
            }

            if (Regex.IsMatch(inputTime, pattern))
            {
                TimeSpan timeSpan = TimeSpan.Parse(inputTime);
                return TimeSpanToString(timeSpan);
            }
            else
            {
                MessageBox.Show(errorMessage);
                return "";
            }
        }


        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = comboBox4.SelectedItem.ToString();

            // Enable/disable text boxes or checkboxes based on the selected item
            if (selectedItem != "disabled")
            {
                textBox8.Enabled = true;
            }
            else
            {
                textBox8.Enabled = false;
            }
        }

        private string GetAllSelectedItems(CheckedListBox checkedListBox)
        {
            string selectedItems = "";

            // Iterate through the CheckedItems collection
            foreach (var item in checkedListBox.CheckedItems)
            {
                // Convert to lowercase and replace spaces with hyphens
                string formattedItem = item.ToString().ToLower().Replace(" ", "-");

                // Append each selected item to the label
                selectedItems += formattedItem + ",";
            }

            selectedItems = selectedItems.TrimEnd(',');

            return selectedItems;
        }

        private bool ValidateSelectedItems(CheckedListBox checkedListBox)
        {
            bool wpaPskSelected = false;
            bool wpa2PskSelected = false;

            // Check if "WPA PSK" and "WPA2 PSK" are selected
            foreach (var item in checkedListBox.CheckedItems)
            {
                string selectedItem = item.ToString().ToLower();
                if (selectedItem == "wpa psk")
                {
                    wpaPskSelected = true;
                }
                else if (selectedItem == "wpa2 psk")
                {
                    wpa2PskSelected = true;
                }
            }

            // Check if both "WPA PSK" and "WPA2 PSK" are selected and if the password has more than 8 characters
            if (wpaPskSelected)
            {
                if (textBox4.Text.Length > 8 || textBox4.Text == "")
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("The password must have more than 8 characters for WPA PSK and not be empty.", "Password Length Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            if (wpa2PskSelected)
            {
                if (textBox5.Text.Length > 8 || textBox5.Text == "")
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("The password must have more than 8 characters for WPA2 PSK and not be empty.", "Password Length Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            if (SecProfilesComboBox.SelectedItem == null)
            {
                MessageBox.Show("Select a Security Profile: ");
                return;
            }

            if (!ValidateSelectedItems(checkedListBox1))
            {
                // Halt execution if validation fails
                return;
            }

            //verifica time e password lenght
            if (ValidateAndUpdateTimeFormat(textBox7.Text, 1) == "")
            {
                return;
            }

            if (!ValidateDynamicKeys())
            {
                return;
            }
            if (textBox3.Text == "")
            {
                MessageBox.Show("Please Select a Name.");
                return;
            }
            // Update wifiProfile object with values from the form before creating the security profile
            UpdateWifiProfileFromForm();

            // Create the security profile
            await EditWifiSecurityProfile(wifiProfile);

        }

        private async void SecProfilesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            // Clear textboxes and comboboxes
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Text = "";
                }
                if (control is ComboBox)
                {
                    ((ComboBox)control).SelectedIndex = -1;
                }
            }

            string name = SecProfilesComboBox.SelectedItem.ToString();
            WifiSecurityProfile secProfile = await RetrieveSecProfile(name);


            if (secProfile != null)
            {
                wifiProfile.Id = secProfile.Id;
                textBox3.Text = secProfile.Name;
                comboBox3.SelectedItem = secProfile.Mode.Replace("-", " ");
                FillcheckedListBox1(secProfile);
                FillcheckedListBox23(secProfile);
                textBox4.Text = secProfile.WpaPresharedKey;
                textBox5.Text = secProfile.Wpa2PresharedKey;
                textBox6.Text = secProfile.SupplicantIdentity;
                textBox7.Text = ConvertTimeFormat(secProfile.GroupKeyUpdate);
                comboBox4.SelectedItem = secProfile.ManagementProtection;
                textBox8.Text = secProfile.ManagementProtectionKey;
            }

        }

        private void FillcheckedListBox1(WifiSecurityProfile secProfile)
        {
            string authenticationTypes = secProfile.AuthenticationType;

            // Split the string into individual authentication types and convert them to upper case
            string[] types = authenticationTypes.Split(',').Select(type => type.ToUpper()).ToArray();

            // Iterate through each authentication type
            foreach (string type in types)
            {
                // Replace "-" with " "
                string formattedType = type.Replace("-", " ");

                // Check if the formatted type exists in the CheckedListBox
                int index = checkedListBox1.Items.IndexOf(formattedType);
                if (index != -1)
                {
                    // If it does, select it in the CheckedListBox
                    checkedListBox1.SetItemChecked(index, true);
                }
            }

        }

        private void FillcheckedListBox23(WifiSecurityProfile secProfile)
        {
            string unicast = secProfile.UnicastCiphers;
            string groupcast = secProfile.GroupCiphers;

            // Split the string into individual authentication types and convert them to upper case
            string[] unicastcypher = unicast.Split(',').Select(type => type.ToLower()).ToArray();
            string[] groupcastcypher = groupcast.Split(',').Select(type => type.ToLower()).ToArray();

            // Iterate through each authentication type
            foreach (string type in unicastcypher)
            {
                // Replace "-" with " "
                string formattedType = type.Replace("-", " ");

                // Check if the formatted type exists in the CheckedListBox
                int index = checkedListBox2.Items.IndexOf(formattedType);
                if (index != -1)
                {
                    // If it does, select it in the CheckedListBox
                    checkedListBox2.SetItemChecked(index, true);
                }
            }

            foreach (string type in groupcastcypher)
            {
                // Replace "-" with " "
                string formattedType = type.Replace("-", " ");

                // Check if the formatted type exists in the ComboBox or CheckedListBox
                int index = checkedListBox3.Items.IndexOf(formattedType);
                if (index != -1)
                {
                    // If it does, select it in the CheckedListBox
                    checkedListBox3.SetItemChecked(index, true);
                }
            }

        }

        private async void SecProfilesComboBox_Enter(object sender, EventArgs e)
        {
            try
            {

                // Retrieve the list of wirelessInterfaces
                string response = await Controller.Retrieve("/rest/interface/wireless/security-profiles");
                List<string> wifiSecList = Parser.ParseNamesFromJsonArray(response, "name");

                // Clear existing items in the ComboBox
                SecProfilesComboBox.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string secProfile in wifiSecList)
                {
                    SecProfilesComboBox.Items.Add(secProfile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Security Profiles data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<WifiSecurityProfile> RetrieveSecProfile(string name)
        {
            try
            {
                // Make an HTTP GET request to the specified endpoint
                HttpResponseMessage response = await httpClient.GetAsync(baseUrl + $"/rest/interface/wireless/security-profiles/{name}");
                response.EnsureSuccessStatusCode(); // Throw an exception if the response is not successful

                // Read the response content as a string
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response into a WifiSecurityProfile object
                WifiSecurityProfile secProfile = JsonConvert.DeserializeObject<WifiSecurityProfile>(responseBody);

                return secProfile;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async Task EditWifiSecurityProfile(WifiSecurityProfile profile)
        {
            try
            {
                // Construct the JSON payload for the new security profile

                JObject payload = new JObject
                {
                    ["name"] = profile.Name,
                    ["mode"] = profile.Mode,
                    ["authentication-types"] = profile.AuthenticationType,
                    ["unicast-ciphers"] = profile.UnicastCiphers,
                    ["group-ciphers"] = profile.GroupCiphers,
                    ["wpa-pre-shared-key"] = profile.WpaPresharedKey,
                    ["wpa2-pre-shared-key"] = profile.Wpa2PresharedKey,
                    ["supplicant-identity"] = profile.SupplicantIdentity,
                    ["group-key-update"] = profile.GroupKeyUpdate.ToString().ToLower()
                };

                // Check if ManagementProtection is not null before adding it to the payload
                if (profile.ManagementProtection != null)
                {
                    payload["management-protection"] = profile.ManagementProtection.ToString().ToLower();
                }
                else
                {
                    // Handle the case where ManagementProtection is null
                    // For example, set a default value or log a warning
                    payload["management-protection"] = "disabled";
                }

                payload["management-protection-key"] = profile.ManagementProtectionKey;

                await Controller.EditWirelessSecurity(payload, profile.Id);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Editing security profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateDynamicKeys()
        {
            string selectedItem = comboBox3.SelectedItem.ToString();
            if (selectedItem == "dynamic keys")
            {
                if (checkedListBox2.CheckedIndices.Count < 0 || checkedListBox3.CheckedIndices.Count < 0 || checkedListBox1.CheckedIndices.Count < 0)
                {
                    MessageBox.Show("Select at least 1 Authentication Type, 1 Unicast Cipher and 1 Group Cipher.");
                    return false;
                }
            }
            return true;
        }

        private string ConvertTimeFormat(string duration)
        {
            // Define a regular expression pattern to match the duration format
            string pattern = @"(?:(?<hours>\d+)h)?(?:(?<minutes>\d+)m)?(?:(?<seconds>\d+)s)?";

            // Match the input string against the pattern
            Match match = Regex.Match(duration, pattern);

            // Check if the input string matches the expected format
            if (match.Success)
            {
                // Extract hours, minutes, and seconds from the match
                int hours = match.Groups["hours"].Success ? int.Parse(match.Groups["hours"].Value) : 0;
                int minutes = match.Groups["minutes"].Success ? int.Parse(match.Groups["minutes"].Value) : 0;
                int seconds = match.Groups["seconds"].Success ? int.Parse(match.Groups["seconds"].Value) : 0;

                // Format the time as "hh:mm:ss"
                return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            }
            else
            {
                // Return an empty string if the input format is invalid
                return string.Empty;
            }
        }

        private async Task EraseWifiSecProfile(string id)
        {
            try
            {
                await Controller.DeleteSecProfile(id);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Editing security profile: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void comboBox19_Enter_1(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of routes
                string response = await Controller.Retrieve("/rest/ip/route");

                // Parse the JSON response into a JArray
                JArray routesArray = JArray.Parse(response);

                // Clear existing items in the ComboBox
                comboBox19.Items.Clear();

                // Iterate over each route object in the array
                foreach (JObject routeObject in routesArray)
                {
                    // Extract the route ID and local address from each route object
                    string routeId = routeObject.Value<string>(".id");
                    string localAddress = routeObject.Value<string>("dst-address");
                    string gateway = routeObject.Value<string>("gateway");

                    // Combine route ID and local address into a single string
                    string routeInfo = $"{localAddress}-{gateway}-{routeId}";

                    // Add the combined string as an item in the ComboBox
                    comboBox19.Items.Add(routeInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving routes data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button13_Click_1(object sender, EventArgs e)
        {
            {
                // Check if an item is selected in the ComboBox
                if (comboBox19.SelectedItem != null)
                {
                    // Get the selected item from the ComboBox
                    string selectedRouteInfo = comboBox19.SelectedItem.ToString();

                    // Extract the route ID from the selected item
                    string[] parts = selectedRouteInfo.Split('-');
                    string routeId = parts[parts.Length - 1].Trim(); // Assuming the route ID is the last part
                                                                     // Get the destination address from the input
                    string destAddress = textBoxDestAddress.Text.Trim();
                    string gateway = textBoxGateway.Text.Trim();
                    string checkGateway = comboBoxCheckGateway.SelectedItem?.ToString(); // Handle possible null value
                    string distanceText = textBox18.Text.Trim();
                    string scope = textBox17.Text.Trim();
                    string targetScope = textBox19.Text.Trim();
                    string vrfInterface = comboBoxVRF.SelectedItem?.ToString(); // Handle possible null value
                    string routingTable = comboBox15.SelectedItem?.ToString(); // Handle possible null value
                    string prefSource = textBoxPrefSource.Text.Trim();
                    bool hw = checkboxHwOffload.Checked;

                    // Check if all parameters are null
                    if (string.IsNullOrEmpty(destAddress) && string.IsNullOrEmpty(gateway) &&
                        string.IsNullOrEmpty(checkGateway) && string.IsNullOrEmpty(distanceText) &&
                        string.IsNullOrEmpty(scope) && string.IsNullOrEmpty(targetScope) &&
                        string.IsNullOrEmpty(vrfInterface) && string.IsNullOrEmpty(routingTable) &&
                        string.IsNullOrEmpty(prefSource))
                    {
                        MessageBox.Show("Please provide at least one parameter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Check if a VRF interface is selected
                    if (string.IsNullOrEmpty(vrfInterface))
                    {
                        MessageBox.Show("Please select a VRF interface.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Validate the destination address
                    if (Parser.IsValidIpAddress(destAddress) && Parser.IsValidIpAddressGateway(gateway))
                    {
                        // Call the method to create the route
                        await Controller.UpdateRoute(routeId, destAddress, gateway, checkGateway, distanceText, scope, targetScope, vrfInterface, routingTable, prefSource, hw);
                    }
                    else
                    {
                        // Destination address or gateway is not valid
                        MessageBox.Show("Invalid destination address or gateway. Please enter valid IP addresses.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }


                }
                else
                {
                    // No item selected in the ComboBox
                    MessageBox.Show("Please select a route to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void buttonListarEndIp_Click(object sender, EventArgs e)
        {
            textBoxListarEndIp.Clear();
            try
            {
                string response = await Controller.Retrieve("/rest/ip/address");
                JArray routesArray = JArray.Parse(response);

                // Initialize a list to store route information
                List<string> routeList = new List<string>();

                // Iterate through each route object
                foreach (JObject routeObject in routesArray)
                {
                    // Extract destination address and gateway from the route object
                    string address = routeObject["address"].ToString();
                    string network = routeObject["network"].ToString();
                    string inter = routeObject["interface"].ToString();


                    // Combine destination address and gateway
                    string routeInfo = $"{address} - {network} - {inter}";

                    // Add route information to the list
                    routeList.Add(routeInfo);
                }

                // Display routes in the textbox
                textBoxListarEndIp.Text = routeList.Count > 0 ? string.Join(Environment.NewLine, routeList) : "No addresses found.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving addresses: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button19_Click(object sender, EventArgs e)
        {
            if (comboBoxInterface.SelectedItem == null)
            {
                MessageBox.Show("Select a Interface");
            }
            if (string.IsNullOrEmpty(textBoxEnderecoIP.Text) || string.IsNullOrEmpty(textBoxNetwork.Text) || Parser.IsValidIpAddress(textBoxEnderecoIP.Text.Trim()) || Parser.IsValidIpAddressGateway(textBoxNetwork.Text))
            {
                MessageBox.Show("Fill all Addresses with a valid ip ");
            }
            string address = textBoxEnderecoIP.Text.Trim();
            string network = textBoxNetwork.Text.Trim();
            string inter = comboBoxInterface.SelectedItem.ToString();
            await Controller.CreateIp(address, network, inter);

        }

        private async void comboBoxInterface_Enter(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of bridges
                string response = await Controller.Retrieve("/rest/interface");
                List<string> intList = Parser.ParseNamesFromJsonArray(response, "name");
                // Clear existing items in the ComboBox
                comboBoxInterface.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string intName in intList)
                {
                    comboBoxInterface.Items.Add(intName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving interfaces data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<DHCPServer> RetrieveDHCPServer(string name)
        {
            try
            {
                // Make an HTTP GET request to the specified endpoint
                HttpResponseMessage response = await httpClient.GetAsync(baseUrl + $"/rest/ip/dhcp-server/{name}");
                response.EnsureSuccessStatusCode(); // Throw an exception if the response is not successful

                // Read the response content as a string
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response into a WifiSecurityProfile object
                DHCPServer dhcpServer = JsonConvert.DeserializeObject<DHCPServer>(responseBody);

                return dhcpServer;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }


        private async void buttonListDHCP_Click(object sender, EventArgs e)
        {
            textBoxListarServidoresDhcp.Clear();
            try
            {
                string response = await Controller.Retrieve("/rest/ip/dhcp-server");
                JArray routesArray = JArray.Parse(response);

                // Initialize a list to store route information
                List<string> dhcpList = new List<string>();

                // Iterate through each route object
                foreach (JObject routeObject in routesArray)
                {
                    // Extract destination address and gateway from the route object
                    string name = routeObject["name"].ToString();
                    string inter = routeObject["interface"].ToString();
                    string addresspool = routeObject["address-pool"].ToString();
                    string disabledValue = (bool)routeObject["disabled"] ? "disabled" : "enabled";


                    // Combine destination address and gateway
                    string dhcpInfo = $"{name} - {inter} - {addresspool} - {disabledValue}";

                    // Add route information to the list
                    dhcpList.Add(dhcpInfo);
                }

                // Display routes in the textbox
                textBoxListarServidoresDhcp.Text = dhcpList.Count > 0 ? string.Join(Environment.NewLine, dhcpList) : "No DHCP Servers found.";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving addresses: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void comboBoxServidorDHCP_Enter(object sender, EventArgs e)
        {
            try
            {

                // Retrieve the list of wirelessInterfaces
                string response = await Controller.Retrieve("/rest/ip/dhcp-server");
                List<string> dhcpList = Parser.ParseNamesFromJsonArray(response, "name");

                // Clear existing items in the ComboBox
                comboBoxServidorDHCP.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string dhcp in dhcpList)
                {
                    comboBoxServidorDHCP.Items.Add(dhcp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving DHCP Servers data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private async void buttonServerDHCPApagar_Click(object sender, EventArgs e)
        {
            if (comboBoxServidorDHCP.SelectedItem == null)
            {
                MessageBox.Show("Select a DHCP Server: ");
                return;
            }

            await EraseDHCPServer(dhcpServer.Id);

        }

        private async Task EraseDHCPServer(string id)
        {
            try
            {
                await Controller.DeleteDHCPServer(id);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error Deleting DHCP Server: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void comboBoxServidorDHCP_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Text = "";
                }
                if (control is ComboBox)
                {
                    ((ComboBox)control).SelectedIndex = -1;
                }
            }

            string name = comboBoxServidorDHCP.SelectedItem.ToString();
            DHCPServer dhcpServ = await RetrieveDHCPServer(name);
            await PopulatecomboBoxAddressPool();
            await PopulatecomboBoxDHCPInterfaces();


            if (dhcpServ != null)
            {
                dhcpServer.Id = dhcpServ.Id;
                textBoxDHCPName.Text = dhcpServ.Name;
                comboBoxAddressPool.SelectedItem = dhcpServ.AddressPool;
                comboBoxRadius.SelectedItem = dhcpServ.UseRadius;
                comboBox16.SelectedItem = dhcpServ.Interface;
                textBoxLeaseTime.Text = ConvertTimeFormat(dhcpServ.LeaseTime);
                checkBoxConflitDetetion.Checked = dhcpServ.ConflictDetection;
                checkBoxClassless.Checked = dhcpServ.UseFramedAsClassless;
            }

        }

        private async Task PopulatecomboBoxDHCPInterfaces()
        {
            // Clear any existing items in the ComboBox
            comboBox16.Items.Clear();

            try
            {
                // Retrieve the list of wirelessInterfaces
                string response = await Controller.Retrieve("/rest/interface");
                List<string> InterfaceList = Parser.ParseNamesFromJsonArray(response, "name");

                // Clear existing items in the ComboBox
                comboBox16.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string inter in InterfaceList)
                {
                    comboBox16.Items.Add(inter.ToLower());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Interface data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task PopulatecomboBoxAddressPool()
        {
            // Clear any existing items in the ComboBox
            comboBoxAddressPool.Items.Clear();

            try
            {
                // Retrieve the list of wirelessInterfaces
                string response = await Controller.Retrieve("/rest/ip/pool");
                List<string> AddressPoolList = Parser.ParseNamesFromJsonArray(response, "name");

                // Clear existing items in the ComboBox
                comboBoxAddressPool.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string AddressPool in AddressPoolList)
                {
                    comboBoxAddressPool.Items.Add(AddressPool);
                }
                string staticon = "static-only";
                comboBoxAddressPool.Items.Add(staticon.ToLower());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Adress Pool data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void button21_Click(object sender, EventArgs e)
        {
            //verifica time e password lenght
            if (ValidateAndUpdateTimeFormat(textBoxLeaseTime.Text, 2) == "")
            {
                return;
            }

            if (await ValidateDHCPInterface())
            {
                return;
            }

            if (textBoxDHCPName.Text == "")
            {
                MessageBox.Show("Please select a Name");
                return;
            }

            // Update wifiProfile object with values from the form before creating the security profile
            UpdateDHCPServerFromForm();

            // Create the security profile
            await CreateDHCPServer(dhcpServer);


        }

        private async Task<bool> ValidateDHCPInterface()
        {
            // Retrieve the list of wirelessInterfaces
            string response = await Controller.Retrieve("/rest/interface");
            string targetName = comboBox16.SelectedItem.ToString();

            JArray jsonArray = JArray.Parse(response);

            foreach (JObject obj in jsonArray)
            {
                // Access the "name" field
                string name = (string)obj["name"];

                // Check if the "name" matches the targetName
                if (name == targetName)
                {
                    // If the names match, check if it is a slave interface
                    if (obj.ContainsKey("slave") && (bool)obj["slave"] == true)
                    {
                        MessageBox.Show("The Interface is of Type slave, please chose another one.", "Password Length Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return true;
                    }

                }
            }
            return false;
        }

        private void UpdateDHCPServerFromForm()
        {
            // Update wifiProfile object with values from the form controls
            dhcpServer.Name = textBoxDHCPName.Text;
            dhcpServer.AddressPool = comboBoxAddressPool.SelectedItem.ToString();
            dhcpServer.UseRadius = comboBoxRadius.SelectedItem.ToString();
            dhcpServer.Interface = comboBox16.SelectedItem.ToString();
            dhcpServer.LeaseTime = textBoxLeaseTime.Text;
            dhcpServer.ConflictDetection = checkBoxConflitDetetion.Checked;
            dhcpServer.UseFramedAsClassless = checkBoxClassless.Checked;

        }

        private async Task CreateDHCPServer(DHCPServer dhcpServer)
        {
            try
            {
                // Construct the JSON payload for the new security profile

                JObject payload = new JObject
                {
                    ["name"] = dhcpServer.Name,
                    ["address-pool"] = dhcpServer.AddressPool,
                    ["conflict-detection"] = dhcpServer.ConflictDetection,
                    ["use-framed-as-classless"] = dhcpServer.UseFramedAsClassless,
                    ["interface"] = dhcpServer.Interface,
                    ["lease-time"] = dhcpServer.LeaseTime,
                    ["use-radius"] = dhcpServer.UseRadius
                };

                await Controller.CreateDHCPServer(payload);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error creating DHCP Server: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonServerDHCPEditar_Click(object sender, EventArgs e)
        {
            if (comboBoxServidorDHCP.SelectedItem == null)
            {
                MessageBox.Show("Select a DHCP Server: ");
                return;
            }

            //verifica time
            if (ValidateAndUpdateTimeFormat(textBoxLeaseTime.Text, 2) == "")
            {
                return;
            }

            if (await ValidateDHCPInterface())
            {
                return;
            }

            if (textBoxDHCPName.Text == "")
            {
                MessageBox.Show("Please select a Name");
                return;
            }

            // Update wifiProfile object with values from the form before creating the security profile
            UpdateDHCPServerFromForm();

            // Create the security profile
            await EditDHCPServer(dhcpServer);

        }

        private async Task EditDHCPServer(DHCPServer dhcpServer)
        {
            try
            {
                // Construct the JSON payload for the new security profile
                /*
                JObject payload = new JObject
                {
                    ["name"] = dhcpServer.Name,
                    ["address-pool"] = dhcpServer.AddressPool,
                    ["conflict-detection"] = dhcpServer.ConflictDetection,
                    ["use-framed-as-classless"] = dhcpServer.UseFramedAsClassless,
                    ["interface"] = dhcpServer.Interface,
                    ["lease-time"] = dhcpServer.LeaseTime,
                    ["use-radius"] = dhcpServer.UseRadius
                };
                */
                JObject payload = dhcpServer.ToJObject();
                payload.Remove(".id");
                payload.Remove("disabled");

                await Controller.EditDHCPServer(payload, dhcpServer.Id);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error editing DHCP Server: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void PopulateDNSTab()
        {
            try
            {
                // Make an HTTP GET request to the specified endpoint
                HttpResponseMessage response = await httpClient.GetAsync(baseUrl + $"/rest/ip/dns");
                response.EnsureSuccessStatusCode(); // Throw an exception if the response is not successful

                // Read the response content as a string
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response into a DNS object
                DNS dns = JsonConvert.DeserializeObject<DNS>(responseBody);

                //Populate fields
                // Split the servers string by comma
                string[] serverList = dns.Servers.Split(',');

                // Populate the textbox with servers
                textBoxServers.Clear();
                if (dns.Servers == "" && dns.AllowRemoteRequests == false)
                {
                    checkBox3.Checked = false;
                }
                checkBox3.Checked = true;
                foreach (string server in serverList)
                {
                    textBoxServers.Text += server + Environment.NewLine;
                }
                textBoxUDPPackageSize.Text = dns.MaxUdpPacketSize;
                textBoxQueryServerTimeout.Text = ConvertTimeFormat(dns.QueryServerTimeout);
                textBoxQueryTotalTimeout.Text = ConvertTimeFormat(dns.QueryTotalTimeout);
                textBoxConcurrentQueries.Text = dns.MaxConcurrentQueries;
                textBoxConcurrentTCPSessions.Text = dns.MaxConcurrentTcpSessions;
                textBox20.Text = dns.CacheSize;
                textBoxCacheMaxTTL.Text = ConvertTimeFormat(dns.CacheMaxTTL);
                textBox11.Text = dns.CacheUsed;
                checkBoxRemoteRequests.Checked = dns.AllowRemoteRequests;

            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void comboBoxChannelWidth_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void buttonDNSAtivar_Click(object sender, EventArgs e)
        {
            try
            {
                await Controller.ActivateDNS();
                PopulateDNSTab();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Activating DNS: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void buttonDNSDeactivate_Click(object sender, EventArgs e)
        {
            try
            {
                await Controller.DeactivateDNS();
                PopulateDNSTab();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Deactivating DNS: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonDNSConfigurar_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Parser.ValidateIpAddress(textBoxServers.Text))
                {
                    return;
                }
                if (textBoxUDPPackageSize.Text == "" || !int.TryParse(textBoxUDPPackageSize.Text, out int value) || value < 50 || value > 65507)
                {
                    MessageBox.Show("Value of UDP Package Size is outside the range [50, 65507] or invalid input. ");
                }
                if (ValidateAndUpdateTimeFormat(textBoxQueryServerTimeout.Text, 4) == "")
                {
                    return;
                }
                if (ValidateAndUpdateTimeFormat(textBoxQueryTotalTimeout.Text, 4) == "")
                {
                    return;
                }
                if (textBox20.Text == "" || !int.TryParse(textBoxUDPPackageSize.Text, out value) || value < 64)
                {
                    MessageBox.Show("Value of Cache Size is inferior to 63 or invalid input. ");
                }
                /*if (ValidateAndUpdateTimeFormat(textBoxCacheMaxTTL.Text, 0) == "")
                {
                    return;
                }*/

                UpdateDNSFromForm();
                await EditDNS(dns);
                PopulateDNSTab();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Editing Wireless Interface: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private async Task EditDNS(DNS dns)
        {
            try
            {

                JObject payload = dns.ToJObject();
                payload.Remove("cache-used");

                await Controller.EditDNS(payload);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show("Error editing DNS " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void UpdateDNSFromForm()
        {
            dns.Servers = textBoxServers.Text.Replace("\r\n", ",");
            dns.MaxUdpPacketSize = textBoxUDPPackageSize.Text;
            dns.QueryServerTimeout = textBoxQueryServerTimeout.Text;
            dns.QueryTotalTimeout = textBoxQueryTotalTimeout.Text;
            dns.MaxConcurrentQueries = textBoxConcurrentQueries.Text;
            dns.MaxConcurrentTcpSessions = textBoxConcurrentTCPSessions.Text;
            dns.CacheSize = textBox20.Text;
            dns.CacheMaxTTL = textBoxCacheMaxTTL.Text;
            dns.AllowRemoteRequests = checkBoxRemoteRequests.Checked;

        }

        

        private void comboBoxDNSEntry_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear textboxes and comboboxes
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Text = "";
                }
                if (control is ComboBox)
                {
                    ((ComboBox)control).SelectedIndex = -1;
                }
            }
            PopulateDNSStatic();
        }

        private void PopulateDNSStatic()
        {
            string selectedItem = comboBoxDNSEntry.SelectedItem.ToString();
            int index = selectedItem.IndexOf('-');
            string name = "";
            string id = "";

            if (index != -1)
            {
                // Extract the ID and Name
                id = selectedItem.Substring(0, index).Trim();
                name = selectedItem.Substring(index + 1).Trim();

            }
            else
            {
                // Handle the case where '-' is not found
            }
            DNSStatic dnsStaticLocal = RetrieveStaticDNS(id);

            if (dnsStaticLocal != null)
            {
                dnsStatic.Id = dnsStaticLocal.Id;
                textBoxDNSName.Text = dnsStaticLocal.Name;
                if (dnsStaticLocal.Type != "CNAME")
                {
                    textBox21.Enabled = false;
                }
                else
                {
                    textBox21.Enabled = true;
                }
                if (dnsStaticLocal.Type == null)
                {
                    comboBoxDNSType.SelectedItem = "A";
                }
                else
                {
                    comboBoxDNSType.SelectedItem = dnsStaticLocal.Type;
                }

                textBoxDNSTTL.Text = ConvertTimeFormat(dnsStaticLocal.TTL);
                textBoxDNSAddressList.Text = dnsStaticLocal.AddressList;
                textBoxDNSAddress.Text = dnsStaticLocal.Address;
                checkBox2.Checked = !dnsStaticLocal.Disabled;

                if (dnsStaticLocal.MatchSubdomain == null)
                {
                    checkBoxDNSMatchSubdomain.Checked = false;
                }
                else
                {
                    checkBoxDNSMatchSubdomain.Checked = (bool)dnsStaticLocal.MatchSubdomain;
                }
            }
        }

        private async void comboBoxDNSEntry_Enter(object sender, EventArgs e)
        {
            try
            {

                // Retrieve the list of DNS Static
                string response = await Controller.Retrieve("/rest/ip/dns/static");
                List<(string id, string name)> dnsStaticList = Parser.ParseIdNameFromJsonArray(response, ".id", "name");

                // Clear existing items in the ComboBox
                comboBoxDNSEntry.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (var dnsStatic in dnsStaticList)
                {
                    comboBoxDNSEntry.Items.Add($"{dnsStatic.id} - {dnsStatic.name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Static DNS data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private DNSStatic RetrieveStaticDNS(string id)
        {
            try
            {
                // Make an HTTP GET request to the specified endpoint ("/rest/dns/static")
                HttpResponseMessage response = httpClient.GetAsync(baseUrl + "/rest/ip/dns/static").Result;
                response.EnsureSuccessStatusCode(); // Throw an exception if the response is not successful

                // Read the response content as a string
                string responseBody = response.Content.ReadAsStringAsync().Result;

                // Deserialize the JSON response into a list of WirelessSettings objects
                List<DNSStatic> dnsStaticList = JsonConvert.DeserializeObject<List<DNSStatic>>(responseBody);

                // Find the WirelessSettings object with the matching name
                return dnsStaticList.FirstOrDefault(s => s.Id == id);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async void buttonDNSStaticActivate_Click(object sender, EventArgs e)
        {
            try
            {
                await Controller.ActivateDNSStatic(dnsStatic.Id, comboBoxDNSEntry.Text);
                PopulateDNSStatic();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Enabling Static DNS: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void buttonDNSStaticDeactivate_Click(object sender, EventArgs e)
        {
            try
            {
                await Controller.DeactivateDNSStatic(dnsStatic.Id, comboBoxDNSEntry.Text);
                PopulateDNSStatic();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Disabling Static DNS: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonDNSStaticRemove_Click(object sender, EventArgs e)
        {
            try
            {
                await Controller.RemoveDNSStatic(dnsStatic.Id, comboBoxDNSEntry.Text);
                PopulateDNSStatic();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Removing Static DNS: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void buttonDNSStaticAdd_Click(object sender, EventArgs e)
        {
            try
            {
                string type = comboBoxDNSType.Text;
                if (checkBoxDNSMatchSubdomain.Checked && textBoxDNSName.Text == "")
                {
                    MessageBox.Show("Name Required for Match Subdomain.");
                    return;
                }
                switch (type)
                {
                    case "A":
                        if (!Parser.IsValidIpAddressGateway(textBoxDNSAddress.Text))
                        {
                            MessageBox.Show($"Invalid IPv4 Address: {textBoxDNSAddress.Text}");
                            return;
                        }
                        break;
                    case "AAAA":
                        if (!Parser.ValidateIPv6(textBoxDNSAddress.Text))
                        {
                            MessageBox.Show($"Invalid IPv6 Address: {textBoxDNSAddress.Text}");
                            return;
                        }
                        break;
                    case "CNAME":
                        if (!Parser.ValidateCNAME(textBoxDNSAddress.Text))
                        {
                            MessageBox.Show($"Invalid CNAME: {textBoxDNSAddress.Text}");
                            return;
                        }
                        break;
                }
                if (ValidateAndUpdateTimeFormat(textBoxDNSTTL.Text, 4) == "")
                {
                    return;
                }
                UpdateDNSStaticFromForm();
                await CreateStaticDNS();
                PopulateDNSStatic();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Creating Static DNS {textBoxDNSName.Text}: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateDNSStaticFromForm()
        {
            dnsStatic.Address = textBoxDNSAddress.Text;
            dnsStatic.AddressList = textBoxDNSAddressList.Text;
            dnsStatic.Disabled = false;
            dnsStatic.MatchSubdomain = checkBoxDNSMatchSubdomain.Checked;
            dnsStatic.Name = textBoxDNSName.Text;
            dnsStatic.TTL = textBoxDNSTTL.Text;
            dnsStatic.Type = comboBoxDNSType.SelectedItem.ToString();
        }

        private async Task CreateStaticDNS()
        {
            try
            {

                JObject payload = dnsStatic.ToJObject();

                await Controller.CreateStaticDNS(payload);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Creating Static DNS Entry {dnsStatic.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonDNSStaticEdit_Click(object sender, EventArgs e)
        {
            try
            {
                string type = comboBoxDNSType.Text;
                if (checkBoxDNSMatchSubdomain.Checked && textBoxDNSName.Text == "")
                {
                    MessageBox.Show("Name Required for Match Subdomain.");
                    return;
                }
                switch (type)
                {
                    case "A":
                        if (!Parser.IsValidIpAddressGateway(textBoxDNSAddress.Text))
                        {
                            MessageBox.Show($"Invalid IPv4 Address: {textBoxDNSAddress.Text}");
                            return;
                        }
                        break;
                    case "AAAA":
                        if (!Parser.ValidateIPv6(textBoxDNSAddress.Text))
                        {
                            MessageBox.Show($"Invalid IPv6 Address: {textBoxDNSAddress.Text}");
                            return;
                        }
                        break;
                    case "CNAME":
                        if (!Parser.ValidateCNAME(textBoxDNSAddress.Text))
                        {
                            MessageBox.Show($"Invalid CNAME: {textBoxDNSAddress.Text}");
                            return;
                        }
                        break;
                }
                if (ValidateAndUpdateTimeFormat(textBoxDNSTTL.Text, 4) == "")
                {
                    return;
                }
                UpdateDNSStaticFromForm();
                await EditStaticDNS();
                //PopulateDNSStatic();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Editing Static DNS {dnsStatic.Id}-{textBoxDNSName.Text}: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private async Task EditStaticDNS()
        {
            try
            {

                JObject payload = dnsStatic.ToJObject();

                await Controller.EditStaticDNS(payload);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Creating Static DNS Entry {dnsStatic.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void comboBoxWireguardInterface_Enter(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of WG Interfaces
                string response = await Controller.Retrieve("/rest/interface/wireguard");
                List<string> WGInterface = Parser.ParseNamesFromJsonArray(response, "name");

                // Clear existing items in the ComboBox
                comboBoxWireguardInterface.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string wg in WGInterface)
                {
                    comboBoxWireguardInterface.Items.Add(wg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Wireguard Interfaces data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void comboBoxWireguardInterface_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear textboxes and comboboxes
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Text = "";
                }
                if (control is ComboBox)
                {
                    ((ComboBox)control).SelectedIndex = -1;
                }
            }
            PopulateWGInterface();

        }

        private void PopulateWGInterface()
        {
            string name = comboBoxWireguardInterface.SelectedItem.ToString();

            WireguardInterface wgInt = RetrieveWGInt(name);

            wgInterface.Id = wgInt.Id;
            wgInterface.Name = wgInt.Name;
            textBoxWireguardInterface.Text = wgInt.Name;
            textBoxWireguardListenPort.Text = wgInt.ListenPort;
            textBox16.Text = wgInt.PrivateKey;
            textBoxWireguardPublicKey.Text = wgInt.PublicKey;
            checkBoxRunning.Checked = wgInt.Running;
            checkBoxWGActivate.Checked = !wgInt.Disabled;
        }

        private WireguardInterface RetrieveWGInt(string name)
        {
            try
            {
                // Make an HTTP GET request to the specified endpoint ("/rest/interface/wireguard/{name}")
                HttpResponseMessage response = httpClient.GetAsync(baseUrl + $"/rest/interface/wireguard/{name}").Result;
                response.EnsureSuccessStatusCode(); // Throw an exception if the response is not successful

                // Read the response content as a string
                string responseBody = response.Content.ReadAsStringAsync().Result;

                // Deserialize the JSON response into a list of WGInt objects
                WireguardInterface wgInt = JsonConvert.DeserializeObject<WireguardInterface>(responseBody);

                // Find the WireguardInterface object with the matching name
                return wgInt;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async void buttonWireguardCreateInterface_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBoxWireguardInterface.Text == "")
                {
                    MessageBox.Show("Please Enter a Name for the Wireguard Interface");
                    return;
                }
                if (textBoxWireguardListenPort.Text == "")
                {
                    Random random = new Random();
                    textBoxWireguardListenPort.Text = random.Next(10000, 60000 + 1).ToString(); // "+1" to include the upper bound
                }
                if (textBox16.Text != "")
                {
                    if (!Parser.IsValidPrivateKey(textBox16.Text))
                    {
                        MessageBox.Show("Please Enter a Valid Private Key, or leave empty for a random generated one");
                        return;
                    }
                }


                UpdateWGIntFromForm();
                await CreateWGInt();

                string name = comboBoxWireguardInterface.SelectedItem.ToString();

                WireguardInterface wgInt = RetrieveWGInt(name);
                textBox16.Text = wgInt.PrivateKey;
                textBoxWireguardPublicKey.Text = wgInt.PublicKey;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Creating Wireguard Interface: {textBoxWireguardInterface.Text}: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async Task CreateWGInt()
        {
            try
            {

                JObject payload = wgInterface.ToJObject();
                payload.Remove(".id");
                payload.Remove("public-key");
                payload.Remove("running");

                await Controller.CreateWGInt(payload);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Creating Wireguard Interface {wgInterface.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
           
        private void disconnect_Click(object sender, EventArgs e)
        {
            Disconnect();
        }

        private void UpdateWGIntFromForm()
        {
            wgInterface.Name = textBoxWireguardInterface.Text;
            wgInterface.Disabled = false;
            wgInterface.ListenPort = textBoxWireguardListenPort.Text;
            wgInterface.PrivateKey = textBox16.Text;
        }
        private void Disconnect()
        {
            // Reset isConnected flag to indicate disconnection
            isConnected = false;

            // Optionally, dispose of the HttpClient instance to release associated resources
            httpClient.Dispose();
            // Disable the textboxes after successful connection
            textBox9.Enabled = true; // Username textbox
            textBox10.Enabled = true; // Password textbox
            textBox1.Enabled = true; // IP Address textbox

            MessageBox.Show("Disconnected from router.");
        }

        private async void buttonWireguardEnableInterface_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxWireguardInterface.SelectedItem == null)
                {
                    MessageBox.Show("Select a  Wireguard Interface: ");
                    return;
                }
                JObject payload = new JObject
                {
                    [".id"] = wgInterface.Id,
                    ["name"] = wgInterface.Name
                };

                await Controller.EnableWGInt(payload);
                PopulateWGInterface();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Enabling Wireguard Interface {wgInterface.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void buttonWireguardDisableInterface_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxWireguardInterface.SelectedItem == null)
                {
                    MessageBox.Show("Select a Wireguard Interface ");
                    return;
                }
                JObject payload = new JObject
                {
                    [".id"] = wgInterface.Id,
                    ["name"] = wgInterface.Name
                };

                await Controller.DisableWGInt(payload);
                PopulateWGInterface();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Disabling Wireguard Interface {wgInterface.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void buttonWireguardDeleteInterface_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxWireguardInterface.SelectedItem == null)
                {
                    MessageBox.Show("Select a Wireguard Interface ");
                    return;
                }
                JObject payload = new JObject
                {
                    [".id"] = wgInterface.Id,
                    ["name"] = wgInterface.Name
                };

                await Controller.DeleteWGInt(payload);
                //PopulateWGInterface();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Deleting Wireguard Interface {wgInterface.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void buttonWGIntEdit_Click(object sender, EventArgs e)
        {

            try
            {
                if (textBoxWireguardInterface.Text == "")
                {
                    MessageBox.Show("Please Enter a Name for the Wireguard Interface");
                    return;
                }
                if (textBoxWireguardListenPort.Text == "")
                {
                    Random random = new Random();
                    textBoxWireguardListenPort.Text = random.Next(10000, 60000 + 1).ToString(); // "+1" to include the upper bound
                }
                if (textBox16.Text != "")
                {
                    if (!Parser.IsValidPrivateKey(textBox16.Text))
                    {
                        MessageBox.Show("Please Enter a Valid Private Key, or leave empty for a random generated one");
                        return;
                    }
                }


                UpdateWGIntFromForm();
                await EditWGInt();
                //PopulateWGInterface();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Editing Wireguard Interface: {textBoxWireguardInterface.Text}: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async Task EditWGInt()
        {
            try
            {

                JObject payload = wgInterface.ToJObject();
                payload.Remove(".id");
                payload.Remove("public-key");
                payload.Remove("running");

                await Controller.EditWGInt(payload, wgInterface.Id);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Creating Wireguard Interface {wgInterface.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonWireguardRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxWireguardInterface.SelectedItem == null)
                {
                    MessageBox.Show("Select a  Wireguard Interface ");
                    return;
                }
                if (!Parser.IsValidPrivateKey(textBox16.Text))
                {
                    MessageBox.Show("Select a Valid Private Key ");
                    return;
                }
                JObject payload = new JObject
                {
                    [".id"] = wgInterface.Id,
                    ["name"] = wgInterface.Name,
                    ["private-key"] = textBox16.Text
                };

                await Controller.UpdatePrivatePublicKey(payload);
                //PopulateWGInterface();
                PopulateWGInterface();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Deleting Wireguard Interface {wgInterface.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void comboBoxWireguardPeer_Enter(object sender, EventArgs e)
        {
            try
            {
                // Retrieve the list of WG Interfaces
                string response = await Controller.Retrieve("/rest/interface/wireguard/peers");
                List<string> WGPeers = Parser.ParseNamesFromJsonArray(response, "name");

                // Clear existing items in the ComboBox
                comboBoxWireguardPeer.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string wg in WGPeers)
                {
                    comboBoxWireguardPeer.Items.Add(wg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Wireguard Peers data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void comboBoxWireguardPeer_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear textboxes and comboboxes
            foreach (Control control in this.Controls)
            {
                if (control is TextBox)
                {
                    ((TextBox)control).Text = "";
                }
                if (control is ComboBox)
                {
                    ((ComboBox)control).SelectedIndex = -1;
                }
            }
            PopulateWGPeers();

        }

        private async void PopulateWGPeers()
        {
            string name = comboBoxWireguardPeer.SelectedItem.ToString();
            comboBoxWireguardPeerInterface.Items.Clear();

            WireguardPeers wgP = RetrieveWGPeer(name);
            await PopulatecomboBoxPeerInterface();

            wgPeer.Id = wgP.Id;
            wgPeer.Name = wgP.Name;

            textBoxWireguardPeerName.Text = wgP.Name;
            comboBoxWireguardPeerInterface.SelectedItem = wgP.Interface;
            textBox12.Text = wgP.PublicKey;
            checkBox6.Checked = !wgP.Disabled;
            string[] allowedAddr = wgP.AllowedAddress.Split(',');

            // Populate the textbox with servers
            textBox13.Clear();
            foreach (string addr in allowedAddr)
            {
                textBox13.Text += addr + Environment.NewLine;
            }
        }

        private WireguardPeers RetrieveWGPeer(string name)
        {
            try
            {
                // Make an HTTP GET request to the specified endpoint ("/rest/interface/wireguard/{name}")
                HttpResponseMessage response = httpClient.GetAsync(baseUrl + $"/rest/interface/wireguard/peers/{name}").Result;
                response.EnsureSuccessStatusCode(); // Throw an exception if the response is not successful

                // Read the response content as a string
                string responseBody = response.Content.ReadAsStringAsync().Result;

                // Deserialize the JSON response into a list of WGInt objects
                WireguardPeers wgPeer = JsonConvert.DeserializeObject<WireguardPeers>(responseBody);

                // Find the WireguardInterface object with the matching name
                return wgPeer;
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private async Task PopulatecomboBoxPeerInterface()
        {
            // Clear any existing items in the ComboBox
            comboBoxWireguardPeerInterface.Items.Clear();

            try
            {
                // Retrieve the list of wirelessInterfaces
                string response = await Controller.Retrieve("/rest/interface/wireguard");
                List<string> WGInt = Parser.ParseNamesFromJsonArray(response, "name");

                // Clear existing items in the ComboBox
                comboBoxWireguardPeerInterface.Items.Clear();

                // Add each bridge name as an item in the ComboBox
                foreach (string wg in WGInt)
                {
                    comboBoxWireguardPeerInterface.Items.Add(wg);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Wireguard Interfaces data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void buttonWireguardEnablePeer_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxWireguardPeer.SelectedItem == null)
                {
                    MessageBox.Show("Select a Wireguard Peer ");
                    return;
                }
                JObject payload = new JObject
                {
                    [".id"] = wgPeer.Id,
                    ["name"] = wgPeer.Name
                };

                await Controller.EnableWGPeer(payload);
                PopulateWGPeers();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Enabling Wireguard Peer {wgPeer.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void buttonWireguardDisablePeer_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxWireguardPeer.SelectedItem == null)
                {
                    MessageBox.Show("Select a Wireguard Peer ");
                    return;
                }
                JObject payload = new JObject
                {
                    [".id"] = wgPeer.Id,
                    ["name"] = wgPeer.Name
                };

                await Controller.DisableWGPeer(payload);
                PopulateWGPeers();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Desabling Wireguard Peer {wgPeer.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void buttonWireguardDeletePeer_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxWireguardPeer.SelectedItem == null)
                {
                    MessageBox.Show("Select a Wireguard Peer ");
                    return;
                }
                JObject payload = new JObject
                {
                    [".id"] = wgPeer.Id,
                    ["name"] = wgPeer.Name

                };

                await Controller.DeleteWGPeer(payload);
                //PopulateWGPeers();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Deleting Wireguard Peer {wgPeer.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private async void buttonWireguardCreatePeer_Click(object sender, EventArgs e)
        {

            try
            {
                if (textBoxWireguardPeerName.Text == "")
                {
                    MessageBox.Show("Please Enter a Name for the Wireguard Peer");
                    return;
                }
                if (comboBoxWireguardPeerInterface.Text == "")
                {
                    MessageBox.Show("Please Select a interface to Use");
                    return;

                }
                string[] lines = textBox13.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    if (!Parser.ValidateIPv6(line) && !Parser.IsValidIpAddress(line) && !Parser.IsValidIpAddressGateway(line))
                    {
                        MessageBox.Show("Please Select a Valid IP or IPv6 Address");
                        return;
                    }
                }
                bool helper = await IsPubKeyValid(textBox12.Text);
                if (!helper)
                {
                    MessageBox.Show("Please Select a Public Key that is not being used.");
                    return;
                }
                UpdateWGPeerFromForm();

                await CreateWGPeer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Creating Wireguard Peer: {textBoxWireguardPeerName.Text}: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
        public async Task<bool> IsPubKeyValid(string key)
        {
            try
            {
                // Retrieve the list of WireGuard peers.
                string response = await Controller.Retrieve("/rest/interface/wireguard/peers");

                // Parse the JSON response to extract public keys
                List<string> pubKeys = Parser.ParseNamesFromJsonArray(response, "public-key");

                string textBoxContent = key;
                foreach (string pubKey in pubKeys)
                {
                    if (textBoxContent == pubKey)
                    {
                        return false;

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Public Key data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task CreateWGPeer()
        {
            try
            {

                JObject payload = wgPeer.ToJObject();
                payload.Remove(".id");
                payload["disabled"] = false;

                await Controller.CreateWGPeer(payload);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Creating Wireguard Peer {wgPeer.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateWGPeerFromForm()
        {
            wgPeer.Name = textBoxWireguardPeerName.Text;
            wgPeer.Interface = comboBoxWireguardPeerInterface.SelectedItem.ToString();
            wgPeer.PublicKey = textBox12.Text;
            string text = textBox13.Text;
            text = text.Replace("\r\n", ",");
            text = text.Replace("\n", ",");
            text = text.Replace("\r", ",");
            wgPeer.AllowedAddress = text;
        }

        private async void buttonWGPeerEdit_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBoxWireguardPeerName.Text == "")
                {
                    MessageBox.Show("Please Enter a Name for the Wireguard Peer");
                    return;
                }
                if (comboBoxWireguardPeerInterface.Text == "")
                {
                    MessageBox.Show("Please Select a interface to Use");
                    return;

                }
                string[] lines = textBox13.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    if (!Parser.ValidateIPv6(line) && !Parser.IsValidIpAddress(line) && !Parser.IsValidIpAddressGateway(line))
                    {
                        MessageBox.Show("Please Select a Valid IP or IPv6 Address");
                        return;
                    }
                }
                /*bool helper = await IsPubKeyValid(textBox12.Text);
                if (!helper)
                {
                    MessageBox.Show("Please Select a Public Key that is not being used.");
                    return;
                }*/
                /*bool helper2 = await IsWGPeerNameAvailable(textBoxWireguardPeerName.Text);
                if (!helper2)
                {
                    MessageBox.Show("Please Select a Name that is not being used.");
                    return;
                }*/
                UpdateWGPeerFromForm();
                await EditWGPeer();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Editing Wireguard Peer: {textBoxWireguardPeerName.Text}: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async Task EditWGPeer()
        {
            try
            {

                JObject payload = wgPeer.ToJObject();
                payload.Remove(".id");

                await Controller.EditWGPeer(payload, wgPeer.Id);
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Error Editing Wireguard Peer {wgPeer.Name} " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async Task<bool> IsWGPeerNameAvailable(string name)
        {
            try
            {
                // Retrieve the list of WireGuard peers.
                string response = await Controller.Retrieve("/rest/interface/wireguard/peers");

                // Parse the JSON response to extract public keys
                List<string> names = Parser.ParseNamesFromJsonArray(response, "name");

                string textBoxContent = name;
                foreach (string nam in names)
                {
                    if (textBoxContent == nam)
                    {
                        return false;

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Wireguard Peer Name data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
