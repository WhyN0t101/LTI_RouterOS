using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTI_RouterOS.Model
{
    internal class DNSStatic
    {

        [JsonProperty(".id")]
        public string Id { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("address-list")]
        public string AddressList { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("match-subdomain")]
        public bool? MatchSubdomain { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ttl")]
        public string TTL { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        public JObject ToJObject()
        {
            return JObject.FromObject(this);
        }
    }
}

