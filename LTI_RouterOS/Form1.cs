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
        private Json Parser = new Json();


        public Form1()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            InitializeComponent();
            httpClient = new HttpClient();
            wifiProfile = new WifiSecurityProfile();
            wirelessSettings = new WirelessSettings();
            dhcpServer = new DHCPServer();
            InitializeComboBoxes();

        }
        private void InitializeComboBoxes()
        {
            textBox4.Enabled = false;
            textBox5.Enabled = false;
            textBox6.Enabled = false;
            textBox7.Enabled = false;
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
            numericUpDown1.Maximum = 2000;
            numericUpDown1.Value = 1492;
            comboBox15.SelectedIndex = 0;
            comboBoxCheckGateway.SelectedIndex = 2;
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            textBox1.Text = "192.168.79.1";
            textBox9.Text = "admin";
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

        private void tabControl1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // Check if connected to router before allowing access to other tabs
            if (!isConnected && tabControl1.SelectedIndex != 0)
            {
                MessageBox.Show("Please connect to the router first.");
                tabControl1.SelectedIndex = 0; // Switch back to the connection tab
            }
        }



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
            if (ValidateAndUpdateTimeFormat(textBox7.Text) == "")
            {
                return;
            }

            if (!ValidateDynamicKeys())
            {
                return;
            }

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
            string arpTimeout = string.IsNullOrWhiteSpace(textBoxArpTimeoutBridge.Text) ? null : Parser.ParseTimeFormat(textBoxArpTimeoutBridge.Text);
            string ageingTime = string.IsNullOrWhiteSpace(textBoxAgeingTime.Text) ? null : Parser.ParseTimeFormat(textBoxAgeingTime.Text);
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
                /*
                checkedListBox1.SetItemChecked(0, false);
                checkedListBox1.SetItemChecked(1, false);
                checkedListBox1.SetItemChecked(2, false);
                checkedListBox1.SetItemChecked(3, false);
                */
                // Disable certain text boxes or checkboxes
                checkedListBox1.Enabled = false;
                //checkedListBox2.SetItemChecked(0, true);
                checkedListBox2.Enabled = false;
                //checkedListBox3.SetItemChecked(0, true);
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
                //checkedListBox2.SetItemChecked(0, true);
                //checkedListBox3.SetItemChecked(0, true);
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = true;

            }
            else if (selectedItem == "static keys optional")
            {
                /*
                checkedListBox1.SetItemChecked(0, false);
                checkedListBox1.SetItemChecked(1, false);
                checkedListBox1.SetItemChecked(2, false);
                checkedListBox1.SetItemChecked(3, false);
                */


                checkedListBox1.Enabled = false;
                //checkedListBox2.SetItemChecked(0, false);
                //checkedListBox2.SetItemChecked(1, false);
                checkedListBox2.Enabled = false;
                //checkedListBox3.SetItemChecked(0, false);
                //checkedListBox3.SetItemChecked(0, false);
                checkedListBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = false;

            }
            else if (selectedItem == "static keys required")
            {
                /*
                checkedListBox1.SetItemChecked(0, false);
                checkedListBox1.SetItemChecked(1, false);
                checkedListBox1.SetItemChecked(2, false);
                checkedListBox1.SetItemChecked(3, false);
                ~*/
                checkedListBox1.Enabled = false;
                //checkedListBox2.SetItemChecked(0, false);
                //checkedListBox2.SetItemChecked(1, false);
                checkedListBox2.Enabled = false;
                //checkedListBox3.SetItemChecked(0, false);
                //checkedListBox3.SetItemChecked(1, false);
                checkedListBox3.Enabled = false;
                textBox4.Enabled = false;
                textBox5.Enabled = false;
                textBox6.Enabled = false;
                textBox7.Enabled = false;
            }
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

        //MTU Range Tem de ser alto
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
            // Defaults
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Select a bridge");
                return;
            }
            string selected = comboBox1.SelectedItem.ToString();

            string response = await Controller.Retrieve("/rest/interface/bridge/" + selected + "");

            // Parse the JSON response into a JObject
            JObject bridgeObject = JObject.Parse(response);

            // Retrieve the bridge ID directly from the JObject
            string bridgeId = bridgeObject[".id"].ToString();

            string bridgeName = textBoxBridgeName.Text;
            int mtu = (int)numericUpDown1.Value;
            string arpEnabled = comboBoxARP.SelectedItem.ToString();
            string arpTimeout = string.IsNullOrWhiteSpace(textBoxArpTimeoutBridge.Text) ? null : Parser.ParseTimeFormat(textBoxArpTimeoutBridge.Text);
            string ageingTime = string.IsNullOrWhiteSpace(textBoxAgeingTime.Text) ? null : Parser.ParseTimeFormat(textBoxAgeingTime.Text);
            bool dhcpSnooping = checkBoxDHCPSnooping.Checked;
            bool igmpSnooping = checkBoxIGMP.Checked;
            bool fastForward = checkBoxFF.Checked;

            try
            {
                await Controller.UpdateBridge(bridgeId, bridgeName, mtu, arpEnabled, arpTimeout, ageingTime, igmpSnooping, dhcpSnooping, fastForward);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating bridge: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async void button10_Click(object sender, EventArgs e)
        {
            if (WirelessInterfaceCombobox.SelectedItem == null)
            {
                MessageBox.Show("Select a Wireless Interface: ");
                return;
            }
            string name = WirelessInterfaceCombobox.SelectedItem.ToString();
            WirelessSettings settings = RetrieveWirelessSettings(name);

            if (settings.disabled == "true")
            {
                MessageBox.Show("Wireless Interface already disabled");
                return;
            }

            try
            {
                await Controller.DeactivateWirelessInterface(settings.Id);
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

            if (settings.disabled == "false")
            {
                MessageBox.Show("Wireless Interface already enabled");
                return;
            }

            try
            {
                await Controller.ActivateWirelessInterface(settings.Id);
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
                ["channel-width"] = comboBoxChannelWidth.SelectedItem.ToString(),
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
            if (IsValidIpAddress(destAddress) && IsValidIpAddressGateway(gateway))
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
        // Validate an IPv4 address without a subnet mask
        private bool IsValidIpAddressGateway(string ipAddress)
        {
            IPAddress ip;
            return IPAddress.TryParse(ipAddress, out ip) && ip.AddressFamily == AddressFamily.InterNetwork;
        }



        private bool IsValidIpAddress(string ipAddress)
        {
            try
            {
                // Attempt to parse the IP address with subnet mask
                IPAddress ip;
                string[] parts = ipAddress.Split('/');

                // Check if there are two parts (IP address and subnet mask)
                if (parts.Length != 2)
                    return false;

                // Validate the IP address part
                if (!IPAddress.TryParse(parts[0], out ip))
                    return false;

                // Validate the subnet mask part
                int subnetMask = int.Parse(parts[1]);
                if (subnetMask < 0 || subnetMask > 32)
                    return false;

                return true; // IP address with subnet mask is valid
            }
            catch (Exception)
            {
                return false; // Exception occurred while parsing
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


        private string ValidateAndUpdateTimeFormat(string inputTime)
        {

            // Define a regular expression pattern for the expected time format "hh:mm:ss"
            string pattern = @"^([01]?[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$";

            // Check if the input matches the expected format
            if (Regex.IsMatch(inputTime, pattern))
            {
                // Parse the inputted time string to a TimeSpan object
                TimeSpan timeSpan = TimeSpan.Parse(inputTime);

                // Convert TimeSpan to the desired format
                string newTimeFormat = TimeSpanToString(timeSpan);

                return newTimeFormat;
            }
            else
            {
                // If the input format is invalid, display an error message
                MessageBox.Show("Invalid time format. Please enter the time in the format 'hh:mm:ss'.");
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
            if (ValidateAndUpdateTimeFormat(textBox7.Text) == "")
            {
                return;
            }

            if (!ValidateDynamicKeys())
            {
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
                    if (IsValidIpAddress(destAddress) && IsValidIpAddressGateway(gateway))
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
            if (string.IsNullOrEmpty(textBoxEnderecoIP.Text) || string.IsNullOrEmpty(textBoxNetwork.Text) || IsValidIpAddress(textBoxEnderecoIP.Text.Trim()) || IsValidIpAddressGateway(textBoxNetwork.Text))
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
    }
}
