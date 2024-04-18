using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTI_RouterOS.Model
{
    internal class DHCPServer
    {

        [JsonProperty(".id")]
        public string Id { get; set; }

        [JsonProperty("add-arp")]
        public string AddArp { get; set; }

        [JsonProperty("address-pool")]
        public string AddressPool { get; set; }

        [JsonProperty("always-broadcast")]
        public string AlwaysBroadcast { get; set; }

        [JsonProperty("authoritative")]
        public string Authoritative { get; set; }

        [JsonProperty("delay-threshold")]
        public string DelayThreshold { get; set; }

        [JsonProperty("disabled")]
        public string Disabled { get; set; }

        [JsonProperty("interface")]
        public string Interface { get; set; }

        [JsonProperty("lease-time")]
        public string LeaseTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("server-address")]
        public string ServerAddress { get; set; }

        [JsonProperty("use-radius")]
        public string UseRadius { get; set; }
    }
}
