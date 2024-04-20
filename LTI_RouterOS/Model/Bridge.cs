using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTI_RouterOS.Model
{
    internal class Bridge
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mtu")]
        public string Mtu { get; set; }

        [JsonProperty("arp")]
        public string Arp { get; set; }

        [JsonProperty("arp-timeout")]
        public string ArpTimeout { get; set; }

        [JsonProperty("ageing-time")]
        public string AgeingTime { get; set; }

        [JsonProperty("igmp-snooping")]
        public bool IgmpSnooping { get; set; }

        [JsonProperty("dhcp-snooping")]
        public bool DhcpSnooping { get; set; }

        [JsonProperty("fast-forward")]
        public bool FastForward { get; set; }
    }
}
