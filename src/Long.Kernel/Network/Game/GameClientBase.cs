using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.Registration;
using Long.Kernel.States.User;
using Long.Network.Packets;
using Long.Network.Security;
using Long.Network.Sockets;
using System.Drawing;
using System.Net.Sockets;

namespace Long.Kernel.Network.Game
{
    public abstract class GameClientBase : TcpServerActor
    {
        public GameClientBase(Socket socket, Memory<byte> buffer, uint readPartition, uint writePartition)
            : base(socket, buffer, null, readPartition, writePartition, "")
        {
            GUID = Guid.NewGuid();
        }

        public uint Identity => Character?.Identity ?? 0;
        public uint AccountIdentity { get; set; }
        public ushort AuthorityLevel { get; set; }
        public string MacAddress { get; set; } = "Unknown";
        public int LastLogin { get; set; }
        public uint Seed { get; set; }
        public Guid GUID { get; }
        public AwaitingCreation Creation { get; set; }
        public Character Character { get; set; }

        public Task DisconnectWithMessageAsync(MsgConnectEx.RejectionCode rejectionCode)
        {
            return SendAsync(new MsgConnectEx(rejectionCode), () =>
            {
                Disconnect();
                return Task.CompletedTask;
            });
        }

        public Task DisconnectWithMessageAsync(string message)
        {
            return SendAsync(new MsgTalk(TalkChannel.Talk, Color.White, message), () =>
            {
                Disconnect();
                return Task.CompletedTask;
            });
        }

        public Task DisconnectWithMessageAsync(IPacket msg)
        {
            return SendAsync(msg, () =>
            {
                Disconnect();
                return Task.CompletedTask;
            });
        }
    }
}
