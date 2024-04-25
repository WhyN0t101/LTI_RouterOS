using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LTI_RouterOS.Model
{
    internal class WireguardPeers
    {
        [JsonProperty(".id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }

        [JsonProperty("allowed-address")]
        public string AllowedAddress { get; set; }

        [JsonProperty("interface")]
        public string Interface { get; set; }

        [JsonProperty("public-key")]
        public string PublicKey { get; set; }

        public JObject ToJObject()
        {
            return JObject.FromObject(this);
        }
    }
}
