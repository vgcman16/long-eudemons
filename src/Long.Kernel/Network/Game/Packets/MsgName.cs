using Long.Kernel.Database;
using Long.Kernel.Database.Repositories;
using Long.Kernel.Managers;
using Long.Kernel.Modules.Systems.Syndicate;
using Long.Kernel.Scripting.Action;
using Long.Kernel.States.User;
using Long.Network.Packets;
using static Long.Kernel.Modules.Systems.Syndicate.ISyndicate;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgName : MsgBase<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgName>();

        public enum StringAction : short
        {
            None = 0,
            Fireworks,
            CreateGuild,
            Guild,
            ChangeTitle,
            DeleteRole = 5,
            Mate,
            QueryNpc,
            Wanted,
            MapEffect,
            RoleEffect = 10,
            MemberList = 11,
            KickoutGuildMember,
            QueryWanted,
            QueryPoliceWanted,
            PoliceWanted = 15,
            QueryMate,
            AddDicePlayer,
            DeleteDicePlayer,
            DiceBonus,
            PlayerWave = 20, //x,y and string
            SetEudemonName = 24,
            LastTalkMsg = 25,// if id!= 999999
            NewBroadcast = 28,
            WhisperWindowInfo = 30, //"%u %d %d %d %d %s %s %s"

            ClearListSyn = 26,
            MemberList2 = 21,
            MemberList3 = 38,// "%u %s %d %d %u %d %u %d %d"

            ReplaceAnnounce = 33,
            ReportAnnounce = 34,
            DeleteAnnounce = 35,

            LoadHotKeys = 214,
            SaveHotKeys = 215,
        }

        public StringAction Action { get; set; }
        public uint Identity { get; set; }
        public ushort X
        {
            get => (ushort)(Identity - (Y << 16));
            set => Identity = (uint)((Y << 16) | value);
        }
        public ushort Y
        {
            get => (ushort)(Identity >> 16);
            set => Identity = (uint)(value << 16) | Identity;
        }
        public List<string> Strings = new();

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
            Action = (StringAction)reader.ReadUInt16();
            Strings = reader.ReadStrings();
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
            writer.Write((ushort)PacketType.MsgName);
            writer.Write(Identity);         // 4
            writer.Write((ushort)Action);   // 8
            writer.Write(Strings);          // 10
            return writer.ToArray();
        }

        public override async Task ProcessAsync(GameClientBase client)
        {
            Character user = client.Character;
            switch (Action)
            {
                #region [Mate|WhisperWindow]

                case StringAction.QueryMate:
                    {
                        var targetUser = RoleManager.GetUser(Identity);
                        if (targetUser == null)
                        {
                            return;
                        }

                        Strings[0] = targetUser.MateName;
                        await client.Character.SendAsync(this);
                        break;
                    }

                case StringAction.WhisperWindowInfo:
                    {
                        if (Strings.Count == 0)
                        {
                            await client.SendAsync(this);
                            return;
                        }

                        var targetUser = RoleManager.GetUser(Strings[0]);
                        if (targetUser == null)
                        {
                            await client.SendAsync(this);
                            return;
                        }

                        Strings.Add($"{targetUser.Identity} {targetUser.Level} {targetUser.BattlePower} # # ");
                        await client.SendAsync(this);
                        break;
                    }

                #endregion

                #region [Hotkeys]
                case StringAction.LoadHotKeys:
                    {
                        Strings.Clear();
                        var keys = await ShortcutKeyRepository.GetAsync(user.Identity);
                        if (keys != null)
                        {
                            Identity = 1;
                            Strings.Add(keys.ItemKey);
                            await user.SendAsync(this);

                            Strings.Clear();
                            Identity = 2;
                            Strings.Add(keys.MagicKey);
                        }

                        await user.SendAsync(this);
                        break;
                    }
                case StringAction.SaveHotKeys:
                    {
                        if (Identity < 1 || Identity > 2 || Strings.Count == 0)
                        {
                            return;
                        }

                        var keys = await ShortcutKeyRepository.GetAsync(user.Identity);
                        if (keys == null)
                        {
                            keys = new Long.Database.Entities.DbShortcutKey()
                            {
                                PlayerId = user.Identity,
                                ItemKey = string.Empty,
                                MagicKey = string.Empty,
                            };
                        }

                        switch (Identity)
                        {
                            case 1: // item
                                keys.ItemKey = Strings[0];
                                break;
                            case 2: // magic
                                keys.MagicKey = Strings[0];
                                break;
                        }

                        if (keys.Identity > 0)
                        {
                            await ServerDbContext.UpdateAsync(keys);
                        }
                        else
                        {
                            await ServerDbContext.CreateAsync(keys);
                        }
                        break;
                    }
                #endregion

                default:
                    {
                        logger.Warning("MsgName:{0} subtype is unhandled", Action);
                        break;
                    }
            }
        }
    }
}
