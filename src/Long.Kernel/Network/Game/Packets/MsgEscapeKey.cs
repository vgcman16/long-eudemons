using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgEscapeKey : MsgBase<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgEscapeKey>();

        public MsgEscapeKey()
        {
        }

        public int Data0 { get; set; }
        public int Data1 { get; set; }
        public int Data2 { get; set; }

        public override void Decode(byte[] bytes)
        {
            using PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Data0 = reader.ReadUInt16();
            Data1 = reader.ReadUInt16();
            Data2 = reader.ReadUInt16();
        }

        public override byte[] Encode()
        {
            using PacketWriter writer = new();
            writer.Write((ushort)PacketType.MsgEscapeKey);
            writer.Write(Data0);
            writer.Write(Data1);
            writer.Write(Data2);
            return writer.ToArray();
        }

        public override Task ProcessAsync(GameClientBase client)
        {
            return client.SendAsync(this);
        }
    }
}
