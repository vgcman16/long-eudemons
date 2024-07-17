using Long.Kernel.States.User;
using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgAction2 : MsgBase<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgAction2>();

        public MsgAction2()
        {
        }

        public int TimeStamp { get; set; }
        public int Data1 { get; set; }
        public uint Data2 { get; set; }

        public int Data3 { get; set; }
        public ushort Data3Left
        {
            get => (ushort)(Data3 - (Data3Right << 16));
            set => Data3 = (int)(Data3Right << 16 | value);
        }

        public ushort Data3Right
        {
            get => (ushort)(Data3 >> 16);
            set => Data3 = (int)(value << 16) | Data3;
        }
        public short Data4 { get; set; }
        public Action2Type Action { get; set; }
        public int Data6 { get; set; }
        public List<string> Strings { get; set; }

        public override void Decode(byte[] bytes)
        {
            using PacketReader reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            TimeStamp = reader.ReadInt32();
            Data1 = reader.ReadInt32();
            Data2 = reader.ReadUInt32();
            Data3 = reader.ReadInt32();
            Data4 = reader.ReadInt16();
            Action = (Action2Type)reader.ReadInt16();
            Data6 = reader.ReadInt32();
        }

        public override byte[] Encode()
        {
            using PacketWriter writer = new();
            writer.Write((ushort)PacketType.MsgAction2);
            writer.Write(TimeStamp);    // 4
            writer.Write(Data1);        // 8
            writer.Write(Data2);        // 12
            writer.Write(Data3);        // 16
            writer.Write(Data4);        // 20
            writer.Write((short)Action);// 22
            writer.Write(Data6);        // 24
            return writer.ToArray();
        }

        public enum Action2Type
        {
            RequestExit = 29,
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
                #region Exit

                case Action2Type.RequestExit:
                    {
                        TimeStamp = Environment.TickCount;
                        await client.SendAsync(this);
                        break;
                    }

                #endregion

                default:
                    {
                        logger.Warning($"MsgDataEx()-> Unknown subtype: {Action} \n {PacketDump.Hex(this.Encode())}");
                        await client.SendAsync(this);
                        break;
                    }
            }
        }
    }
}
