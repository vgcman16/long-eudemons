using Long.Kernel;
using Long.Kernel.Managers;
using Long.Kernel.Network.Game;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.Processors;
using Long.Kernel.Service;
using Long.Kernel.States.User;
using Long.Network.Packets;
using Long.Network.Sockets;
using Serilog;
using System.Net.Sockets;

namespace Long.Game.Network.Game
{
    public sealed class GameServerSocket : TcpServerListener<GameClient>
    {
        private static readonly ILogger logger = Log.ForContext<GameServerSocket>();

        public static GameServerSocket Instance { get; private set; }

        public GameServerSocket(ServerSettings serverSettings)
            : base(serverSettings.Game.MaxOnlinePlayers,
                  readProcessors: serverSettings.Game.Listener.RecvProcessors, 
                  writeProcessors: serverSettings.Game.Listener.SendProcessors)
        {
            if (Instance != null)
            {
                throw new ApplicationException("Cannot start server socket twice!");
            }

            Instance = this;
        }

        protected override async Task<GameClient> AcceptedAsync(Socket socket, Memory<byte> buffer)
        {
            uint readPartition = packetProcessor.SelectReadPartition();
            uint writePartition = packetProcessor.SelectWritePartition();
            var client = new GameClient(socket, buffer, readPartition, writePartition);
            return client;
        }

        protected override void Received(GameClient actor, ReadOnlySpan<byte> packet)
        {
            Services.NetworkMonitor.Receive(packet.Length);
            if (actor.ConnectionStage == TcpServerActor.Stage.Exchange)
            {
                actor.ReceiveTimeOutSeconds = 900;
                actor.ConnectionStage = TcpServerActor.Stage.Receiving;
            }
            packetProcessor.QueueRead(actor, packet.ToArray());
        }

        protected override async Task ProcessAsync(GameClient actor, byte[] message)
        {
            // Validate connection
            if (!actor.Socket.Connected)
            {
                return;
            }

            // Read in TQ's binary header
            ushort length = BitConverter.ToUInt16(message, 0);
            PacketType type = (PacketType)BitConverter.ToUInt16(message, 2);

            try
            {
                bool handled = await ModuleManager.OnNetworkMessageReceivedAsync(actor, type, message);
                MsgBase<GameClientBase> msg = null;
                switch (type)
                {
                    case PacketType.MsgRegister: msg = new MsgRegister(); break;
                    case PacketType.MsgTalk: msg = new MsgTalk(); break;
                    case PacketType.MsgItem: msg = new MsgItem(); break;
                    case PacketType.MsgAction: msg = new MsgAction(); break;
                    case PacketType.MsgConnect: msg = new MsgConnect(); break;
                    case PacketType.MsgEscapeKey: msg = new MsgEscapeKey(); break;
                    case PacketType.MsgAction2: msg = new MsgAction2(); break;
                    case PacketType.MsgTick: msg = new MsgTick(); break;
                    case PacketType.MsgName: msg = new MsgName(); break;
                    case PacketType.MsgNpc: msg = new MsgNpc(); break;
                    case PacketType.MsgTaskDialog: msg = new MsgTaskDialog(); break;
                    case PacketType.MsgDataArray: msg = new MsgDataArray(); break;
                    case PacketType.MsgWalkEx: msg = new MsgWalkEx(); break;
                    case PacketType.MsgPackage: msg = new MsgPackage(); break;
                    default:
                        {
                            if (!handled)
                            {
                                logger.Warning("Missing packet {0}, Length {1}\n" + PacketDump.Hex(message), type, length);
                            }
                            return;
                        }
                }

                msg.Decode(message);

                if (actor.Character?.Map != null)
                {
                    Character user = RoleManager.GetUser(actor.Character.Identity);
                    if (user == null || !user.Client.GUID.Equals(actor.GUID))
                    {
                        actor.Disconnect();
                        if (user != null)
                        {
                            await RoleManager.KickOutAsync(actor.Identity);
                        }
                        return;
                    }

                    WorldProcessor.Instance.Queue(actor.Character.Map.Partition, () => msg.ProcessAsync(actor));
                }
                else
                {
                    // we will not send all packets to NO_MAP_GROUP
                    // after this point we are only letting 1052 and first 1010 packet
                    if (type == PacketType.MsgConnect
                        || type == PacketType.MsgRegister
                        || type == PacketType.MsgEscapeKey
                        || type == PacketType.MsgPlayerRestore
                        || (msg is MsgAction action && action.Action != MsgAction.EOActionType.actionJump))
                    {
                        WorldProcessor.Instance.Queue(WorldProcessor.NO_MAP_GROUP, () => msg.ProcessAsync(actor));
                    }
                    else
                    {
                        logger.Error("Message [{0}] sent out of map by client accountId: {1}, authority: {2}, ip: {3}, mac: {4}.", type, actor.AccountIdentity, actor.AuthorityLevel, actor.IpAddress, actor.MacAddress);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on socket process! Message: {0}", ex.Message);
            }
        }

        protected override void Disconnected(GameClient actor)
        {
            if (actor.Creation != null)
            {
                RoleManager.Registration.Remove(actor.Creation.Token);
            }

            if (actor.Character != null)
            {
                WorldProcessor.Instance.Queue(WorldProcessor.NO_MAP_GROUP, () => actor.Character.OnLogoutAsync());
            }
        }
    }
}
