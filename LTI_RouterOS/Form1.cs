using LTI_RouterOS.Controller;
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
        private GET getController; // Declaration of getData variable
        private bool isConnected = false;
        public Form1()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            InitializeComponent();
            httpClient = new HttpClient();            
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
                getController = new GET(username, password); // Instantiate GET class after user provides credentials
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
            await getController.TestConnection(baseUrl); // Test connection asynchronously
            isConnected = true;
            MessageBox.Show("Connected to router successfully!");
        }


        private async void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                string response = await getController.Retrieve(baseUrl, "/rest/interface");
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
                textBox2.Text = await getController.GetBridges(baseUrl, "/rest/interface/bridge");
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
                comboBox1.Text = await getController.GetBridges(baseUrl, "/rest/interface/bridge");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving bridge data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
