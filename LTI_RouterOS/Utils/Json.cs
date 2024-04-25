using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
