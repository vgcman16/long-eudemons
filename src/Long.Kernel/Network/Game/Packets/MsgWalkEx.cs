using Long.Kernel.Managers;
using Long.Kernel.States;
using Long.Kernel.States.User;
using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgWalkEx : MsgBase<GameClientBase>
    {
        public uint Timestamp { get; set; }
        public uint Identity { get; set; }
        public uint Command { get; set; }
        public ushort CommandX
        {
            get => (ushort)(Command - (CommandY << 16));
            set => Command = (uint)(CommandY << 16 | value);
        }
        public ushort CommandY
        {
            get => (ushort)(Command >> 16);
            set => Command = (uint)(value << 16) | Command;
        }
        public byte Mode { get; set; }
        public byte Direction { get; set; }
        public ushort Param0 { get; set; }
        public uint Param1 { get; set; }

        public override void Decode(byte[] bytes)
        {
            using var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Timestamp = reader.ReadUInt32();
            Identity = reader.ReadUInt32();
            Command = reader.ReadUInt32();
            Direction = reader.ReadByte();
            Mode = reader.ReadByte();
            Param0 = reader.ReadUInt16();
        }

        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgWalkEx);
            writer.Write(Timestamp);
            writer.Write(Identity);
            writer.Write(Command);
            writer.Write(Direction);
            writer.Write(Mode);
            writer.Write(Param0);
            return writer.ToArray();
        }

        public enum RoleMoveMode
        {
            Walk = 0,
            Run,
            Shift,
            Jump,
            Trans,
            Chgmap,
            JumpMagicAttack,
            Collide,
            Synchro,
            Track,
            RunDir0 = 20,
            RunDir7 = 27
        }

        /// <summary>
        ///     Process can be invoked by a packet after decode has been called to structure
        ///     packet fields and properties. For the server implementations, this is called
        ///     in the packet handler after the message has been dequeued from the server's
        ///     <see cref="PacketProcessor{TClient}" />.
        /// </summary>
        /// <param name="client">Client requesting packet processing</param>
        public override async Task ProcessAsync(GameClientBase client)
        {
            // Run is different on eudemons, we have to check the correct coordinantes.
            if (client != null && Identity == client.Character.Identity)
            {
                Character user = client.Character;
                await user.ProcessOnMoveAsync((RoleMoveMode)Mode);
                bool moved = await user.MoveTowardAsync((int)Direction, (int)Mode, false);
                if (moved)
                {
                    await user.SendAsync(this);
                    await user.Screen.UpdateAsync(new MsgWalkEx()
                    {
                        Timestamp = (uint)Environment.TickCount,
                        Identity = user.Identity,
                        CommandX = user.X,
                        CommandY = user.Y,
                        Direction = Direction,
                        Mode = Mode,
                        Param0 = Param0
                    });

                    await user.ProcessAfterMoveAsync();
                }
                return;
            }

            Role target = RoleManager.GetRole(Identity);
            if (target == null)
            {
                return;
            }

            await target.ProcessOnMoveAsync((RoleMoveMode)Mode);
            bool roleMoved = await target.MoveTowardAsync((int)Direction, (int)Mode, false);
            if (roleMoved)
            {
                await target.BroadcastRoomMsgAsync(new MsgWalkEx()
                {
                    Timestamp = (uint)Environment.TickCount,
                    Identity = target.Identity,
                    CommandX = target.X,
                    CommandY = target.Y,
                    Direction = Direction,
                    Mode = Mode,
                    Param0 = Param0
                }, false);

                await target.ProcessAfterMoveAsync();
            }
        }
    }
}
