using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;

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
    }
}
