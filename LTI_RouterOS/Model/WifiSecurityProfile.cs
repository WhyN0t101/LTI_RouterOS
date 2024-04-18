using Newtonsoft.Json;
using System;

namespace LTI_RouterOS.Model
{
    internal class WifiSecurityProfile
    {
        [JsonProperty(".id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("authentication-types")]
        public string AuthenticationType { get; set; }

        [JsonProperty("unicast-ciphers")]
        public string UnicastCiphers { get; set; }

        [JsonProperty("group-ciphers")]
        public string GroupCiphers { get; set; }

        [JsonProperty("wpa-pre-shared-key")]
        public string WpaPresharedKey { get; set; }

        [JsonProperty("wpa2-pre-shared-key")]
        public string Wpa2PresharedKey { get; set; }

        [JsonProperty("supplicant-identity")]
        public string SupplicantIdentity { get; set; }

        [JsonProperty("group-key-update")]
        public string GroupKeyUpdate { get; set; }

        [JsonProperty("management-protection")]
        public string ManagementProtection { get; set; }

        [JsonProperty("management-protection-key")]
        public string ManagementProtectionKey { get; set; }
        
        public bool IsAuthenticationTypeSet { get; set; }
        public bool IsUnicastCiphersSet { get; set; }
        public bool IsGroupCiphersSet { get; set; }
        public bool IsWpaPresharedKeySet { get; set; }
        public bool IsWpa2PresharedKeySet { get; set; }
        public bool IsSupplicantIdentitySet { get; set; }
        public bool IsGroupKeyUpdateSet { get; set; }
        public bool IsManagementProtectionSet { get; set; }
        public bool IsManagementProtectionKeySet { get; set; }

        public WifiSecurityProfile()
        {
            IsAuthenticationTypeSet = false;
            IsUnicastCiphersSet = false;
            IsGroupCiphersSet = false;
            IsWpaPresharedKeySet = false;
            IsWpa2PresharedKeySet = false;
            IsSupplicantIdentitySet = false;
            IsGroupKeyUpdateSet = false;
            IsManagementProtectionSet = false;
            IsManagementProtectionKeySet = false;
        }
    }
}
