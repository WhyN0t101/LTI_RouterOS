using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LTI_RouterOS.Model
{
    internal class DNS
    {

        [JsonProperty("allow-remote-requests")]
        public bool AllowRemoteRequests { get; set; }

        [JsonProperty("cache-max-ttl")]
        public string CacheMaxTTL { get; set; }

        [JsonProperty("cache-size")]
        public string CacheSize { get; set; }

        [JsonProperty("cache-used")]
        public string CacheUsed { get; set; }

        [JsonProperty("max-concurrent-queries")]
        public string MaxConcurrentQueries { get; set; }

        [JsonProperty("max-concurrent-tcp-sessions")]
        public string MaxConcurrentTcpSessions { get; set; }

        [JsonProperty("max-udp-packet-size")]
        public string MaxUdpPacketSize { get; set; }

        [JsonProperty("query-server-timeout")]
        public string QueryServerTimeout { get; set; }

        [JsonProperty("query-total-timeout")]
        public string QueryTotalTimeout { get; set; }

        [JsonProperty("servers")]
        public string Servers { get; set; }

        public JObject ToJObject()
        {
            return JObject.FromObject(this);
        }
    }
}
