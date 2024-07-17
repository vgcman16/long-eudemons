using Long.Kernel.States.User;
using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    /// <remarks>Packet Type 1006</remarks>
    /// <summary>
    ///     Message defining character information, used to initialize the client interface
    ///     and game state. Character information is loaded from the game database on login
    ///     if a character exists.
    /// </summary>
    public sealed class MsgUserInfo : MsgBase<GameClientBase>
    {
        /// <summary>
        ///     Instantiates a new instance of <see cref="MsgUserInfo" /> using data fetched
        ///     from the database and stored in <see cref="DbUser" />.
        /// </summary>
        /// <param name="character">Character info from the database</param>
        public MsgUserInfo(Character character)
        {
            Identity = character.Identity;
            Lookface = character.Mesh;
            Hairstyle = character.Hairstyle;
            Silver = character.Silvers;
            Emoney = character.EudemonPoints;
            Experience = character.Experience;
            Strength = character.Strength;
            //Agility = character.Speed;
            Vitality = character.Vitality;
            Spirit = character.Spirit;
            AttributePoints = character.AttributePoints;
            HealthPoints = (ushort)character.Life;
            MaxLife = (ushort)character.MaxLife;
            ManaPoints = (ushort)character.Mana;
            KillPoints = character.PkPoints;
            Level = character.Level;
            Profession = (byte)character.Profession;
            Metempsychosis = character.Metempsychosis;
            EudemonsPointsBound = character.EudemonPointsBound;
            VipLevel = character.VipLevel;
            CharacterName = character.Name;
            SpouseName = character.MateName;
        }

        public uint Identity { get; set; }
        public uint Lookface { get; set; }
        public ushort Hairstyle { get; set; }
        public ulong Silver { get; set; }
        public uint Emoney { get; set; }
        public ulong Experience { get; set; }
        public ushort Strength { get; set; }
        public ushort Agility { get; set; }
        public ushort Vitality { get; set; }
        public ushort Spirit { get; set; }
        public ushort AttributePoints { get; set; }
        public ushort HealthPoints { get; set; }
        public ushort ManaPoints { get; set; }
        public ushort KillPoints { get; set; }
        public byte Level { get; set; }
        public byte Profession { get; set; }
        public byte Metempsychosis { get; set; }
        public byte Nobility { get; set; }
        public uint VipLevel { get; set; }
        public uint EudemonsPointsBound { get; set; }
        public uint Exploits { get; set; }
        public uint BonusPoints { get; set; }
        public ushort MaxLife { get; set; }
        public ushort MaxMana { get; set; }
        public ushort MaxSummons { get; set; }
        public byte BrooderySize { get; set; }
        public byte WingLevel { get; set; }
        public byte GodBrooderySizeLimit { get; set; }
        public byte DemonLevel { get; set; }
        public uint DemonExperience { get; set; }
        public uint DemonExperienceIndex { get; set; }
        public byte GodLevel { get; set; }
        public byte GodStep { get; set; }
        public uint GodExperience { get; set; }
        public uint GodExperienceIndex { get; set; }
        public uint Dodge { get; set; }
        public uint Wood { get; set; }
        public uint PaladinPoints { get; set; }
        public uint CreditUsable { get; set; }
        public uint CreditTotal { get; set; }
        public uint PhysicalAttack { get; set; }
        public ushort MagicAttack { get; set; }
        public byte TutorLevel { get; set; }// level final -1 (cliente sempre faz +1)
        public uint TutorExp { get; set; }// Cada 6 equivale a 1 ponto... entao Valor é um multiplo de 6
        public ushort MercenaryLevel { get; set; }
        public ushort MercenaryExp { get; set; }
        public ushort BattlePower { get; set; }
        public bool AutoAllot { get; set; }
        public uint SoulPoints { get; set; }
        public uint MaxSoulPoints { get; set; }
        public uint MuteFlag { get; set; }
        public uint Business { get; set; }
        public string CharacterName { get; set; }
        public string SpouseName { get; set; }

        /// <summary>
        ///     Encodes the packet structure defined by this message class into a byte packet
        ///     that can be sent to the client. Invoked automatically by the client's send
        ///     method. Encodes using byte ordering rules interoperable with the game client.
        /// </summary>
        /// <returns>Returns a byte packet of the encoded packet.</returns>
        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgUserInfo); // 2
            writer.Write(Identity);           // 4
            writer.Write(Lookface);           // 8
            writer.Write((uint)Hairstyle);    // 12
            writer.Write((uint)Silver);       // 16
            writer.Write(Emoney);             // 20
            writer.Write(Experience);         // 24
            writer.Write(TutorExp);           // 32 TutorExp... 6 = 1
            writer.Write(0);                  // 36
            writer.Write(PhysicalAttack);     // 40 Ataque
            writer.Write(Dodge);              // 44 Esquiva
            writer.Write(Strength);           // 48 Dexterity
            writer.Write(Vitality);           // 50 Speed
            writer.Write(Agility);            // 52 Vitality
            writer.Write(MagicAttack);        // 54 MagicAtk
            writer.Write((short)0);           // 56
            writer.Write(HealthPoints);       // 58 HealthPoints
            writer.Write(MaxLife);            // 60 Max Life
            writer.Write(ManaPoints);         // 62 Mana Points
            writer.Write(SoulPoints);         // 64
            writer.Write(MaxSoulPoints);      // 68
            writer.Write(KillPoints);         // 72
            writer.Write(Level);              // 74
            writer.Write(Profession);         // 75 
            writer.Write((byte)0);            // 76 
            writer.Write((byte)0);            // 77 
            writer.Write(AutoAllot);          // 78 AutoAllot probably!
            writer.Write(TutorLevel);         // 79 
            writer.Write(MercenaryLevel);     // 80
            writer.Write(MaxSummons);         // 82
            writer.Write(0);                  // 84 
            writer.Write(Exploits);           // 88 
            writer.Write(BrooderySize);       // 92
            writer.Seek(44, SeekOrigin.Current);
            writer.Write(VipLevel);           // 140
            writer.Write(Wood);               // 144 
            writer.Write(PaladinPoints);      // 148
            writer.Write(EudemonsPointsBound);// 152 Eudemons Point Bound
            writer.Write(new List<string>     // 156 2 8 Vladimir 6 Nenhum
            {
                CharacterName,
                SpouseName
            });
            return writer.ToArray();
        }
    }
}