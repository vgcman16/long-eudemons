using Long.Network.Sockets;
using ProtoBuf;

namespace Long.Network.Packets.Login
{
    public abstract class MsgLoginUserError<TActor>
        : MsgBase<TActor> where TActor : TcpServerActor
    {
        public MsgLoginUserError()
        {
        }

        public uint AccountId { get; set; }

        public override void Decode(byte[] bytes)
        {
            using var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            AccountId = reader.ReadUInt32();
        }

        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgLoginUserError);
            writer.Write(AccountId);
            return writer.ToArray();
        }
    }
}
