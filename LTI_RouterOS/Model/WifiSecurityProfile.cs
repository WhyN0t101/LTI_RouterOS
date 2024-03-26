using System;
namespace LTI_RouterOS.Model
{
    internal class WifiSecurityProfile
    {
        public string Name { get; set; }
        public string Mode { get; set; } // Mode property is required

        // Optional properties
        public string AuthenticationType { get; set; }
        public string UnicastCiphers { get; set; }
        public string GroupCiphers { get; set; }
        public string WpaPresharedKey { get; set; }
        public string Wpa2PresharedKey { get; set; }
        public string SupplicantIdentity { get; set; }
        public string GroupKeyUpdate { get; set; }
        public string ManagementProtection { get; set; }
        public string ManagementProtectionKey { get; set; }

        // Flags to indicate whether optional properties are set
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
            // Initialize all optional property flags to false by default
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

