using LTI_RouterOS.Controller;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;

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
            getController = new GET(); // Initialization of getData variable
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void Connect(string ipAddress)
        {
            baseUrl =  "http://" + ipAddress;
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
        private void getAllInt_Click(object sender, EventArgs e)
        {
            try
            {
                string response = getController.Retrieve(baseUrl, "/rest/interface");
                List<string> defaultNames = ParseDefaultNames(response);

                if (defaultNames.Count > 0)
                {
                    InterfacesBox.Text = string.Join(Environment.NewLine, defaultNames);
                }
                else
                {
                    InterfacesBox.Text = "No default names found.";
                }
            }
            catch (Exception ex)
            {
                // Handle request errors
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private List<string> ParseDefaultNames(string response)
        {
            List<string> defaultNames = new List<string>();

            dynamic jsonData = JsonConvert.DeserializeObject(response);
            foreach (var item in jsonData)
            {
                string defaultName = item["default-name"];
                defaultNames.Add(defaultName);
            }

            return defaultNames;
        }


    }
}
