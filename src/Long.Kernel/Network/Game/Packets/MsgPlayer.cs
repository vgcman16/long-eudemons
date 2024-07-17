using Long.Kernel.States;
using Long.Kernel.States.Npcs;
using Long.Kernel.States.User;
using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgPlayer : MsgBase<GameClientBase>
    {
        public MsgPlayer(Character user, Character target = null, ushort x = 0, ushort y = 0)
        {
            Identity = user.Identity;
            Mesh = user.Mesh;
            Status = user.StatusFlag;
            MapX = x == 0 ? user.X : x;
            MapY = y == 0 ? user.Y : y;
            Hairstyle = user.Hairstyle;
            Direction = (byte)user.Direction;
            Pose = (byte)user.Action;
            Level = user.Level;
            Profession = (byte)user.Profession;
            ActionSpeed = user.AdjustSpeed(user.Speed);
            TutorLevel = 0;
            MercenaryLevel = 0;
            Weapon = user.Weapon?.Type ?? 0;
            
            if (user.Sprite != null)
            {
                Armor = user.Sprite.Type;
            }
            else
            {
                Armor = user.Armor?.Type ?? 0;
            }

            Name = user.Name;
        }

        public MsgPlayer(Npc npc)
        {
            IsStatuary = true;
            Identity = npc.Identity;
            Name = npc.DataStr;
            Armor = npc.Task2;
            Weapon = npc.Task3;
            Direction = (byte)(npc.Mesh % 10);
            Hairstyle = (byte)npc.Task1;
            Mesh = npc.Task6;
            MapX = npc.X;
            MapY = npc.Y;
            StatuaryPose = (int)(npc.Task5 + (npc.Mesh % 10));
            StatuaryData = (ushort)npc.Data0;
            StatuaryBattlePower = (ushort)npc.Task4;
            SyndicateIdentity = npc.Task7;
        }

        private bool IsPlayer => Identity >= 1000000;
        private bool IsStatuary { get; set; }
        public uint Identity { get; set; }
        public uint Mesh { get; set; }

        #region Union

        public uint SyndicateIdentity { get; set; }
        public uint OwnerIdentity { get; set; }

        #endregion

        #region Union

        public uint[] Status { get; set; } = new uint[8];

        public ushort StatuaryLife { get; set; }
        public ushort StatuaryFrame { get; set; }
        public int StatuaryPose { get; set; }
        public ushort StatuaryData { get; set; }
        public ushort StatuaryBattlePower { get; set; }
        public int StatuaryData2 { get; set; }

        #endregion

        public uint Armor { get; set; }
        public uint Weapon { get; set; }
        public uint MountId { get; set; }
        public uint MonsterType { get; set; }
        public uint MonsterLevel { get; set; }
        public ushort Life { get; set; }
        public ushort MaxLife { get; set; }
        public byte Level { get; set; }
        public byte Profession { get; set; }
        public ushort MapX { get; set; }
        public ushort MapY { get; set; }
        public ushort Hairstyle { get; set; }
        public byte Direction { get; set; }
        public byte Pose { get; set; }
        public uint FlowerCharm { get; set; }
        public byte ActionSpeed { get; set; }
        public byte TutorLevel { get; set; }
        public byte MercenaryLevel { get; set; }
        public byte NobilityRank { get; set; }
        public uint FamilyId { get; set; }
        public ushort FamilyRank { get; set; }
        public ushort SyndicateRank { get; set; }
        public byte CoachTimes { get; set; }
        public ushort SynDress { get; set; }
        public byte WingLevel { get; set; }
        public ushort Size { get; set; }
        public string Name { get; set; }

        /// <summary>
        ///     Encodes the packet structure defined by this message class into a byte packet
        ///     that can be sent to the client. Invoked automatically by the client's send
        ///     method. Encodes using byte ordering rules interoperable with the game client.
        /// </summary>
        /// <returns>Returns a byte packet of the encoded packet.</returns>
        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgPlayer);
            writer.Write(Identity);                 //4 id probably!
            writer.Write(Mesh);                     //8 mesh
            for (int i = 0; i < Status.Length; i++) //12 status buffer, size default is 4
            {
                writer.Write(Status[i]);
            }

            if (IsPlayer)
            {
                //32 MASK_RANK_SHIFT = 24 MASK_SYNID = 0x00FFFFFF
                writer.Write(SyndicateIdentity);    //28 syndicate id
                writer.Write(Armor);                //32 armor
                writer.Write(Weapon);               //36 weapon
                writer.Write(0);                    //40 
                writer.Write(MountId);              //44 mount identity
            }
            else
            {
                writer.Write(OwnerIdentity);        //28 pet identity
                writer.Write(0);                    //32
                writer.Write(0);                    //36
                writer.Write((uint)Life);           //40
                writer.Write(MonsterLevel);         //44
            }

            writer.Write(MapX);                     //48
            writer.Write(MapY);                     //50
            writer.Write((uint)Hairstyle);          //52
            writer.Write(Direction);                //56
            writer.Write(Pose);                     //57
            writer.Write(Level);                    //58
            writer.Write(Profession);               //59

            if (IsPlayer)
            {
                writer.Write(ActionSpeed);          //60 action speed
                writer.Write(TutorLevel);           //61 tutor level
                writer.Write(MercenaryLevel);       //62 mercenary rank
                writer.Write(NobilityRank);         //63 Nobility rank
            }
            else
            {
                writer.Write(MonsterType);          //60
            }

            writer.Write(0);                        //64 
            writer.Write((ushort)0);                //68
            writer.Write(FlowerCharm);              //70
            writer.Write((ushort)0);                //74
            writer.Write(FamilyId);                 //76
            writer.Write(FamilyRank);               //80
            writer.Write(SyndicateRank);            //82
            writer.Write(CoachTimes);               //84
            writer.Write((byte)0);                  //85
            writer.Write(WingLevel);                //86
            writer.Write(new List<string>           //87 1 4 Name (Amount, Len, Data)
            {
                Name,
            });

            return writer.ToArray();
        }
    }
}