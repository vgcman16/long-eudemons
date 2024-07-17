using Long.Kernel.Managers;
using Long.Kernel.Processors;
using Long.Kernel.States;
using Long.Kernel.States.User;
using Long.Kernel.States.World;
using Long.Network.Packets;
using Long.Shared.Helpers;
using System.Drawing;
using static Long.Kernel.States.Role;
using static Long.Kernel.States.User.Character;

namespace Long.Kernel.Network.Game.Packets
{
    /// <remarks>Packet Type 1010</remarks>
    /// <summary>
    ///     Message containing a general action being performed by the client. Commonly used
    ///     as a request-response protocol for question and answer like exchanges. For example,
    ///     walk requests are responded to with an answer as to if the step is legal or not.
    /// </summary>
    public sealed class MsgAction : MsgBase<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgAction>();
        private static readonly ILogger cheatLogger = Logger.CreateLogger("cheat");

        public MsgAction()
        {
            Timestamp = (uint)Environment.TickCount;
        }

        // Packet Properties
        public uint Timestamp { get; set; }
        public uint Identity { get; set; }
        public uint Command { get; set; }
        public uint Argument { get; set; }
        public uint Data { get; set; }
        public EOActionType Action { get; set; }
        public ushort Direction { get; set; }
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

        public ushort ArgumentX
        {
            get => (ushort)(Argument - (ArgumentY << 16));
            set => Argument = (uint)(ArgumentY << 16 | value);
        }

        public ushort ArgumentY
        {
            get => (ushort)(Argument >> 16);
            set => Argument = (uint)(value << 16) | Argument;
        }

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
            Timestamp = reader.ReadUInt32(); // 4
            Identity = reader.ReadUInt32(); // 8
            Command = reader.ReadUInt32(); // 12
            Argument = reader.ReadUInt32(); // 16
            Data = reader.ReadUInt32();// 20
            Action = (EOActionType)reader.ReadUInt16(); // 24
            Direction = reader.ReadUInt16(); // 26
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
            writer.Write((ushort)PacketType.MsgAction);
            writer.Write(Timestamp);                    // 4
            writer.Write(Identity);                     // 8
            writer.Write(Command);                      // 12
            writer.Write(Argument);                     // 16
            writer.Write(Data);                         // 20
            writer.Write((ushort)(Action + 9527));      // 24
            writer.Write(Direction);                    // 26
            return writer.ToArray();
        }

        /// <summary>
        ///     Defines actions that may be requested by the user, or given to by the server.
        ///     Allows for action handling as a packet subtype. Enums should be named by the
        ///     action they provide to a system in the context of the player actor.
        /// </summary>
        public enum EOActionType
        {
            actionNone = 0,
            actionChgDir = 1,
            actionPosition = 2,
            actionEmotion = 3,
            actionBroadcastPos = 4,
            actionDivorce = 5,
            actionSelfUnfreeze = 6,
            actionChgMap = 7,           // ÎÞÐè²ÎÊý
            actionFlyMap = 8,
            actionChgWeather = 9,
            actionFireworks = 10,
            actionDie = 11,
            actionQuitSyn = 12,
            actionWalk = 13,
            actionEnterMap = 14,        // µÇÂ¼µÚ1²½£¬Í¨Öª¿Í»§¶ËµÇÂ¼³É¹¦£¬Íæ¼ÒÒÑ¾­½øÈëµØÍ¼¡£½öÍæ¼Ò! (to client: idUser is Map)
            actionGetItemSet = 15,      // µÇÂ¼µÚ2²½
            actionGetGoodFriend = 16,       // µÇÂ¼µÚ3²½
            actionForward = 17,
            actionLeaveMap = 18,        // ·þÎñÆ÷->¿Í»§¶Ë,idPlayer¡£ÓëCMsgPlayer¶ÔÓ¦£¬±íÊ¾Íæ¼ÒÀë¿ªµØÍ¼£¬ÒªÇóÉ¾³ý¶ÔÏó¡£
            actionJump = 19,
            actionRun = 20,
            actionEquip = 21,
            actionUnequip = 22,
            actionUplev = 23,       // Íæ¼ÒÉý¼¶(Ö»ÏÔÊ¾¶¯»­)
            actionXpCLear = 24,
            actionReborn = 25,
            actionDelRole = 26,
            actionGetWeaponSkillSet = 27,       // µÇÂ¼µÚ4²½
            actionGetMagicSet = 28,     // µÇÂ¼µÚ5²½
            actionSetPkMode = 29,
            actionGetSynAttr = 30,
            actionGhost = 31,       // ±ä¹í
            actionSynchro = 32,     // ×ø±êÍ¬²½¡£send to client self, request client broadcast self coord if no synchro; broadcast to other client, set coord of this player
            actionQueryFriendInfo = 33,     // idTarget = friend id
            actionQueryLeaveWord = 34,
            actionChangeFace = 35,
            //		actionMine				=36,		// ÍÚ¿ó£¬¸ÄÓÃCMsgInteract
            actionTeamMemeberPos = 37,
            actionQueryPlayer = 38,
            actionAbordMagic = 39,
            actionMapARGB = 40,     // to client only
            actionMapStatus = 41,       // to npc server only, idTarget = map id.
            actionQueryTeamMember = 42,
            actionCreateBooth = 43,     // ¿ªÊ¼°ÚÌ¯ to server - unPosX,unPosY: playerpos; unDir:dirofbooth; to client - idTarget:idnpc;
            actionSuspendBooth = 44,        // ÊÕÆð°ÚÌ¯
            actionResumeBooth = 45,     // ¼ÌÐø°ÚÌ¯ to server - unPosX,unPosY: playerpos; unDir:dirofbooth; to client - idTarget:idnpc;
            actionDestroyBooth = 46,        // Í£Ö¹°ÚÌ¯
            actionQueryCryOut = 47,     // ²éÑ¯°ÚÌ¯ßººÈ
            actionPostCmd = 48,     // to client only
            actionQueryEquipment = 49,      // to server //idTargetÎªÐèÒªqueryµÄPlayerID
            actionAbortTransform = 50,      // to server
            actionCombineSubSyn = 51,       // to client, idUser-> idSubSyn, idTarget-> idTargetSyn
                                            //		actionTakeOff			=52,		// to server, wing user take off
            actionGetMoney = 53,        // to client only // ¼ñµ½500ÒÔ¼°500ÒÔÉÏÇ®£¬Ö»´«¸ø×Ô¼º£¬dwDataÎª¼ñµ½µÄÇ®
                                        //		actionCancelKeepBow		=54,		// to server, cancel keep_bow status
            actionQueryEnemyInfo = 55,      // idTarget = enemy id	// to server
            actionMoveStop = 56,        // to target client, data=milli secs.
            actionKickBack = 57,        // to client	idUser is Player ID, unPosX unPosY is Player pos
            actionDropMagic = 58,       // to client only, data is magic type
            actionDropSkill = 59,       // to client only, data is weapon skill type
            actionSoundEffect = 60,     // to client only, client play sound effect, dwData is monster type
            actionQueryMedal = 61,      // to server idUser is Player ID, dwData is medal
            actionDelMedal = 62,        // to server idUser is Player ID, dwData is medal
            actionAddMedal = 63,        // to client idUser is Player ID, dwData is medal
            actionSelectMedal = 64,     // to server idUser is Player ID, dwData is medal
            actionQueryHonorTitle = 65,     // to server idUser is Player ID, dwData is title
            actionDelHonorTitle = 66,       // to server idUser is Player ID, dwData is title
            actionAddHonorTitle = 67,       // to client idUser is Player ID, dwData is title
            actionSelectHonorTitle = 68,        // to server idUser is Player ID, dwData is title
            actionOpenDialog = 69,      // to client only, open a dialog, dwData is id of dialog
            actionFlashStatus = 70,     // broadcast to client only, team member only. dwData is dwStatus
            actionQuerySynInfo = 71,        // to server, return CMsgSynInfo. dwData is target syn id.

            ///////////////
            actionStudentApply = 72,        // Ê¦¸¸ÉêÇëÕÐÊÕÍ½µÜ // to server/client idUserÎªÊ¦¸¸ID dwDataÎªÍ½µÜID
            actionTeacherApply = 73,        // Í½µÜÉêÇë°ÝÊ¦ // to server/client idUserÎªÍ½µÜID dwDataÎªÊ¦¸¸ID
            actionAgreeStudentApply = 74,       // Í¬Òâ°ÝÊ¦ // to server idUserÎªÊ¦¸¸ID dwDataÎªÍ½µÜID
            actionAgreeTeacherApply = 75,       // Í¬ÒâÕÐÊÕÑ§Éú // to server idUserÎªÍ½µÜID dwDataÎªÊ¦¸¸ID
            actionDismissStudent = 76,      // ¿ª³ýÑ§Éú// to server idUserÎªÊ¦¸¸ID dwDataÎªÍ½µÜID
            actionLeaveTeacher = 77,        // ±³ÅÑÊ¦ÃÅ // to server idUserÎªÍ½µÜID dwDataÎªÊ¦¸¸ID
            actionQuerySchoolMember = 78,       // ²éÑ¯Ê¦Í½ÁÐ±í //to server //
            actionTeacherRequest = 79,       // ÔÚÐÂÊÖ¹¤»áÖÐ·¢°ÝÊ¦ÉêÇë

            ////////////////
            // Ó¶±øÈÎÎñÏà¹Ø
            actionQueryPlayerTaskAcceptedList = 80, // ²éÑ¯Íæ¼ÒÐüÉÍµÄÒÑ½ÓÈÎÎñÁÐ±í // to server // dwDataÎªÉÏÒ»´Î²éÑ¯µÄ×îºóÒ»¸öID
            actionQueryPlayerTaskUnacceptedList = 81, // ²éÑ¯Íæ¼ÒÐüÉÍµÄÎ´½ÓÈÎÎñÁÐ±í // to server // dwDataÎªÉÏÒ»´Î²éÑ¯µÄ×îºóÒ»¸öID
                                                      //		actionQueryPlayerTaskCompletedList		=82, // ²éÑ¯Íæ¼ÒÐüÉÍµÄÒÑÍê³ÉÈÎÎñÁÐ±í // to server // dwDataÎªÉÏÒ»´Î²éÑ¯µÄ×îºóÒ»¸öID
            actionQueryPlayerMyTaskList = 83, // ²éÑ¯Íæ¼ÒÐüÉÍµÄÎÒµÄÈÎÎñÁÐ±í // to server // dwDataÎªÉÏÒ»´Î²éÑ¯µÄ×îºóÒ»¸öID
            actionQueryPlayerTaskDetail = 84, // ²éÑ¯Íæ¼ÒÐüÉÍÈÎÎñÏêÏ¸ÄÚÈÝ // to server // dwDataÎª²éÑ¯ÈÎÎñID

            actionAcceptPlayerTask = 88, // ½ÓÊÜÈÎÎñ // to server // dwDataÎªÈÎÎñID
                                         //		actionCompletePlayerTask				=89, // Íê³ÉÈÎÎñ // to server // dwDataÎªÈÎÎñID	// <== ¸ÄÓÃCMsgItem
            actionCancelPlayerTask = 90, // ³·ÏúÈÎÎñ // to server // dwDataÎªÈÎÎñID

            actionLockUser = 91, // Ëø¶¨¿Í»§¶Ë²¢Í¬²½×ø±ê·½Ïò
            actionUnlockUser = 92, // ½âËø¿Í»§¶Ë²¢Í¬²½×ø±ê·½Ïò
            actionMagicTrack = 93, // Í¬²½×ø±ê·½Ïò

            actionQueryStudentsOfStudent = 94, // ²éÑ¯Í½ËïÁÐ±í£¬idTarget = ÐèÒª²éÑ¯Í½ËïÁÐ±íµÄÍ½µÜID

            actionBurstXp = 95,     // XP±¬·¢£¬ÔÊÐíÊ¹ÓÃXP¼¼ÄÜ
            actionTransPos = 96,        // Í¨Öª¿Í»§¶Ë½«Íæ¼ÒÒÆ¶¯µ½Ëæ»úµÄÎ»ÖÃ
            actionDisappear = 97,       // Í¨Öª¿Í»§¶ËÄ¿±ê³¬³öÊÓÏß·¶Î§ÏûÊ§, idUserÎªÄ¿±êID£¬dwData=0±íÊ¾½ÇÉ«£¬dwData=1±íÊ¾ÆäËûMapItem
            actionConfirmExit = 102,

            actionBroadcastWaiting = 171,
            actionBroadcastUserWaiting = 172,

            actionRequestBooth = 180,
            actionRequestBuyBooth = 181,
            actionBoothDeposit = 184,
            actionBoothWithdraw = 185,
            actionQueryBuyPrice = 187,

            actionLoginCompleted = 189,

            // for ai server
            actionSuperChgMap = 200,        // to game server, data=idMap
            actionFullItem = 201,           // to game server, data=itemtype
            actionChangeMapDoc = 202,       // to ai server, idPlayer=idMap, data=idDoc
            actionAddTerrainObj = 203,      // to ai server, idUser=idMap, data=idOwner, npc_id=idTerrainObj
            actionDelTerrainObj = 204,      // to ai server, idUser=idMap, data=idOwner
        };

        public override async Task ProcessAsync(GameClientBase client)
        {
            Character user = client.Character;
            Action = (EOActionType)((ushort)Action - 9527);

            /*
             * Some of the actions are held by the modules, so we ignore them here and bypass the processor.
             */
            if ((Action >= EOActionType.actionStudentApply && Action <= EOActionType.actionLeaveTeacher)
                || (Action >= EOActionType.actionCreateBooth && Action <= EOActionType.actionQueryCryOut)
                || (Action >= EOActionType.actionRequestBooth && Action <= EOActionType.actionQueryBuyPrice))
            {
                return;
            }

            switch (Action)
            {
                case EOActionType.actionEnterMap: // 74
                    {
                        if (user == null)
                        {
                            return;
                        }

                        GameMap targetMap = MapManager.GetMap(user.MapIdentity);
                        if (targetMap == null)
                        {
                            await user.SavePositionAsync(1000, 300, 278);
                            client.Disconnect();
                            return;
                        }

                        Identity = user.MapIdentity;
                        CommandX = user.X;
                        CommandY = user.Y;
                        Data = user.MapIdentity;

                        async Task enterMapPartitionTask()
                        {
                            await user.EnterMapAsync();
                            await user.SendAsync(this);
                        }

                        WorldProcessor.Instance.Queue(targetMap.Partition, enterMapPartitionTask); // sends the current player from Partition 0 the proper partition
                        break;
                    }

                case EOActionType.actionChgMap: // 85
                    {
                        uint idMap = 0;
                        var tgtPos = new Point();
                        var sourcePos = new Point(CommandX, CommandY);
                        bool result = user.Map.GetPassageMap(ref idMap, ref tgtPos, ref sourcePos);

                        if (!result)
                        {
                            user.Map.GetRebornMap(ref idMap, ref tgtPos);
                        }

                        GameMap targetMap = MapManager.GetMap(idMap);
                        if (targetMap.IsRecordDisable())
                        {
                            await user.SavePositionAsync(user.RecordMapIdentity, user.RecordMapX, user.RecordMapY);
                        }

                        await user.FlyMapAsync(idMap, tgtPos.X, tgtPos.Y);
                        break;
                    }

                case EOActionType.actionGetItemSet:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        await user.UserPackage.SendAsync();
                        await user.SendAsync(this);
                        break;
                    }


                case EOActionType.actionGetMagicSet:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        await user.SendAsync(this);
                        break;
                    }

                case EOActionType.actionGetWeaponSkillSet:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        await user.SendAsync(this);
                        break;
                    }

                case EOActionType.actionGetGoodFriend:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        if (user.MateIdentity != 0)
                        {
                            Character mate = RoleManager.GetUser(user.MateIdentity);
                            if (mate != null)
                            {
                                await mate.SendAsync(user.Gender == 1 ? StrMaleMateLogin : StrFemaleMateLogin);
                            }
                        }

                        if (user.Relation != null)
                        {
                            await user.Relation.SendAllFriendAsync();
                            await user.Relation.SendAllEnemyAsync();
                        }

                        await user.SendAsync(this);
                        break;
                    }

                case EOActionType.actionChgDir:
                    {
                        await user.SetDirectionAsync((FacingDirection)(Argument % 8), false);
                        await user.BroadcastRoomMsgAsync(this, true);
                        break;
                    }

                case EOActionType.actionEmotion:
                    {
                        await user.SetActionAsync((EntityAction)Data, true);
                        break;
                    }

                case EOActionType.actionSetPkMode:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        Data = Data % 4;
                        if (!Enum.IsDefined(typeof(PkModeType), (int)Data))
                        {
                            return;
                        }

                        await user.SetPkModeAsync((PkModeType)Data);
                        break;
                    }

                case EOActionType.actionGetSynAttr:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        await user.SendAsync(this);
                        break;
                    }

                case EOActionType.actionQuerySchoolMember:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        await user.SendAsync(this);
                        break;
                    }

                case EOActionType.actionQueryPlayer:
                    {
                        Role targetRole = RoleManager.GetRole(Data);
                        if (targetRole != null)
                        {
                            await targetRole.SendSpawnToAsync(user);
                        }
                        break;
                    }

                case EOActionType.actionQueryLeaveWord:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        await user.LoadLeaveWordAsync();
                        break;
                    }

                case EOActionType.actionConfirmExit:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        await user.SendAsync(this);
                        break;
                    }
                case (EOActionType)108:
                case (EOActionType)109:
                case (EOActionType)111:
                case (EOActionType)113:
                case (EOActionType)349:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        await user.SendAsync(this);
                        break;
                    }

                case (EOActionType)112:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        await user.SendAsync(this);
                        await user.SendAsync(new MsgAction()
                        {
                            Action = (EOActionType)149,
                            Identity = user.Identity,
                        });
                        break;
                    }

                case EOActionType.actionLoginCompleted:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        await user.OnLoginAsync();
                        await OnUserLoginCompleteAsync(user);
                        await user.SendAsync(this);

                        await user.SendAsync(new MsgUserAttrib(user.Identity, ClientUpdateType.MaxMana, user.MaxMana));
                        await user.SetAttributesAsync(ClientUpdateType.MaxEnergy, user.MaxEnergy);
                        await user.SetAttributesAsync(ClientUpdateType.Energy, Character.DEFAULT_USER_ENERGY);
                        break;
                    }

                case EOActionType.actionQueryFriendInfo:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        if (user.Relation != null && user.Relation.IsFriend(Data))
                        {
                            await user.Relation.SendFriendInfoAsync(Data);
                        }
                        break;
                    }

                case EOActionType.actionQueryTeamMember:
                    {
                        // i dont really know what to do here, the client spam alot of times the same packet (really, like 100 times)...
                        // but seriously.... we dont need to spawn anything, cuz the screen itself is already 
                        // displaying members and leader correctly, i think is ok, but i'll let the code here anyway.
                        // its just commented.

                        /*if (user == null)
                        {
                            return;
                        }

                        if (user.Team == null)
                        {
                            return;
                        }

                        await user.Team.SendMemberPosAsync(Data, user);*/
                        break;
                    }

                case EOActionType.actionQueryEnemyInfo:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        if (user.Relation != null && user.Relation.IsEnemy(Data))
                        {
                            await user.Relation.SendEnemyInfoAsync(Data);
                        }
                        break;
                    }

                case EOActionType.actionDelRole:
                    {
                        if (user == null)
                        {
                            return;
                        }

                        //if (user.SecondaryPassword != Command)
                        //{
                        //    await user.SendMenuMessageAsync(StrDeleteInvalidPassword);
                        //    return;
                        //}

                        if (await user.DeleteUserAsync())
                        {
                            await RoleManager.KickOutAsync(user.Identity, "User Delete Role.");
                        }

                        break;
                    }

                case EOActionType.actionJump: // 137
                    {
                        var role = RoleManager.GetRole(Identity);
                        if (role == null)
                        {
                            return;
                        }

                        if (!role.IsAlive)
                        {
                            if (role is Character player)
                            {
                                await player.KickbackAsync();
                                await player.SendAsync(StrDead);
                            }
                            return;
                        }

                        if (role.Map.IsRaceTrack())
                        {
                            if (role is Character player)
                            {
                                await player.KickbackAsync();
                            }
                            return;
                        }

                        ushort newX = (ushort)Command;
                        ushort newY = (ushort)(Command >> 16);

                        if (Identity == user.Identity)
                        {
                            if (!user.IsAlive)
                            {
                                await user.SendAsync(StrDead, TalkChannel.System, Color.Red);
                                return;
                            }

                            if (user.GetDistance(newX, newY) >= 2 * Screen.VIEW_SIZE)
                            {
                                await user.SendAsync(StrInvalidMsg, TalkChannel.System, Color.Red);
                                await RoleManager.KickOutAsync(user.Identity, "big jump");
                                return;
                            }
                        }

                        ArgumentX = user.X;
                        ArgumentY = user.Y;

                        await user.ProcessOnMoveAsync(MsgWalkEx.RoleMoveMode.Jump);
                        bool result = await role.JumpPosAsync(newX, newY);

                        if (result)
                        {
                            CommandX = user.X;
                            CommandY = user.Y;

                            await role.SetDirectionAsync((FacingDirection)(Direction % 8), false);
                            if (role is Character roleUser)
                            {
                                await role.SendAsync(this);
                                await roleUser.Screen.UpdateAsync(this);
                            }
                            else
                            {
                                await role.BroadcastRoomMsgAsync(this, true);
                            }
                            await role.ProcessAfterMoveAsync();
                        }
                        break;
                    }

                default:
                    {
                        logger.Warning($"MsgAction unhandled subtype {Action}. \n {PacketDump.Hex(Encode())}");
                        break;
                    }
            }
        }
    }
}
