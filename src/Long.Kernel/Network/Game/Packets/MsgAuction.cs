using Long.Kernel.Managers;
using Long.Kernel.States.Npcs;
using Long.Kernel.States.User;
using Long.Network.Packets;
using Newtonsoft.Json;
using System.Configuration;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgAuction : MsgBase<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgAuction>();

        public MsgAuction()
        {
            Timestamp = Environment.TickCount;
        }

        public uint NpcId { get; set; }
        public uint ItemId { get; set; }
        public uint Bid { get; set; }
        public AuctionType Action { get; set; }
        public int Timestamp { get; set; }
        public string Name { get; set; }

        public override void Decode(byte[] bytes)
        {
            using PacketReader reader = new(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            NpcId = reader.ReadUInt32();
            ItemId = reader.ReadUInt32();
            Action = (AuctionType)reader.ReadInt32();
            Bid = reader.ReadUInt32();
            Timestamp = reader.ReadInt32();
            Name = reader.ReadString();
        }

        public override byte[] Encode()
        {
            using PacketWriter writer = new();
            writer.Write((ushort)PacketType.MsgAuction);
            writer.Write(NpcId);
            writer.Write(ItemId);
            writer.Write((int)Action);
            writer.Write(Bid);
            writer.Write(Timestamp);
            writer.Write(Name);
            return writer.ToArray();
        }

        public enum AuctionType : byte
        {
            AuctionAdd,
            AuctionBid,
            AuctionNew,
            AuctionHintUser,
            AuctionHammer,
            AuctionItemInfo
        }

        public override async Task ProcessAsync(GameClientBase client)
        {
            Character user = client.Character;
            if (user == null)
            {
                return;
            }

            switch (Action)
            {
                case AuctionType.AuctionAdd:
                    {
                        var npc = user.Map.QueryAroundRole(user, NpcId) as BaseNpc;
                        if (npc == null)
                        {
                            return;
                        }

                        if (!npc.IsAuctionNpc())
                        {
                            return;
                        }

                        await AuctionManager.AddNewItemAsync(client.Character, NpcId, ItemId, Bid, 1);
                        break;
                    }

                default:
                    {
                        logger.Warning("MsgAuction>> {0}\n" + JsonConvert.SerializeObject(this), Action);
                        break;
                    }
            }
        }
    }
}
