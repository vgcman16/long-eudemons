using Long.Database.Entities;
using Long.Kernel.Database;
using Long.Kernel.Managers;
using Long.Kernel.Modules.Systems.Syndicate;
using Long.Kernel.Processors;
using Long.Kernel.States;
using Long.Kernel.States.Npcs;
using Long.Kernel.States.User;
using Long.Kernel.States.World;
using Long.Network.Packets;
using Long.Network.Packets.Ai;
using Long.Shared.Helpers;
using System.Drawing;
using static Long.Kernel.Network.Game.Packets.MsgAction;
using static Long.Kernel.Network.Game.Packets.MsgItem;

namespace Long.Kernel.Network.Game.Packets
{
    /// <remarks>Packet Type 1004</remarks>
    /// <summary>
    ///     Message defining a chat message from one player to the other, or from the system
    ///     to a player. Used for all chat systems in the game, including messages outside of
    ///     the game world state, such as during character creation or to tell the client to
    ///     continue logging in after connect.
    /// </summary>
    public sealed class MsgTalk : MsgBase<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgTalk>();
        private static readonly ILogger commandLogger = Logger.CreateConsoleLogger("gm_command");

        #region Constant's

        public const uint SystemLookface = 2962001;
        public const string SYSTEM = "SYSTEM";
        public const string ALLUSERS = "ALLUSERS";

        public static MsgTalk LoginOk { get; } = new(TalkChannel.Login, "ANSWER_OK");
        public static MsgTalk LoginInvalid { get; } = new(TalkChannel.Login, "Invalid login");
        public static MsgTalk LoginNewRole { get; } = new(TalkChannel.Login, "NEW_ROLE");
        public static MsgTalk RegisterOk { get; } = new(TalkChannel.Register, "ANSWER_OK");
        public static MsgTalk RegisterInvalid { get; } = new(TalkChannel.Register, "Invalid request token.");
        public static MsgTalk RegisterInvalidBody { get; } = new(TalkChannel.Register, "Invalid character body.");
        public static MsgTalk RegisterInvalidProfession { get; } = new(TalkChannel.Register, "Invalid character profession.");
        public static MsgTalk RegisterNameTaken { get; } = new(TalkChannel.Register, "Character name taken");
        public static MsgTalk RegisterTryAgain { get; } = new(TalkChannel.Register, "Error, please try later");

        #endregion

        public MsgTalk()
        {
        }

        /// <summary>
        ///     Instantiates a new instance of <see cref="MsgTalk" /> using the recipient's
        ///     character ID, a destination channel, and text to display. By default, sends
        ///     from "SYSTEM" to "ALLUSERS".
        /// </summary>
        /// <param name="characterID">Character's identifier</param>
        /// <param name="channel">Destination channel to send the text on</param>
        /// <param name="text">Text to be displayed in the client</param>
        public MsgTalk(TalkChannel channel, string text)
        {
            Timestamp = Environment.TickCount;
            Color = Color.White;
            Channel = channel;
            Style = TalkStyle.Normal;
            CurrentTime = uint.Parse(DateTime.Now.ToString("HHmm"));
            SenderMesh = SystemLookface;
            SenderName = SYSTEM;
            RecipientMesh = uint.MaxValue;
            RecipientName = ALLUSERS;
            Suffix = string.Empty;
            Message = text;
        }

        /// <summary>
        ///     Instantiates a new instance of <see cref="MsgTalk" /> using the recipient's
        ///     character ID, a destination channel, a text color, and text to display. By
        ///     default, sends from "SYSTEM" to "ALLUSERS".
        /// </summary>
        /// <param name="characterID">Character's identifier</param>
        /// <param name="channel">Destination channel to send the text on</param>
        /// <param name="color">Color text is to be displayed in</param>
        /// <param name="text">Text to be displayed in the client</param>
        public MsgTalk(TalkChannel channel, Color color, string text)
        {
            Timestamp = Environment.TickCount;
            Color = color;
            Channel = channel;
            Style = TalkStyle.Normal;
            CurrentTime = uint.Parse(DateTime.Now.ToString("HHmm"));
            SenderMesh = SystemLookface;
            SenderName = SYSTEM;
            RecipientMesh = uint.MaxValue;
            RecipientName = ALLUSERS;
            Suffix = string.Empty;
            Message = text;
        }

        /// <summary>
        ///     Instantiates a new instance of <see cref="MsgTalk" /> using the recipient's
        ///     character ID, a destination channel, a text color, and text to display. By
        ///     default, sends from "SYSTEM" to "ALLUSERS".
        /// </summary>
        /// <param name="characterID">Character's identifier</param>
        /// <param name="channel">Destination channel to send the text on</param>
        /// <param name="color">Color text is to be displayed in</param>
        /// <param name="text">Text to be displayed in the client</param>
        public MsgTalk(TalkChannel channel, TalkStyle style, Color color, string text)
        {
            Timestamp = Environment.TickCount;
            Color = color;
            Channel = channel;
            Style = TalkStyle.Normal;
            CurrentTime = uint.Parse(DateTime.Now.ToString("HHmm"));
            SenderMesh = SystemLookface;
            SenderName = SYSTEM;
            RecipientMesh = uint.MaxValue;
            RecipientName = ALLUSERS;
            Suffix = string.Empty;
            Message = text;
        }

        /// <summary>
        ///     Instantiates a new instance of <see cref="MsgTalk" /> using the recipient's
        ///     character ID, a destination channel, a text color, sender and recipient's name,
        ///     and text to display.
        /// </summary>
        /// <param name="characterID">Character's identifier</param>
        /// <param name="channel">Destination channel to send the text on</param>
        /// <param name="color">Color text is to be displayed in</param>
        /// <param name="recipient">Name the message displays it is to</param>
        /// <param name="sender">Name the message displays it is from</param>
        /// <param name="text">Text to be displayed in the client</param>
        public MsgTalk(TalkChannel channel, Color color, string recipient, string sender, string text)
        {
            Timestamp = Environment.TickCount;
            Color = color;
            Channel = channel;
            Style = TalkStyle.Normal;
            CurrentTime = uint.Parse(DateTime.Now.ToString("HHmm"));
            SenderMesh = SystemLookface;
            SenderName = sender;
            RecipientMesh = uint.MaxValue;
            RecipientName = recipient;
            Suffix = string.Empty;
            Message = text;
        }

        // Packet Properties
        public int Timestamp { get; set; }
        public Color Color { get; set; }
        public TalkChannel Channel { get; set; }
        public TalkStyle Style { get; set; }
        public uint CurrentTime { get; set; }
        public uint RecipientMesh { get; set; }
        public string RecipientName { get; set; }
        public uint SenderMesh { get; set; }
        public string SenderName { get; set; }
        public string Suffix { get; set; }
        public string Message { get; set; }

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
            Color = Color.FromArgb(reader.ReadInt32());
            Channel = (TalkChannel)reader.ReadUInt16();
            Style = (TalkStyle)reader.ReadUInt16();
            CurrentTime = reader.ReadUInt32();
            RecipientMesh = reader.ReadUInt32();
            SenderMesh = reader.ReadUInt32();
            List<string> strings = reader.ReadStrings();
            if (strings.Count > 3)
            {
                SenderName = strings[0];
                RecipientName = strings[1];
                Suffix = strings[2];
                Message = strings[3];
            }
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
            writer.Write((ushort)PacketType.MsgTalk);
            writer.Write(Color.ToArgb()); // 8
            writer.Write((ushort)Channel); // 12
            writer.Write((ushort)Style); // 14
            writer.Write(CurrentTime); // 20
            writer.Write(RecipientMesh); // 24
            writer.Write(SenderMesh); // 28
            writer.Write(new List<string> // 32
            {
                SenderName,
                RecipientName,
                Suffix,
                Message,
            });
            return writer.ToArray();
        }

        public override async Task ProcessAsync(GameClientBase client)
        {
            Character user = client.Character;
            if (!user.Name.Equals(SenderName))
            {
#if DEBUG
                if (user.IsGm())
                {
                    await user.SendAsync("Invalid sender name????");
                }
#endif
                logger.Warning("[Cheat] User {0} {1} attempted to send message with invalid {2} SenderName.\n{3}", user.Identity, user.Name, SenderName, PacketDump.Hex(Encode()));
                return;
            }

            Character target = RoleManager.GetUser(RecipientName);
            await ServerDbContext.CreateAsync(new DbMessageLog
            {
                SenderIdentity = user.Identity,
                SenderName = user.Name,
                TargetIdentity = target?.Identity ?? 0,
                TargetName = target?.Name ?? RecipientName,
                Channel = (ushort)Channel,
                Message = Message,
                Time = DateTime.Now
            });

            string tempMessage = Message;
            if (tempMessage.StartsWith("#") /*&& user.Gender == 2*/ && tempMessage.Length > 7)
            {
                // let's suppose that the user is with flower charm
                tempMessage = tempMessage[3..^3];
            }

            if (tempMessage.StartsWith("/"))
            {
                string[] splitCommand = tempMessage.Split(' ', 2);
                if (splitCommand.Length > 0)
                {
                    string command = splitCommand[0][1..];
                    string args = string.Empty;
                    if (splitCommand.Length > 1)
                    {
                        args = splitCommand[1];
                    }

                    if (await ExecuteCommandAsync(user, command.ToLower(), args))
                    {
                        commandLogger.Information($"{user.Name} >> {Message}");
                        return;
                    }
                }
            }

            if(tempMessage == "disconnect")
            {
                await user.OnLogoutAsync();
                return;
            }

            if (tempMessage.Contains("updtest0"))
            {
                string[] newArg = tempMessage.Split(' ');
                await user.SendAsync(new MsgUserAttrib(user.Identity, (ClientUpdateType)byte.Parse(newArg[1]), ulong.Parse(newArg[2])));
                return;
            }

            if (tempMessage.Contains("updtest2"))
            {
                string[] newArg = tempMessage.Split(' ');
                await user.AttachStatusAsync(int.Parse(newArg[1]), int.Parse(newArg[2]), 5, 0);
                return;
            }

            switch (Channel)
            {
                case TalkChannel.System:// talk
                    {
                        if (!user.IsAlive)
                        {
                            return;
                        }
                        await user.BroadcastRoomMsgAsync(this, false);
                        break;
                    }

                case TalkChannel.Whisper:
                    {
                        if (target == null)
                        {
                            await user.SendAsync(StrTargetNotOnline, TalkChannel.Talk, Color.White);
                            await user.LeaveWordAsync(RecipientName, Message);
                            return;
                        }

                        SenderMesh = user.Mesh;
                        RecipientMesh = target.Mesh;
                        await target.SendAsync(this);
                        break;
                    }

                //case TalkChannel.Team:
                //    {
                //        if (user.Team == null)
                //        {
                //            return;
                //        }

                //        await user.Team.SendAsync(this, user.Identity);
                //        break;
                //    }

                case TalkChannel.Friend:
                    {
                        if (user.Relation == null)
                        {
                            return;
                        }

                        await user.Relation.SendToFriendsAsync(this);
                        break;
                    }

                //case TalkChannel.Guild:
                //    {
                //        if (user.SyndicateIdentity == 0)
                //        {
                //            return;
                //        }

                //        await user.Syndicate.SendAsync(this, user.Identity);
                //        break;
                //    }

                //case TalkChannel.Family:
                //    {
                //        if (user.FamilyIdentity == 0)
                //        {
                //            return;
                //        }

                //        await user.Family.SendAsync(this, user.Identity);
                //        break;
                //    }

                //case TalkChannel.Ally:
                //    {
                //        if (user.SyndicateIdentity == 0)
                //        {
                //            return;
                //        }

                //        await user.Syndicate.SendAsync(this, user.Identity);
                //        await user.Syndicate.BroadcastToAlliesAsync(this);
                //        break;
                //    }

                //case TalkChannel.Announce:
                //    {
                //        if (user.SyndicateIdentity == 0 || user.SyndicateRank != ISyndicateMember.SyndicateRank.GuildLeader)
                //        {
                //            return;
                //        }

                //        user.Syndicate.Announce = Message[..Math.Min(127, Message.Length)];
                //        user.Syndicate.AnnounceDate = DateTime.Now;
                //        await user.Syndicate.SaveAsync();
                //        await user.SendAsync(this);
                //        break;
                //    }

                //case TalkChannel.FamilyAnnounce:
                //    {
                //        if (user.FamilyIdentity == 0 || user.FamilyRank != Modules.Systems.Family.IFamily.FamilyRank.ClanLeader)
                //        {
                //            return;
                //        }

                //        user.Family.Announcement = Message[..Math.Min(127, Message.Length)];
                //        await user.Family.SaveAsync();
                //        await user.SendAsync(this);
                //        break;
                //    }

                case TalkChannel.Reject:
                    {
                        target = RoleManager.GetUser(RecipientName);
                        if (target == null)
                        {
                            return;
                        }

                        switch (Message.ToLower())
                        {
                            case "a": target.PopRequest(Character.RequestType.Friend); break;
                            case "b": target.PopRequest(Character.RequestType.Trade); break;
                            case "c": target.PopRequest(Character.RequestType.TeamApply); break;
                            case "d": target.PopRequest(Character.RequestType.Guide); break;
                            case "e": target.PopRequest(Character.RequestType.Guide); break;
                        }

                        /* pTarget->SendMsg(this);
                        // ÉÏÃæÊÇ´ÓÐÅÑö¼Ì³Ð¹ýÀ´µÄ´¦Àí·½Ê½
                        // ´æÔÚÒ»¸öBUG£¬¾ÍÊÇÃ»ÓÐÇå³ýCUserµÄÇëÇó£¬ÐèÒªÒÔÏÂÐÞ¸Ä£º
                        if (strcmp(szWords, "a") == 0)  // REJECT_FRIEND
                        {
                            pTarget->FetchApply(CUser::APPLY_FRIEND, pUser->GetID());
                        }
                        else if (strcmp(szWords, "b") == 0) // REJECT_TRADE
                        {
                            pTarget->FetchApply(CUser::APPLY_TRADE, pUser->GetID());
                        }
                        else if (strcmp(szWords, "c") == 0) // REJECT_TEAM
                        {
                            pTarget->FetchApply(CUser::APPLY_TEAMAPPLY, pUser->GetID());
                        }
                        else if (strcmp(szWords, "d") == 0) // REJECT_TEACHERAPPLY
                        {
                            pTarget->FetchApply(CUser::APPLY_TEACHERAPPLY, pUser->GetID());
                        }
                        else if (strcmp(szWords, "e") == 0) // REJECT_STUDENGAPPLY
                        {
                            pTarget->FetchApply(CUser::APPLY_STUDENTAPPLY, pUser->GetID());
                        }*/
                        break;
                    }
                case TalkChannel.Vendor:
                    {
                        if (user.Booth == null)
                        {
                            return;
                        }

                        user.Booth.HawkMessage = Message;
                        await user.BroadcastRoomMsgAsync(this, true);
                        break;
                    }

                default:
                    {
                        logger.Warning("Unhandled {0} talk channel.\n{1}", Channel, PacketDump.Hex(Encode()));
                        break;
                    }
            }
        }

        private async Task<bool> ExecuteCommandAsync(Character user, string command, string arg)
        {
            if (user.IsPm())
            {
                switch (command)
                {
                    case "heal":
                        {
                            await user.SetAttributesAsync(ClientUpdateType.Life, user.MaxLife);
                            await user.SetAttributesAsync(ClientUpdateType.Mana, user.MaxMana);
                            return true;
                        }

                    case "uplev":
                        {
                            if (byte.TryParse(arg, out byte level))
                            {
                                await user.AwardLevelAsync(level);
                            }
                            return true;
                        }

                    case "awardtask":
                        {
                            string[] newArg = arg.Split(' ');
                            if (!uint.TryParse(newArg[0], out var taskId))
                            {
                                return false;
                            }

                            byte completed = 0;
                            if (newArg.Length > 1)
                            {
                                byte.TryParse(newArg[1], out completed);
                            }

                            await user.TaskDetail?.SetCompleteAsync(taskId, completed);
                            return true;
                        }

                    case "pro":
                        {
                            if (byte.TryParse(arg, out byte proProf))
                            {
                                await user.SetAttributesAsync(ClientUpdateType.Class, proProf);
                            }

                            return true;
                        }

                    case "life":
                        {
                            await user.SetAttributesAsync(ClientUpdateType.Life, user.MaxLife);
                            return true;
                        }

                    case "mana":
                        {
                            await user.SetAttributesAsync(ClientUpdateType.Mana, user.MaxMana);
                            return true;
                        }

                    case "updtest":
                        {
                            string[] newArg = arg.Split(' ');
                            await user.SendAsync(new MsgUserAttrib(user.Identity, (ClientUpdateType)byte.Parse(newArg[0]), uint.Parse(newArg[1])));
                            return true;
                        }

                    case "restore":
                        {
                            if (user.IsAlive)
                            {
                                await user.SetAttributesAsync(ClientUpdateType.Life, user.MaxLife);
                                await user.SetAttributesAsync(ClientUpdateType.Mana, user.MaxMana);
                            }
                            else
                            {
                                await user.RebornAsync(false, true);
                            }
                            return true;
                        }

                    case "superman":
                        {
                            await user.SetAttributesAsync(ClientUpdateType.Force, 176);
                            await user.SetAttributesAsync(ClientUpdateType.Speed, 256);
                            await user.SetAttributesAsync(ClientUpdateType.Health, 110);
                            await user.SetAttributesAsync(ClientUpdateType.Spirit, 125);
                            return true;
                        }

                    case "xp":
                        {
                            await user.SetXpAsync(100);
                            await user.BurstXpAsync();
                            return true;
                        }

                    case "sp":
                        {
                            await user.SetAttributesAsync(ClientUpdateType.Energy, user.MaxEnergy);
                            return true;
                        }

                    case "awarditem":
                        {
                            string[] splitParam = arg.Split(' ');
                            if (!uint.TryParse(splitParam[0], out uint idAwardItem))
                            {
                                return true;
                            }

                            int count;
                            if (splitParam.Length < 2 || !int.TryParse(splitParam[1], out count))
                            {
                                count = 1;
                            }

                            DbItemtype itemtype = ItemManager.GetItemtype(idAwardItem);
                            if (itemtype == null)
                            {
                                await user.SendAsync($"[AwardItem] Itemtype {idAwardItem} not found");
                                return true;
                            }

                            await user.UserPackage.AwardItemAsync(idAwardItem, count);
                            await user.SendAsync($"[AwardItem] {itemtype.Name} award success!");
                            return true;
                        }

                    case "awardmoney":
                        {
                            if (int.TryParse(arg, out int moneyAmount))
                            {
                                await user.AwardMoneyAsync(moneyAmount);
                            }

                            return true;
                        }

#if DEBUG
                    case "awardemoney":
                        {
                            if (int.TryParse(arg, out int emoneyAmount))
                            {
                                await user.AwardEudemonPointsAsync(emoneyAmount);
                                await user.SaveEmoneyLogAsync(Character.EmoneyOperationType.AwardCommand, 0, 0, (uint)emoneyAmount);
                            }
                            return true;
                        }

                    case "awardemoneymono":
                        {
                            if (int.TryParse(arg, out int emoneyAmount))
                            {
                                await user.AwardBoundEudemonPointsAsync(emoneyAmount);
                                await user.SaveEmoneyMonoLogAsync(Character.EmoneyOperationType.AwardCommand, 0, 0, (uint)emoneyAmount);
                            }
                            return true;
                        }
#endif

                    case "kickoutall":
                        {
                            await RoleManager.KickOutAllAsync(arg, true);
                            return true;
                        }
                }
            }
            if (user.IsGm())
            {
                switch (command)
                {
                    case "bring":
                        {
                            if (user.MapIdentity == 5000)
                            {
                                await user.SendAsync("You cannot bring players to GM area.");
                                return true;
                            }

                            Character bringTarget = RoleManager.GetUser(arg);
                            if (bringTarget == null && uint.TryParse(arg, out uint idFindTarget))
                            {
                                bringTarget = RoleManager.GetUser(idFindTarget);
                            }

                            if (bringTarget == null)
                            {
                                await user.SendAsync("Target not found");
                                return true;
                            }

                            if ((bringTarget.MapIdentity == 6002 || bringTarget.MapIdentity == 6010) && !user.IsPm())
                            {
                                await user.SendAsync("You cannot move players from jail.");
                                return true;
                            }

                            await bringTarget.FlyMapAsync(user.MapIdentity, user.X, user.Y);
                            return true;
                        }

                    case "cmd":
                        {
                            string[] cmdParams = arg.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                            string subCmd = cmdParams[0];

                            if (command.Length > 1)
                            {
                                string subParam = cmdParams[1];

                                switch (subCmd.ToLower())
                                {
                                    case "broadcast":
                                        await RoleManager.BroadcastWorldMsgAsync(subParam, TalkChannel.Center,
                                            Color.White);
                                        break;

                                    case "gmmsg":
                                        await RoleManager.BroadcastWorldMsgAsync($"{user.Name} says: {subParam}",
                                            TalkChannel.Center, Color.White);
                                        break;

                                    case "player":
                                        if (subParam.Equals("all", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            await user.SendAsync(
                                                $"Players Online: {RoleManager.OnlinePlayers}, Distinct: {RoleManager.OnlineUniquePlayers} (max: {RoleManager.MaxOnlinePlayers})",
                                                TalkChannel.TopLeft, Color.White);
                                        }
                                        else if (subParam.Equals("map", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            await user.SendAsync(
                                                $"Map Online Players: {user.Map.PlayerCount} ({user.Map.Name})",
                                                TalkChannel.TopLeft, Color.White);
                                        }

                                        break;
                                }

                                return true;
                            }

                            return true;
                        }

                    case "chgmap":
                        {
                            string[] chgMapParams = arg.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
                            if (chgMapParams.Length < 3)
                            {
                                return true;
                            }

                            if (uint.TryParse(chgMapParams[0], out uint chgMapId)
                                && ushort.TryParse(chgMapParams[1], out ushort chgMapX)
                                && ushort.TryParse(chgMapParams[2], out ushort chgMapY))
                            {
                                await user.FlyMapAsync(chgMapId, chgMapX, chgMapY);
                            }

                            return true;
                        }

                    case "openui":
                        {
                            if (uint.TryParse(arg, out uint ui))
                            {
                                await user.SendAsync(new MsgAction
                                {
                                    Action = EOActionType.actionPostCmd,
                                    Identity = user.Identity,
                                    Data = ui,
                                    ArgumentX = user.X,
                                    ArgumentY = user.Y
                                });
                            }
                            return true;
                        }

                    case "openwindow":
                        {
                            if (uint.TryParse(arg, out uint window))
                            {
                                await user.SendAsync(new MsgAction
                                {
                                    Action = EOActionType.actionOpenDialog,
                                    Identity = user.Identity,
                                    Data = window,
                                    ArgumentX = user.X,
                                    ArgumentY = user.Y
                                });
                            }

                            return true;
                        }

                    case "kickout":
                        {
                            Character findTarget = RoleManager.GetUser(arg);
                            if (findTarget == null && uint.TryParse(arg, out uint idFindTarget))
                            {
                                findTarget = RoleManager.GetUser(idFindTarget);
                            }

                            if (findTarget == null)
                            {
                                await user.SendAsync("Target not found");
                                return true;
                            }

                            try
                            {
                                findTarget.Client.Disconnect();
                            }
                            catch (Exception ex)
                            {
                                logger.Error(ex, "Error on kickout", ex.Message);
                                WorldProcessor.Instance.Queue(WorldProcessor.NO_MAP_GROUP, () =>
                                {
                                    RoleManager.ForceLogoutUser(findTarget.Identity);
                                    return Task.CompletedTask;
                                });
                            }

                            return true;
                        }

                    case "find":
                        {
                            Character findTarget = RoleManager.GetUser(arg);
                            if (findTarget == null && uint.TryParse(arg, out uint idFindTarget))
                            {
                                findTarget = RoleManager.GetUser(idFindTarget);
                            }

                            if (findTarget == null)
                            {
                                await user.SendAsync("Target not found");
                                return true;
                            }

                            await user.FlyMapAsync(findTarget.MapIdentity, findTarget.X, findTarget.Y);
                            return true;
                        }

                    case "fly":
                        {
                            string[] chgMapParams = arg.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);
                            if (chgMapParams.Length < 1)
                            {
                                await user.SendAsync("/fly mapid: for random position in map", TalkChannel.Talk);
                                await user.SendAsync("/fly x y: for random position in same map aroud coords", TalkChannel.Talk);
                                await user.SendAsync("/fly mapid x y: for random position in map around coords", TalkChannel.Talk);
                                return true;
                            }

                            GameMap gameMap;
                            uint mapId;
                            int x;
                            int y;

                            if (chgMapParams.Length == 1)
                            {
                                mapId = uint.Parse(chgMapParams[0]);
                                gameMap = MapManager.GetMap(mapId);
                                if (gameMap == null)
                                {
                                    await user.SendAsync("Invalid map");
                                    return true;
                                }

                                Point result = await gameMap.QueryRandomPositionAsync();
                                if (result == default)
                                {
                                    await user.SendAsync("Could not find valid position.");
                                    return true;
                                }

                                x = result.X;
                                y = result.Y;
                            }
                            else if (chgMapParams.Length == 2)
                            {
                                mapId = user.MapIdentity;
                                gameMap = user.Map;
                                x = int.Parse(chgMapParams[0]);
                                y = int.Parse(chgMapParams[1]);

                                Point result = await gameMap.QueryRandomPositionAsync(x, y, 18);
                                if (result != default)
                                {
                                    x = result.X;
                                    y = result.Y;
                                }
                            }
                            else
                            {
                                mapId = uint.Parse(chgMapParams[0]);
                                gameMap = MapManager.GetMap(mapId);
                                if (gameMap == null)
                                {
                                    await user.SendAsync("Invalid map");
                                    return true;
                                }

                                x = int.Parse(chgMapParams[1]);
                                y = int.Parse(chgMapParams[2]);

                                Point result = await gameMap.QueryRandomPositionAsync(x, y, 18);
                                if (result != default)
                                {
                                    x = result.X;
                                    y = result.Y;
                                }
                            }

                            if (!gameMap.IsStandEnable(x, y))
                            {
                                await user.SendAsync(StrInvalidCoordinate);
                                return true;
                            }

                            var error = false;
                            List<Role> roleSet = user.Map.Query9Blocks(x, y);
                            foreach (Role role in roleSet)
                            {
                                if (role is BaseNpc npc
                                    && role.X == x && role.Y == y)
                                {
                                    error = true;
                                    break;
                                }
                            }

                            if (!error)
                            {
                                await user.FlyMapAsync(gameMap.Identity, x, y);
                            }
                            else
                            {
                                await user.SendAsync(StrInvalidCoordinate);
                            }

                            return true;
                        }

                    case "/bot":
                        {
                            string[] myParams = arg.Split(new[] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);

                            if (myParams.Length < 2)
                            {
                                await user.SendAsync("/bot [target_name] [reason]", TalkChannel.Talk);
                                return true;
                            }

                            Character target = RoleManager.GetUser(myParams[0]);
                            if (target != null)
                            {
                                await target.SendAsync(StrBotjail);
                                await target.FlyMapAsync(6002, 28, 74);
                                await target.SaveAsync();
                            }
                            return true;
                        }

                    case "/macro":
                        {
                            string[] myParams = arg.Split(new[] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);

                            if (myParams.Length < 2)
                            {
                                await user.SendAsync("/macro [target_name] [reason]", TalkChannel.Talk);
                                return true;
                            }

                            Character target = RoleManager.GetUser(myParams[0]);
                            if (target != null)
                            {
                                await target.SendAsync(StrMacrojail);
                                await target.FlyMapAsync(6010, 28, 74);
                                await target.SaveAsync();
                            }
                            return true;
                        }
                }
            }
            switch (command)
            {
                case "pos":
                    {
                        await user.SendAsync($"MapID[{user.MapIdentity}],Name[{user.Map?.Name}],Pos[{user.X},{user.Y}]", TalkChannel.Talk, Color.White);
                        return true;
                    }
                case "dc":
                case "disconnect":
                    {
                        await RoleManager.KickOutAsync(user.Identity, "/kickout");
                        return true;
                    }
                case "clearinventory":
                    {
                        //if (user.MessageBox != null)
                        //{
                        //    await user.SendAsync(StrClearInventoryCloseBoxes);
                        //    return true;
                        //}

                        //user.MessageBox = new CleanInventoryMessageBox(user);
                        //await user.MessageBox.SendAsync();
                        await user.UserPackage.ClearInventoryAsync();
                        return true;
                    }
            }
            return false;
        }
    }

    /// <summary>
    ///     Enumeration for defining the channel text is printed to. Can also print to
    ///     separate states of the client such as character registration, and can be
    ///     used to change the state of the client or deny a login.
    /// </summary>
    public enum TalkChannel : ushort
    {
        Talk = 2000,
        Whisper,
        Action,
        Team,
        Guild,
        Family = 2006,
        System,
        Yell,
        Friend,
        Center = 2011,
        TopLeft,
        Ghost,
        Service,
        Tip,
        PushUnread,
        Broadcast = 2017,
        World = 2021,
        Auction = 2021,
        Qualifier = 2022,
        Ally = 2025,
        MessageTeam = 2045,
        Register = 2100,
        Login,
        Shop,
        Vendor = 2104,
        Website,
        GuildWarRight1 = 2108,
        GuildWarRight2,
        Offline,
        Announce,
        MessageBox,
        Reject = 2113,
        FamilyAnnounce = 2130,
        TradeBoard = 2201,
        FriendBoard,
        TeamBoard,
        GuildBoard,
        OthersBoard,
        Bbs,
        Monster = 2600
    }

    /// <summary>
    ///     Enumeration type for controlling how text is stylized in the client's chat
    ///     area. By default, text appears and fades overtime. This can be overridden
    ///     with multiple styles, hard-coded into the client.
    /// </summary>
    [Flags]
    public enum TalkStyle : ushort
    {
        Normal = 0,
        Scroll = 1 << 0,
        Flash = 1 << 1,
        Blast = 1 << 2
    }
}
