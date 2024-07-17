using Long.Kernel.Scripting.Action;
using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgNpcInfo : MsgBase<GameClientBase>
    {
        public uint Identity { get; set; }
        public ushort PosX { get; set; }
        public ushort PosY { get; set; }
        public ushort Lookface { get; set; }
        public ushort NpcType { get; set; }
        public ushort Sort { get; set; }
        public byte Leng { get; set; }
        public byte Fat { get; set; }
        public uint Data { get; set; }
        public uint OwnerId { get; set; }
        public string Name { get; set; }
        public string Syndicate1 { get; set; }
        public string Syndicate2 { get; set; }

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
            PosX = reader.ReadUInt16();
            PosY = reader.ReadUInt16();
            Lookface = reader.ReadUInt16();
            NpcType = reader.ReadUInt16();
            Leng = reader.ReadByte();
            Fat = reader.ReadByte();
            byte temp = reader.ReadByte();
            List<string> names = reader.ReadStrings();
            if (names.Count > 0)
            {
                Name = names[0];
            }
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
            writer.Write((ushort)PacketType.MsgNpcInfo);
            writer.Write(Identity); // 4
            writer.Write(PosX);     // 8
            writer.Write(PosY);     // 10
            writer.Write((uint)Lookface); // 12
            writer.Write(NpcType);  // 16
            writer.Write(Leng);     // 18
            writer.Write(Fat);      // 19
            writer.Write(Data);
            writer.Write(OwnerId);
            if (!string.IsNullOrEmpty(Name))
            {
                writer.Write(new List<string>()
                {
                    Name,
                    Syndicate1,
                    Syndicate2
                });
            }
            else
            {
                writer.Write(0);
            }

            return writer.ToArray();
        }

        public override Task ProcessAsync(GameClientBase client)
        {
            return GameAction.ExecuteActionAsync(client.Character.InteractingItem, client.Character, null, null,
                $"{PosX} {PosY} {Lookface} {Identity} {NpcType}");
        }
    }
}
