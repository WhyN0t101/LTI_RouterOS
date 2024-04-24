using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTI_RouterOS.Model
{
    internal class WireguardInterface
    {
        [JsonProperty(".id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("listen-port")]
        public string ListenPort { get; set; }

        [JsonProperty("private-key")]
        public string PrivateKey { get; set; }

        [JsonProperty("public-key")]
        public string PublicKey { get; set; }
       
        [JsonProperty("running")]
        public bool Running { get; set; }
        public JObject ToJObject()
        {
            return JObject.FromObject(this);
        }
    }
}
