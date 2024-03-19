using LTI_RouterOS.Controller;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;

namespace LTI_RouterOS
{
    public partial class Form1 : Form
    {
        private readonly HttpClient httpClient;
        private string baseUrl;
        private readonly GET getController; // Declaration of getData variable

        public Form1()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            InitializeComponent();
            httpClient = new HttpClient();
            getController = new GET();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Connect(string ipAddress)
        {
            baseUrl = "http://" + ipAddress;
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("admin:")));
        }
        private void connectButton_Click_1(object sender, EventArgs e)
        {
            string ipAddress = textBox1.Text.Trim();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                Connect(ipAddress);
                MessageBox.Show("Connected to " + ipAddress, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please enter an IP address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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



        private void getAllInt_Click(object sender, EventArgs e)
        {
            try
            {
                string response = getController.Retrieve(baseUrl, "/rest/interface");
                List<string> interfaceNames = ParseNamesFromJsonArray(response, "default-name");

                if (interfaceNames.Count > 0)
                {
                    InterfacesBox.Text = string.Join(Environment.NewLine, interfaceNames);
                }
                else
                {
                    InterfacesBox.Text = "No interface names found.";
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle request errors
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void getAllBridge_Click(object sender, EventArgs e)
        {
            try
            {
                string response = getController.Retrieve(baseUrl,"/rest/interface/bridge");
                List<string> bridgeNames = ParseNamesFromJsonArray(response, "name");

                if (bridgeNames.Count > 0)
                {
                    bridgeText.Text = string.Join(Environment.NewLine, bridgeNames);
                }
                else
                {
                    bridgeText.Text = "No bridge names found.";
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle request errors
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}
