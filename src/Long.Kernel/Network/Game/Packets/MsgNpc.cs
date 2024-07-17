using Long.Kernel.Managers;
using Long.Kernel.States.Npcs;
using Long.Kernel.States.User;
using Long.Kernel.States;
using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgNpc : MsgBase<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgNpc>();

        public int Timestamp { get; set; }
        public uint Identity { get; set; }
        public uint Data { get; set; }
        public NpcRequestType RequestType { get; set; }
        public ushort Event { get; set; }

        /// <summary>
        ///     Decodes a byte packet into the packet structure defined by this message class.
        ///     Should be invoked to structure data from the client for processing. Decoding
        ///     follows TQ Digital's byte ordering rules for an all-binary protocol.
        /// </summary>
        /// <param name="bytes">Bytes from the packet processor or client socket</param>
        public override void Decode(byte[] bytes)
        {
            using var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Identity = reader.ReadUInt32();
            Data = reader.ReadUInt32();
            RequestType = (NpcRequestType)reader.ReadUInt16();
            Event = reader.ReadUInt16();
        }

        /// <summary>
        ///     Encodes the packet structure defined by this message class into a byte packet
        ///     that can be sent to the client. Invoked automatically by the client's send
        ///     method. Encodes using byte ordering rules interoperable with the game client.
        /// </summary>
        /// <returns>Returns a byte packet of the encoded packet.</returns>
        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgNpc);
            writer.Write(Identity);
            writer.Write(Data);
            writer.Write((ushort)RequestType);
            writer.Write(Event);
            return writer.ToArray();
        }

        public enum NpcRequestType : ushort
        {
            Activate = 0,           // to server	
            AddNpc = 1,             // no use
            LeaveMap = 2,           // to client		// É¾³ý
            DeleteNpc = 3,          // to server
            ChangePosition = 4,     // to client/server
            LayNpc = 5,             // to client(id=region,data=lookface), answer MsgNpcInfo(CMsgPlayer for statuary)

            CancelInteraction = 255 // to server
        }

        public override async Task ProcessAsync(GameClientBase client)
        {
            Character user = client.Character;
            switch (RequestType)
            {
                case NpcRequestType.Activate:
                    {
#if DEBUG
                        logger.Information($"{user.Identity} activating Npc: {Identity}");
                        await user.SendAsync($"NpcID: {Identity}");
#endif
                        user.ClearTaskId();
                        Role role = user.Map.QueryRole(Identity);
                        if (role == null)
                        {
                            role = RoleManager.GetRole(Identity);
                        }
                        if (role is BaseNpc npc
                            && ((role.MapIdentity == user.MapIdentity || role.Map.Identity == user.Map.BaseMapId)
                                && role.GetDistance(user) <= 18 || role.MapIdentity == 5000))
                        {
                            user.InteractingNpc = npc.Identity;
                            await npc.ActivateNpcAsync(user);
                        }

                        break;
                    }

                case NpcRequestType.CancelInteraction:
                    {
                        user.CancelInteraction();
                        break;
                    }

                default:
                    {
                        logger.Error($"MsgNpc::ProcessAsync() -> invalid request type: {RequestType}");
                        break;
                    }
            }
        }
    }
}
