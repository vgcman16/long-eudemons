using Long.Network.Packets;

namespace Long.Login.Network.Login.Packets
{
    public class MsgPCNum : MsgBase<LoginClient>
    {
        public MsgPCNum() 
        { 
        }

        public uint Data {  get; set; }

        public string Keys { get; set; }

        public override void Decode(byte[] bytes)
        {
            using var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Data = reader.ReadUInt32(); //4
            Keys = reader.ReadString(44).TrimEnd('\0'); // 8
        }

        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgPCNum);
            writer.Write(Data);
            writer.Write(Keys, 44);
            return writer.ToArray();
        }

        public override Task ProcessAsync(LoginClient client)
        {
            // this packet stores the machine connected to the server.
            return Task.CompletedTask;
        }
    }
}
