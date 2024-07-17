using Long.Kernel.Network.Game;
using Long.Kernel.Service;
using System.Net.Sockets;

namespace Long.Game.Network.Game
{
    public sealed class GameClient : GameClientBase
    {
        public GameClient(Socket socket, Memory<byte> buffer, uint readPartition, uint writePartition) 
            : base(socket, buffer, readPartition, writePartition)
        {
        }

        public override Task SendAsync(byte[] packet)
        {
            Services.NetworkMonitor.Send(packet.Length);
            GameServerSocket.Instance?.Send(this, packet);
            return Task.CompletedTask;
        }

        public override Task SendAsync(byte[] packet, Func<Task> task)
        {
            Services.NetworkMonitor.Send(packet.Length);
            GameServerSocket.Instance?.Send(this, packet, task);
            return Task.CompletedTask;
        }
    }
}
