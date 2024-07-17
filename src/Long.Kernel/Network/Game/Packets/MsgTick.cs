using Long.Kernel.Managers;
using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgTick : MsgBase<GameClientBase>
    {
        public MsgTick()
        {

        }

        public MsgTick(uint roleId) 
        {
            Identity = roleId;
        }

        public uint Identity { get; set; }
        public uint Data { get; set; }
        public uint ChkData { get; set; }

        public override void Decode(byte[] bytes)
        {
            using var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Identity = reader.ReadUInt32();
            Data = reader.ReadUInt32();
            ChkData = reader.ReadUInt32();
        }

        public override byte[] Encode()
        {
            using PacketWriter writer = new();
            writer.Write((ushort)PacketType.MsgTick);
            writer.Write(Identity);
            writer.Write(Data);
            writer.Write(ChkData);
            return writer.ToArray();
        }

        public override Task ProcessAsync(GameClientBase client)
        {
            var user = client.Character;
            if (user == null)
            {
                return Task.CompletedTask;
            }

            uint dwTime = Data ^ (Identity * Identity + 9527);
            return user.ProcessTickAsync(dwTime);
        }
    }
}
