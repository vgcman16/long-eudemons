using Long.Network.Packets.Game;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgConnectEx : MsgConnectEx<GameClientBase>
    {
        public MsgConnectEx(RejectionCode rejectionCode) : base(rejectionCode)
        {
        }

        public MsgConnectEx(string ipAddress, uint port, uint accountId, uint data) 
            : base(ipAddress, port, accountId, data)
        {
        }
    }
}
