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

        [JsonProperty("address-pool")]
        public string AddressPool { get; set; }

        [JsonProperty("disabled")]
        public string Disabled { get; set; }

        [JsonProperty("conflict-detection")]
        public bool ConflictDetection { get; set; }

        [JsonProperty("use-framed-as-classless")]
        public bool UseFramedAsClassless { get; set; }

        [JsonProperty("interface")]
        public string Interface { get; set; }

        [JsonProperty("lease-time")]
        public string LeaseTime { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("use-radius")]
        public string UseRadius { get; set; }

        public DHCPServer()
        {
            UseFramedAsClassless = true;
            ConflictDetection = true;
        }
    }

    
}
