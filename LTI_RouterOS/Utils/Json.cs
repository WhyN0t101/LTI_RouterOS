using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LTI_RouterOS
{
    public class Json
    {
        public List<string> ParseNamesFromJsonArray(string json, string propertyName)
        {
            List<string> names = new List<string>();
            JArray jsonArray = JArray.Parse(json);
            foreach (JObject jsonObject in jsonArray)
            {
                if (jsonObject.ContainsKey(propertyName))
                {
                    string name = (string)jsonObject[propertyName];
                    names.Add(name);
                }
            }
            return names;
        }

        public string ParseNameFromJsonObject(string json, string propertyName)
        {
            JObject jsonObject = JObject.Parse(json);
            if (jsonObject.ContainsKey(propertyName))
            {
                return (string)jsonObject[propertyName];
            }
            return null;
        }
        public string ParseTimeFormat(string time)
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

        public List<string> ParseNameSlaveFromJsonArray(string json, string propertyName)
        {
            List<string> names = new List<string>();
            JArray jsonArray = JArray.Parse(json);
            foreach (JObject jsonObject in jsonArray)
            {
                if (jsonObject.ContainsKey(propertyName))
                {
                    string name = (string)jsonObject[propertyName];
                    names.Add(name);
                }
            }
            return names;
        }

        public bool ValidateIPv6(string ipAddress)
        {
            IPAddress ip;
            return IPAddress.TryParse(ipAddress, out ip) && ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
        }

        public bool ValidateCNAME(string domain)
        {
            try
            {
                IPHostEntry ipEntry = Dns.GetHostEntry(domain);
                return ipEntry.HostName != domain;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public List<(string id, string name)> ParseIdNameFromJsonArray(string jsonArray, string idKey, string nameKey)
        {
            List<(string id, string name)> result = new List<(string id, string name)>();

            JArray array = JArray.Parse(jsonArray);

            foreach (JObject obj in array)
            {
                string id = obj.Value<string>(idKey);
                string name = obj.Value<string>(nameKey);
                result.Add((id, name));
            }

            return result;
        }

        public bool IsValidPrivateKey(string privateKey)
        {
            // Check length
            if (privateKey.Length != 44)
            {
                return false;
            }

            // Decode Base64
            byte[] decodedPrivateKey;
            try
            {
                decodedPrivateKey = Convert.FromBase64String(privateKey);
            }
            catch (FormatException)
            {
                return false;
            }

            // Validate structure
            if (decodedPrivateKey.Length != 32)
            {
                return false;
            }

            return true;
        }
        // Validate an IPv4 address without a subnet mask
        public bool IsValidIpAddressGateway(string ipAddress)
        {
            IPAddress ip;
            return IPAddress.TryParse(ipAddress, out ip) && ip.AddressFamily == AddressFamily.InterNetwork;
        }

        public bool IsValidIpAddress(string ipAddress)
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

        public bool ValidateIpAddress(string ipAddressText)
        {
            // Split the multiline text into individual lines
            string[] lines = ipAddressText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                // Try to parse each line as an IP address
                if (!IPAddress.TryParse(line, out IPAddress ip))
                {
                    MessageBox.Show($"Invalid IP address: {line}");
                    return false;
                }
            }

            // All IP addresses are valid
            return true;
        }

        public bool ValidateTimeFormat(string inputTime, int mode)
        {
            //0 -> hh:mm:ss, allows 00:00:00
            //1 -> sec profile
            //2 -> dhcp
            //3 -> bridge AgeingTime
            //4 -> hh:mm:ss above 0
            string pattern = "";
            string errorMessage = "Invalid time format. Please enter the time in the format 'hh:mm:ss'.";

            switch (mode)
            {
                case 0: // Change the default case label to case 0
                    //General Purpose
                    pattern = @"^(?:[0-9]+|2[0-9]|[3-9][0-9]*):[0-5][0-9]:[0-5][0-9]$"; // Pattern for hh:mm:ss format
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
                    // Regular expression pattern for hh:mm:ss format and above 00:00:09
                    errorMessage = "Invalid time format. Please enter the time in the format 'hh:mm:ss' and ensure it is greater than 00:00:09.";
                    pattern = @"^(?:[0-9]+|2[0-9]|[3-9][0-9]*):(?:[0-5][0-9]):(?:[0-5][0-9])$"; 
                    break;
                case 4:
                    errorMessage = "Invalid time format. Please enter the time in the format 'hh:mm:ss', and above 0s.";
                    pattern = @"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]:(0[1-9]|[1-5][0-9])$";
                    break;
                default:
                    MessageBox.Show(errorMessage);
                    return false;
            }

            if (Regex.IsMatch(inputTime, pattern))
            {
                return true;
            }
            else
            {
                MessageBox.Show(errorMessage);
                return false;
            }
        }

        public IEnumerable<string> GetFilteredValues(string prefix)
        {
            string[] values = {
                "2ghz-b",
                "2ghz-b-only-g",
                "2ghz-b/g",
                "2ghz-b/g/n",
                "2ghz-g/n",
                "2ghz-only-n",
                "5ghz-a",
                "5ghz-only-n",
                "5ghz-a/n",
                "5ghz-a/n/ac",
                "5ghz-only-ac",
                "5ghz-n/ac"
            };

            return values.Where(value => value.StartsWith(prefix));
        }

        


    }
}
