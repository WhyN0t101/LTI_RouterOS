using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTI_RouterOS.Model
{
    internal class WirelessSettings
    {
        
        [JsonProperty(".id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mtu")]
        public int Mtu { get; set; }

        [JsonProperty("l2mtu")]
        public int L2Mtu { get; set; }

        [JsonProperty("mac-address")]
        public string MacAddress { get; set; }

        [JsonProperty("arp")]
        public string Arp { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("band")]
        public string Band { get; set; }

        [JsonProperty("channel-width")]
        public string ChannelWidth { get; set; }

        [JsonProperty("frequency")]
        public int Frequency { get; set; }

        [JsonProperty("ssid")]
        public string Ssid { get; set; }

        [JsonProperty("security-profile")]
        public string SecurityProfile { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("frequency-mode")]
        public string FrequencyMode { get; set; }

        [JsonProperty("installation")]
        public string Installation { get; set; }

        [JsonProperty("default-authentication")]
        public bool DefaultAuthentication { get; set; }

        [JsonProperty("arp-timeout")]
        public string ArpTimeout { get; set; }

        [JsonProperty("disabled")]
        public bool disabled { get; set; }

        public WirelessSettings()
        {
            // Initialize all optional property flags to false by default
            DefaultAuthentication = false;
        }
    }

    


}

