using System.Net;

namespace Long.Network
{
    public static class NetworkDefinition
    {
        public const string GAME_SERVER_FOOTER = "TQServer";
        public const string GAME_CLIENT_FOOTER = "TQClient";
        public const string ACCOUNT_FOOTER = "ulXxXhgz";
        public const string NPC_FOOTER = "WUuTxfpe";
        public const string PATCHER_FOOTER = "EE5n3yMN";
        public const string GM_TOOLS_FOOTER = "BApZYLpQ";

        // true if ipAddress falls inside the CIDR range, example
        // bool result = IsInRange("10.50.30.7", "10.0.0.0/8");
        public static bool IsInRange(string ipAddress, string CIDRmask)
        {
            string[] parts = CIDRmask.Split('/');
            if (parts.Length == 1)
            {
                return ipAddress.Equals(CIDRmask);
            }

            int prefixLength = int.Parse(parts[1]);
            int ipAddr = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);
            int cidrAddr = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);
            int cidrMask = IPAddress.HostToNetworkOrder(-1 << (32 - prefixLength));
            if (prefixLength == 32)
            {
                return true;
            }
            return (ipAddr & cidrMask) == (cidrAddr & cidrMask);
        }
    }
}
