using System;
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

        public Form1()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            InitializeComponent();
            httpClient = new HttpClient();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void Connect(string ipAddress)
        {
            baseUrl = "http://" + ipAddress;
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("admin:")));
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
           //Text Box to Connect
           //string ipAddress = textBoxIPAddress.Text.Trim();
           /* if (!string.IsNullOrEmpty(ipAddress))
            {
                Connect(ipAddress);
                MessageBox.Show("Connected to " + ipAddress, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Please enter an IP address.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }*/
        }

        private void buttonSendRequest_Click(object sender, EventArgs e)
        {
            try
            {
                HttpResponseMessage response = httpClient.GetAsync(baseUrl).Result;
                response.EnsureSuccessStatusCode(); // Ensure success status code
                string responseBody = response.Content.ReadAsStringAsync().Result;
                // Handle the response data as needed
                MessageBox.Show("Response: " + responseBody, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (HttpRequestException ex)
            {
                // Handle request errors
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabPage4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
