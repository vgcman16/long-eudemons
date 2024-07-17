using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgUserAttrib : MsgBase<GameClientBase>
    {
        private readonly List<UserAttribute> Attributes = new();

        public MsgUserAttrib()
        {

        }

        public MsgUserAttrib(uint idRole, ClientUpdateType type, ulong value)
        {
            Identity = idRole;
            Amount++;
            Attributes.Add(new UserAttribute((uint)type, value));
        }

        public MsgUserAttrib(uint idRole, ClientUpdateType type, uint value0, uint value1, uint value3, uint value4)
        {
            Identity = idRole;
            Amount++;
            Attributes.Add(new UserAttribute((uint)type, value0));
            if (type == ClientUpdateType.KeepStatus1)
            {
                if (value1 > 0)
                {
                    Attributes.Add(new UserAttribute((uint)ClientUpdateType.KeepStatus2, value1));
                }

                if (value3 > 0)
                {
                    Attributes.Add(new UserAttribute((uint)ClientUpdateType.KeepStatus3, value3));
                }

                if (value4 > 0)
                {
                    Attributes.Add(new UserAttribute((uint)ClientUpdateType.KeepStatus4, value4));
                }
            }
        }

        public uint Identity { get; set; }
        public int Amount { get; set; }

        public void Append(ClientUpdateType type, ulong data)
        {
            Amount++;
            Attributes.Add(new UserAttribute((uint)type, data));
        }

        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgUserAttrib);
            writer.Write(Identity);
            Amount = Attributes.Count;
            writer.Write(Amount);
            for (var i = 0; i < Amount; i++)
            {
                writer.Write((uint)Attributes[i].Type);
                writer.Write((uint)Attributes[i].Data);
            }

            return writer.ToArray();
        }

        public readonly struct UserAttribute
        {
            public UserAttribute(uint type, ulong data)
            {
                Type = type;
                Data = (uint)data;
            }

            public readonly uint Type;
            public readonly uint Data;
        }
    }

    public enum ClientUpdateType
    {
        Life = 0,
        MaxLife = 1,
        Mana = 2,
        MaxMana = 3,
        Money = 4,
        Experience = 5,
        PkPoints = 6,
        Class = 7,
        Energy = 9,
        MoneySaved = 10,
        Atributes = 11,
        Mesh = 12,
        Level = 13,
        Spirit = 14,
        Health = 15,
        Force = 16,
        Speed = 17,
        HeavensBlessing = 17,
        NobilityMedal = 18,
        Pose = 19,
        CursedTimer = 20,
        Length = 21,
        KoOrder = 22,
        Reborn = 23,
        Expoit = 24,
        MercenaryLevel = 25,
        KeepStatus1 = 26,
        HairStyle = 27,
        Xp = 28,
        MoveSpeed = 29,
        MercenaryExp = 30,
        Fat = 31,
        MentorExp = 32, // value 6 = 1 (multipler of 6)
        MentorLevel = 33, // base is always 1, so we send value - 1
        Dexterity = 34,
        Potential = 35,
        KeepStatus2 = 36,
        MaxEnergy = 37,
        UnknownModelChange = 38,
        MaxEudemon = 39,
        MaxLifePercent = 40,
        MaxEudemonBroodSize = 41,
        EudemonPoints = 46,
        ExperienceIndex = 47,
        PaladinPoints = 48,
        WoodCarry = 50,
        Wood = 52,
        Unknown55 = 55,
        CoachTime = 56,
        CoachExp = 57,
        Wings = 63,
        EudemonPointsBound = 64,
        KeepStatus3 = 70,
        KeepStatus4 = 71,
    }
}
