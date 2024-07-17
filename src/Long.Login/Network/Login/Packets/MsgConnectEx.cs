using Long.Network.Packets.Game;

namespace Long.Login.Network.Login.Packets
{
    public sealed class MsgConnectEx : MsgConnectEx<LoginClient>
    {
        public MsgConnectEx(RejectionCode rejectionCode) 
            : base(rejectionCode)
        {
        }

        public MsgConnectEx(string ipAddress, uint port, uint accountId, uint data) 
            : base(ipAddress, port, accountId, data)
        {
        }
    }
}
