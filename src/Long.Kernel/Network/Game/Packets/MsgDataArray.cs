using Long.Kernel.States.Items;
using Long.Kernel.States.User;
using Long.Network.Packets;
using Long.Shared.Helpers;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgDataArray : MsgBase<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgDataArray>();
        private static readonly ILogger upgradeItemLevelLogger = Logger.CreateLogger("item_upgrade_level");
        private static readonly ILogger upgradeItemQualityLogger = Logger.CreateLogger("item_upgrade_quality");

        private const int MAX_COMPOSITION_ADDITION = 12;
        private const int MAX_COMPOSITION_PROGRESS = 2_000_000;

        private const int MIN_LEVEL = 70;
        private const int MIN_DONATION = 3_000_000;
        private const int MAX_DONATION = 1_000_000_000;

        private static int[][] TALISMANPROGRESS = new int[][]
        {
            new int[]{ 30, 75, 125, 250, 500, 1000, 2000, 3500, 5000, 7500, 10000, 15000 },
            new int[]{ 120, 200, 400, 800, 1600, 3200, 5600, 10500, 15000, 22500, 30000, 45000 },
            new int[]{ 120, 300, 500, 1000, 2000, 4000, 8000, 14000, 20000, 30000, 40000, 60000 },
            new int[]{ 20, 60, 180 },
            new int[]{ 20, 50, 125, 250, 500, 1000, 2000, 4000, 7500, 12500, 20000, 30000, 45000 },
            new int[]{ 80, 200, 400, 1600, 3200, 6400, 12000, 22500, 37500, 60000, 90000, 135000 },
            new int[]{ 80, 200, 500, 1000, 2000, 4000, 8000, 16000, 30000, 50000, 80000, 120000, 180000 },
        };

        public DataArrayMode Action { get; set; }
        public byte Amount { get; set; }
        public byte[] Param0 { get; set; }
        public sbyte[] Param1 { get; set; }

        public ushort[] Param2 { get; set; }
        public short[] Param3 { get; set; }

        public uint[] Param4 { get; set; }
        public int[] Param5 { get; set; }

        public ulong[] Param6 { get; set; }
        public long[] Param7 { get; set; }

        public override void Decode(byte[] bytes)
        {
            PacketReader reader = new(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Action = (DataArrayMode)reader.ReadByte();
            Amount = reader.ReadByte();
            if (Action == DataArrayMode.ComposeItemNormal// normal compose
                || Action == DataArrayMode.UpItemQuality// redstone
                || Action == DataArrayMode.UpItemLevel//violetstone
                || Action == DataArrayMode.ComposeItemHigher//+9 to +12
                || Action == DataArrayMode.TakeInGem//embed
                || Action == DataArrayMode.TakeOutGem//takeout
                || Action == DataArrayMode.ComposeTalisman)// talisman
            {
                reader.ReadUInt16();
                Param4 = new uint[Amount];
                for (int i = 0; i < Amount; i++)
                {
                    Param4[i] = reader.ReadUInt32();
                }
            }
            else if (Action == DataArrayMode.DonateMoney ||
                Action == DataArrayMode.DonateEmoney)
            {
                reader.ReadUInt16();
                Param4 = new uint[Amount];
                for (int i = 0; i < Amount; i++)
                {
                    Param4[i] = reader.ReadUInt32();
                }
            }
        }

        public override byte[] Encode()
        {
            PacketWriter writer = new();
            writer.Write((ushort)PacketType.MsgDataArray);
            writer.Write((byte)Action);
            if (Param0 != null)
            {
                writer.Write(Amount = (byte)Param0.Length);
                writer.Write((ushort)0);
                for (int i = 0; i < Param0.Length; i++)
                {
                    writer.Write(Param0[i]);
                }
            }
            else if (Param1 != null)
            {
                writer.Write(Amount = (byte)Param1.Length);
                writer.Write((ushort)0);
                for (int i = 1; i < Param1.Length; i++)
                {
                    writer.Write(Param1[i]);
                }
            }
            else if (Param2 != null)
            {
                writer.Write(Amount = (byte)Param2.Length);
                writer.Write((ushort)0);
                for (int i = 0; i < Param2.Length; i++)
                {
                    writer.Write(Param2[i]);
                }
            }
            else if (Param3 != null)
            {
                writer.Write(Amount = (byte)Param3.Length);
                writer.Write((ushort)0);
                for (int i = 0; i < Param3.Length; i++)
                {
                    writer.Write(Param3[i]);
                }
            }
            else if (Param4 != null)
            {
                writer.Write(Amount = (byte)Param4.Length);
                writer.Write((ushort)0);
                for (int i = 0; i < Param4.Length; i++)
                {
                    writer.Write(Param4[i]);
                }
            }
            else if (Param5 != null)
            {
                writer.Write(Amount = (byte)Param5.Length);
                writer.Write((ushort)0);
                for (int i = 0; i < Param5.Length; i++)
                {
                    writer.Write(Param5[i]);
                }
            }
            else if (Param6 != null)
            {
                writer.Write(Amount = (byte)Param6.Length);
                writer.Write((ushort)0);
                for (int i = 0; i < Param6.Length; i++)
                {
                    writer.Write(Param6[i]);
                }
            }
            else if (Param7 != null)
            {
                writer.Write(Amount = (byte)Param7.Length);
                writer.Write((ushort)0);
                for (int i = 0; i < Param7.Length; i++)
                {
                    writer.Write(Param7[i]);
                }
            }

            return writer.ToArray();
        }

        public enum DataArrayMode : ushort
        {
            UpItemQuality = 2,
            ComposeItemNormal = 3,
            UpItemLevel = 4,
            ComposeItemHigher = 6,
            ComposeTalisman = 9,
            SetRebuildCondition = 73, // 6 data values.
            SetRebuildDice = 74, // 7 data values
            OfflineTGWindow = 75, // set on hook time,
            //85 seems to do nearly same thing as 75, but send other screen.
            TakeInGem = 76,
            TakeOutGem = 77,
            DonateMoney = 211,
            DonateEmoney = 214,
            DonationRankRequire = 215,
            RequestExit = 217,
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

                case DataArrayMode.RequestExit:
                    {
                        await client.SendAsync(this);
                        break;
                    }

                #endregion

                default:
                    {
                        logger.Warning("Invalid MsgDataArray Action: {0}. {1},{2},{3},{4}[{5}],{6},{7}", Action, user.Identity, user.Name, user.Level, user.MapIdentity, user.Map?.Name, user.X, user.Y);
                        break;
                    }
            }
        }

        #region Static Getters

        public static int GetStoneProgress(uint type, byte plus = 0)
        {
            if (type >= 1037231 && type <= 1037233)
            {
                return TALISMANPROGRESS[3][type % 10];
            }
            else
            {
                return TALISMANPROGRESS[4 + ((type % 1000) / 100)][plus];
            }
        }

        public static int GetProgressRequired(uint type, byte level)
        {
            if (level >= 12)
            {
                return 0;
            }

            if (type < 1110010 || type > 1110210)
            {
                return 0;
            }

            return TALISMANPROGRESS[(type % 1000) / 100][level];
        }

        #endregion
    }
}
