using Long.Kernel.Modules.Interfaces;
using Long.Kernel.Network.Game;
using Long.Module.Relation.Network;
using Long.Network.Packets;

namespace Long.Module.Relation
{
    public sealed class NetworkMessageHandler : INetworkMessageHandler
    {
        public Task<bool> OnReceiveAsync(GameClientBase actor, PacketType type, byte[] message)
        {
            MsgBase<GameClientBase> msg;
            if (type == PacketType.MsgFriend)
            {
                msg = new MsgFriend();
            }
            else if (type == PacketType.MsgEnemyList)
            {
                msg = new MsgEnemyList();
            }
            else
            {
                return Task.FromResult(false);
            }

            if (actor?.Character?.Map == null)
            {
                return Task.FromResult(true);
            }

            msg.Decode(message);
            actor.Character.QueueAction(() => msg.ProcessAsync(actor));
            return Task.FromResult(true);
        }
    }
}
