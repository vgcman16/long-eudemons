using Long.Database.Entities;
using Long.Kernel.Managers;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States;
using Long.Kernel.States.Items;
using Long.Kernel.States.Npcs;
using Long.Kernel.States.User;
using Long.Shared.Helpers;
using Newtonsoft.Json;
using System.Drawing;

namespace Long.Kernel.Scripting.Action
{
    public partial class GameAction
    {
        private static readonly ILogger logger = Log.ForContext<GameAction>();
        private static readonly ILogger actionLogger = Logger.CreateLogger("action_log");
        private static readonly ILogger missingLogger = Logger.CreateConsoleLogger("missing_action_types");
        private static readonly ILogger missingAction = Logger.CreateConsoleLogger("missing_action");

        public static async Task<bool> ExecuteActionAsync(uint idAction, Character user, Role role, ItemBase item, params string[] inputs)
        {
            const int _MAX_ACTION_I = 64;
            const int _DEADLOCK_CHECK_I = 5;

            if (idAction == 0)
            {
                return false;
            }

            int actionCount = 0;
            int deadLookCount = 0;
            uint idNext = idAction, idOld = idAction;

            while (idNext > 0)
            {
                if (actionCount++ > _MAX_ACTION_I)
                {
                    logger.Error("Error: too many game action, from: {0}, last action: {1}", idAction, idNext);
                    return false;
                }

                if (idAction == idOld && deadLookCount++ >= _DEADLOCK_CHECK_I)
                {
                    logger.Error("Error: dead loop detected, from: {0}, last action: {1}", idAction, idNext);
                    return false;
                }

                if (idNext != idOld)
                {
                    deadLookCount = 0;
                }

                DbAction action = ScriptManager.GetAction(idNext);
                if (action == null)
                {
                    missingAction.Error($"Missinc Action[{idNext}],{FormatLogString(action, null, user, role, item, inputs)}");
                    return false;
                }

                string param = await FormatParamAsync(action, user, role, item, inputs);
                if (user?.IsPm() == true)
                {
                    await user.SendAsync($"{action.Id}: [{action.IdNext},{action.IdNextfail}]. type[{action.Type}], data[{action.Data}], param:[{param}].",
                        TalkChannel.Action,
                        Color.White);
                }

                bool result = false;
                TaskActionType actionType = (TaskActionType)action.Type;
                if (actionType >= TaskActionType.ActionSysFirst && actionType <= TaskActionType.ActionSysLimit)
                {
                    result = await ProcessActionSysAsync(action, param, user, role, item, inputs);
                }
                else
                {
                    missingLogger.Warning($"Missing type {FormatLogString(action, param, user, role, item, inputs)}");
                }

                idOld = idAction;
                idNext = result ? action.IdNext : action.IdNextfail;
            }
            return true;
        }

        #region Unknown Category
        #endregion

        private static string FormatLogString(DbAction action, string param, Character user, Role role, ItemBase item, string[] input)
        {
            List<string> append = new()
            {
                $"ActionType:{action?.Type}"
            };
            if (user != null)
            {
                append.Add($"User:{user.Identity},{user.Name}");
            }
            if (role != null)
            {
                append.Add($"Role:{role.Identity},{role.Name}");
            }
            if (item != null)
            {
                append.Add($"(Item: type {item.Type},{item.Name};id {item.Identity})");
            }
            if (action != null)
            {
                return $"[{string.Join(',', append.ToArray())}] {action.Id}: [{action.IdNext},{action.IdNextfail}]. type[{action.Type}], data[{action.Data}], param:[{param ?? action.Param}][input:{JsonConvert.SerializeObject(input)}].";
            }
            return $"[{string.Join(',', append.ToArray())}]";
        }

        private static async Task<string> FormatParamAsync(DbAction action, Character user, Role role, ItemBase item, string[] input)
        {
            string result = action.Param;

            result = result.Replace("%user_name", user?.Name ?? StrNone)
                .Replace("%user_id", user?.Identity.ToString() ?? "0")
                .Replace("%user_lev", user?.Level.ToString() ?? "0")
                .Replace("%user_mete", user?.Metempsychosis.ToString() ?? "0")
                .Replace("%user_meto", user?.Metempsychosis.ToString() ?? "0")
                .Replace("%user_mate", user?.MateName ?? StrNone)
                .Replace("%user_pro", user?.Profession.ToString() ?? "0")
                .Replace("%user_map_id", user?.Map?.Identity.ToString() ?? "0")
                .Replace("%user_map_name", user?.Map?.Name ?? StrNone)
                .Replace("%user_map_x", user?.X.ToString() ?? "0")
                .Replace("%user_map_y", user?.Y.ToString() ?? "0")
                .Replace("%map_owner_id", user?.Map?.OwnerIdentity.ToString() ?? "0")
                //.Replace("%user_nobility_rank", ((int)(user?.Nobility?.Rank ?? 0)).ToString())
                //.Replace("%user_nobility_position", user?.Nobility?.Position.ToString() ?? "0")
                .Replace("%account_id", user?.Client?.AccountIdentity.ToString() ?? "0")
                .Replace("%map_owner_id", user?.Map?.OwnerIdentity.ToString() ?? "0")
                .Replace("%last_add_item_id", user?.LastAddItemIdentity.ToString() ?? "0")
                //.Replace("%businessman_days", $"{user?.BusinessManDays ?? 0}")
                .Replace("%user_vip", user?.VipLevel.ToString())
                .Replace("%user_home_id", user?.HomeIdentity.ToString() ?? "0");

            for (int i = 0; i < Math.Min(input.Length, 8); i++)
            {
                result = result.Replace($"%accept{i}", input[i]);
            }

            if (user != null)
            {
                while (result.Contains("%iter_var"))
                {
                    for (int i = Role.MAX_VAR_AMOUNT - 1; i >= 0; i--)
                    {
                        result = result.Replace($"%iter_var_data{i}", user.VarData[i].ToString());
                        result = result.Replace($"%iter_var_str{i}", user.VarString[i]);
                    }
                }

                while (result.Contains("%taskdata"))
                {
                    int start = result.IndexOf("%taskdata(", StringComparison.InvariantCultureIgnoreCase);
                    string taskId = "", taskDataIdx = "";
                    bool comma = false;
                    for (int i = start + 10; i < result.Length; i++)
                    {
                        if (!comma)
                        {
                            if (result[i] == ',')
                            {
                                comma = true;
                                continue;
                            }

                            taskId += result[i];
                        }
                        else
                        {
                            if (result[i] == ')')
                            {
                                break;
                            }

                            taskDataIdx += result[i];
                        }
                    }

                    uint.TryParse(taskId, out var evt);
                    int.TryParse(taskDataIdx, out var idx);

                    int value = user.TaskDetail?.GetData(evt, $"data{idx}") ?? 0;
                    result = ReplaceFirst(result, $"%taskdata({evt},{idx})", value.ToString());
                }

                while (result.Contains("%emoney_card1"))
                {
                    int start = result.IndexOf("%emoney_card1(", StringComparison.InvariantCultureIgnoreCase);
                    string cardTypeString = "";
                    for (int i = start + 14; i < result.Length; i++)
                    {
                        if (result[i] == ')')
                        {
                            break;
                        }

                        cardTypeString += result[i];
                    }

                    uint.TryParse(cardTypeString, out var cardType);
                    result = ReplaceFirst(result, $"%emoney_card1({cardType})", "0");
                }

                while (result.Contains("%emoney_card2"))
                {
                    result = ReplaceFirst(result, $"%emoney_card2", "0");
                }
            }

            if (role != null)
            {
                if (role is BaseNpc npc)
                {
                    result = result.Replace("%data0", npc.GetData("data0").ToString())
                        .Replace("%data1", npc.GetData("data1").ToString())
                        .Replace("%data2", npc.GetData("data2").ToString())
                        .Replace("%data3", npc.GetData("data3").ToString())
                        .Replace("%npc_ownerid", npc.OwnerIdentity.ToString())
                        .Replace("%map_owner_id", role.Map.OwnerIdentity.ToString() ?? "0")
                        .Replace("%id", npc.Identity.ToString())
                        .Replace("%npc_x", npc.X.ToString())
                        .Replace("%npc_y", npc.Y.ToString());
                }

                result = result.Replace("%map_owner_id", role.Map?.OwnerIdentity.ToString());
            }

            if (item != null)
            {
                result = result.Replace("%item_data", item.Identity.ToString())
                    .Replace("%item_name", item.Name)
                    .Replace("%item_type", item.Type.ToString())
                    .Replace("%item_id", item.Identity.ToString());
            }

            while (result.Contains("%random"))
            {
                int start = result.IndexOf("%random(", StringComparison.InvariantCultureIgnoreCase);
                string rateStr = "";
                for (int i = start + 8; i < result.Length; i++)
                {
                    if (result[i] == ')')
                    {
                        break;
                    }
                    rateStr += result[i];
                }

                int rate = int.Parse(rateStr);
                result = ReplaceFirst(result, $"%random({rateStr})", (await NextAsync(rate)).ToString());
            }

            while (result.Contains("%global_dyna_data_str"))
            {
                int start = result.IndexOf("%global_dyna_data_str(", StringComparison.InvariantCultureIgnoreCase);
                string strEvent = "", strNum = "";
                bool comma = false;
                for (int i = start + 21; i < result.Length; i++)
                {
                    if (!comma)
                    {
                        if (result[i] == ',')
                        {
                            comma = true;
                            continue;
                        }

                        strEvent += result[i];
                    }
                    else
                    {
                        if (result[i] == ')')
                        {
                            break;
                        }

                        strNum += result[i];
                    }
                }

                uint.TryParse(strEvent, out var evt);
                int.TryParse(strNum, out var idx);

                var data = await DynamicGlobalDataManager.GetAsync(evt);
                string value = DynamicGlobalDataManager.GetStringData(data, idx);
                result = ReplaceFirst(result, $"%global_dyna_data_str({evt},{idx})", value.ToString());
            }

            while (result.Contains("%global_dyna_data"))
            {
                int start = result.IndexOf("%global_dyna_data(", StringComparison.InvariantCultureIgnoreCase);
                string strEvent = "", strNum = "";
                bool comma = false;
                for (int i = start + 18; i < result.Length; i++)
                {
                    if (!comma)
                    {
                        if (result[i] == ',')
                        {
                            comma = true;
                            continue;
                        }

                        strEvent += result[i];
                    }
                    else
                    {
                        if (result[i] == ')')
                        {
                            break;
                        }

                        strNum += result[i];
                    }
                }

                uint.TryParse(strEvent, out var evt);
                int.TryParse(strNum, out var idx);

                var data = await DynamicGlobalDataManager.GetAsync(evt);
                long value = DynamicGlobalDataManager.GetData(data, idx);
                result = ReplaceFirst(result, $"%global_dyna_data({evt},{idx})", value.ToString());
            }

            while (result.Contains("%sysdatastr"))
            {
                int start = result.IndexOf("%sysdatastr(", StringComparison.InvariantCultureIgnoreCase);
                string strEvent = "", strNum = "";
                bool comma = false;
                for (int i = start + 12; i < result.Length; i++)
                {
                    if (!comma)
                    {
                        if (result[i] == ',')
                        {
                            comma = true;
                            continue;
                        }

                        strEvent += result[i];
                    }
                    else
                    {
                        if (result[i] == ')')
                        {
                            break;
                        }

                        strNum += result[i];
                    }
                }

                uint.TryParse(strEvent, out var evt);
                int.TryParse(strNum, out var idx);

                var data = await DynamicGlobalDataManager.GetAsync(evt);
                string value = DynamicGlobalDataManager.GetStringData(data, idx);
                result = ReplaceFirst(result, $"%sysdatastr({evt},{idx})", value);
            }

            while (result.Contains("%sysdata"))
            {
                int start = result.IndexOf("%sysdata(", StringComparison.InvariantCultureIgnoreCase);
                string strEvent = "", strNum = "";
                bool comma = false;
                for (int i = start + 9; i < result.Length; i++)
                {
                    if (!comma)
                    {
                        if (result[i] == ',')
                        {
                            comma = true;
                            continue;
                        }

                        strEvent += result[i];
                    }
                    else
                    {
                        if (result[i] == ')')
                        {
                            break;
                        }

                        strNum += result[i];
                    }
                }

                uint.TryParse(strEvent, out var evt);
                int.TryParse(strNum, out var idx);

                var data = await DynamicGlobalDataManager.GetAsync(evt);
                long value = DynamicGlobalDataManager.GetData(data, idx);
                result = ReplaceFirst(result, $"%sysdata({evt},{idx})", value.ToString());
            }

            //if (result.Contains("%iter_upquality_gem"))
            //{
            //    Item pItem = user?.GetEquipment((Item.ItemPosition)user.Iterator);
            //    if (pItem != null)
            //    {
            //        result = result.Replace("%iter_upquality_gem", pItem.GetUpQualityGemAmount().ToString());
            //    }
            //    else
            //    {
            //        result = result.Replace("%iter_upquality_gem", "0");
            //    }
            //}

            if (result.Contains("%iter_itembound"))
            {
                ItemBase pItem = user?.GetEquipment((ItemBase.ItemPosition)user.Iterator);
                if (pItem != null)
                {
                    result = result.Replace("%iter_itembound", pItem.IsBound ? "1" : "0");
                }
                else
                {
                    result = result.Replace("%iter_itembound", "0");
                }
            }

            //if (result.Contains("%iter_uplevel_gem"))
            //{
            //    Item pItem = user?.GetEquipment((Item.ItemPosition)user.Iterator);
            //    if (pItem != null)
            //    {
            //        result = result.Replace("%iter_uplevel_gem", pItem.GetUpgradeGemAmount().ToString());
            //    }
            //    else
            //    {
            //        result = result.Replace("%iter_uplevel_gem", "0");
            //    }
            //}

            result = result.Replace("%map_name", user?.Map?.Name ?? role?.Map?.Name ?? StrNone)
                .Replace("%iter_time", UnixTimestamp.Now.ToString())
                .Replace("%%", "%")
                .Replace("%last_del_item_id", user?.LastDelItemIdentity.ToString() ?? "0");
            return result;
        }

        private static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return string.Concat(text.AsSpan(0, pos), replace, text.AsSpan(pos + search.Length));
        }

        public static string[] SplitParam(string param, int count = 0)
        {
            return count > 0
                ? param.Split(new[] { ' ' }, count, StringSplitOptions.RemoveEmptyEntries)
                : param.Split(' ');
        }

        private static string GetParenthesys(string szParam)
        {
            int varIdx = szParam.IndexOf("(", StringComparison.CurrentCulture) + 1;
            int endIdx = szParam.IndexOf(")", StringComparison.CurrentCulture);
            return szParam[varIdx..endIdx];
        }

        private static byte VarId(string szParam)
        {
            int start = szParam.IndexOf("%var(", StringComparison.InvariantCultureIgnoreCase);
            string rateStr = "";
            for (int i = start + 5; i < szParam.Length; i++)
            {
                if (szParam[i] == ')')
                {
                    break;
                }
                rateStr += szParam[i];
            }
            return byte.Parse(rateStr);
        }

        public enum TaskActionType
        {
            // System
            ActionSysFirst = 100,
            ActionMenutext = 101,
            ActionMenulink = 102,
            ActionMenuedit = 103,
            ActionMenupic = 104,
            ActionMenuMessage = 105,
            ActionMenubutton = 110,
            ActionMenulistpart = 111,
            ActionMenuTaskClear = 113,
            ActionMenucreate = 120,
            ActionRand = 121,
            ActionRandaction = 122,
            ActionChktime = 123,
            ActionPostcmd = 124,
            ActionBrocastmsg = 125,
            ActionMessageBox = 126,
            ActionSysExecAction = 126,
            ActionExecutequery = 127,
            ActionSysDoSomethingUnknown = 128,
            ActionSysInviteFilter = 129,
            ActionSysUpdate = 130,
            ActionSysUpdateCreate = 131,
            ActionVipFunctionCheck = 144, // data is flag data << 1UL
            ActionDynaGlobalData = 150,
            ActionSysLimit = 199,

            //NPC
            ActionNpcFirst = 200,
            ActionNpcAttr = 201,
            ActionNpcErase = 205,
            ActionNpcModify = 206,
            ActionNpcResetsynowner = 207,
            ActionNpcFindNextTable = 208,
            ActionNpcFamilyCreate = 218,
            ActionNpcFamilyDestroy = 219,
            ActionNpcFamilyChangeName = 220,
            ActionNpcLegionWarRepair = 221,
            ActionNpcChangePos = 223,
            ActionNpcLegionWarSynOwnerChk = 225,//nao sei se e isso, mas.
            ActionNpcLegionWarChk = 226,
            ActionNpcRegistration = 230,
            ActionNpcLimit = 299,

            // Map
            ActionMapFirst = 300,
            ActionMapMovenpc = 301,
            ActionMapMapuser = 302,
            ActionMapBrocastmsg = 303,
            ActionMapDropitem = 304,
            ActionMapSetstatus = 305,
            ActionMapAttrib = 306,
            ActionMapRegionMonster = 307,
            ActionMapDropMultiItems = 308,
            ActionMapChangeweather = 310,
            ActionMapChangelight = 311,
            ActionMapMapeffect = 312,
            ActionMapFireworks = 314,
            ActionMapFireworks2 = 315,
            ActionMapAbleExp = 332,
            ActionMapLimit = 399,

            // Lay item
            ActionItemonlyFirst = 400,
            ActionItemRequestlaynpc = 401,
            ActionItemCountnpc = 402,
            ActionItemLaynpc = 403,
            ActionItemDelthis = 498,
            ActionItemonlyLimit = 499,

            // Item
            ActionItemFirst = 500,
            ActionItemAdd = 501,
            ActionItemDel = 502,
            ActionItemCheck = 503,
            ActionItemHole = 504,
            ActionItemRepair = 505,
            ActionItemMultidel = 506,
            ActionItemMultichk = 507,
            ActionItemLeavespace = 508,
            ActionItemUpequipment = 509,
            ActionItemEquiptest = 510,
            ActionItemEquipexist = 511,
            ActionItemEquipcolor = 512,
            ActionItemTransform = 513,
            ActionItemCheckrand = 516,
            ActionItemModify = 517,
            ActionItemAdd1 = 518,
            ActionItemDelAll = 519,
            ActionItemJarCreate = 528,
            ActionItemJarVerify = 529,
            ActionItemUnequip = 530,
            ActionItemRefineryAdd = 532,
            ActionItemAdd2 = 542,
            ActionItemCheck2 = 543,
            ActionItemSuperFlag = 544,
            ActionItemWeaponRChangeSubtype = 545,
            ActionItemAddSpecial = 550,
            ActionItemLimit = 599,

            // Dyn NPCs
            ActionNpconlyFirst = 600,
            ActionNpconlyCreatenewPet = 601,
            ActionNpconlyDeletePet = 602,
            ActionNpconlyMagiceffect = 603,
            ActionNpconlyMagiceffect2 = 604,
            ActionNpconlyLimit = 699,

            // Syndicate
            ActionSynFirst = 700,
            ActionSynCreate = 701,
            ActionSynDestroy = 702,
            ActionSynSetAssistant = 705,
            ActionSynClearRank = 706,
            ActionSynChangeLeader = 709,
            ActionSynAntagonize = 711,
            ActionSynClearAntagonize = 712,
            ActionSynAlly = 713,
            ActionSynClearAlly = 714,
            ActionSynAttr = 717,
            ActionSynChangeName = 732,
            ActionSynLimit = 799,

            //Monsters
            ActionMstFirst = 800,
            ActionMstDropitem = 801,
            ActionMstTeamReward = 802,
            ActionMstRefinery = 803,
            ActionMstLimit = 899,

            //User
            ActionUserFirst = 1000,
            ActionUserAttr = 1001,
            ActionUserFull = 1002, // Fill the user attributes. param is the attribute name. life/mana/xp/sp
            ActionUserChgmap = 1003, // Mapid Mapx Mapy savelocation
            ActionUserRecordpoint = 1004, // Records the user location, so he can be teleported back there later.
            ActionUserHair = 1005,
            ActionUserChgmaprecord = 1006,
            ActionUserChglinkmap = 1007,
            ActionUserTransform = 1008,
            ActionUserIspure = 1009,
            ActionUserTalk = 1010,
            ActionUserTalkBbs = 1011,
            ActionUserMagicEffect = 1011,
            ActionUserGodBless = 1012,
            ActionUserMagic = 1020,
            ActionUserWeaponskill = 1021,
            ActionUserLog = 1022,
            ActionUserBonus = 1023,
            ActionUserDivorce = 1024,
            ActionUserMarriage = 1025,
            ActionUserSex = 1026,
            ActionUserEffect = 1027,
            ActionUserTaskmask = 1028,
            ActionUserMediaplay = 1029,
            ActionUserSupermanlist = 1030,
            ActionUserAddTitle = 1031,
            ActionUserRemoveTitle = 1032,
            ActionUserCreatemap = 1033,
            ActionUserEnterHome = 1034,
            ActionUserEnterMateHome = 1035,
            ActionUserChkinCard2 = 1036,
            ActionUserChkoutCard2 = 1037,
            ActionUserFlyNeighbor = 1038,
            ActionUserUnlearnMagic = 1039,
            ActionUserRebirth = 1040,
            ActionUserWebpage = 1041,
            ActionUserBbs = 1042,
            ActionUserUnlearnSkill = 1043,
            ActionUserDropMagic = 1044,
            ActionUserFixAttr = 1045,
            ActionUserOpenDialog = 1046,
            ActionUserPointAllot = 1047,
            ActionUserPlusExp = 1048,
            ActionUserDelWpgBadge = 1049,
            ActionUserChkWpgBadge = 1050,
            ActionUserTakestudentexp = 1051,
            ActionUserWhPassword = 1052,
            ActionUserSetWhPassword = 1053,
            ActionUserOpeninterface = 1054,
            ActionUserVarCompare = 1060,
            ActionUserVarDefine = 1061,
            ActionUserVarCompareString = 1062,
            ActionUserVarDefineString = 1063,
            ActionUserVarCalc = 1064,
            ActionUserTestEquipment = 1065,
            ActionUserDailyStcCompare = 1067,
            ActionUserDailyStcOpe = 1068,
            ActionUserExecAction = 1071,
            ActionUserTestPos = 1072,
            ActionUserStcCompare = 1073,
            ActionUserStcOpe = 1074,
            ActionUserDataSync = 1075,
            ActionUserSelectToData = 1077,
            ActionUserTaskManager = 1080,
            ActionUserTaskOpe = 1081,
            ActionUserTaskLocaltime = 1082,
            ActionUserTaskFind = 1059,
            ActionUserStcTimeOperation = 1080,
            ActionUserStcTimeCheck = 1081,
            ActionUserAttachStatus = 1082,
            ActionUserGodTime = 1083,
            ActionUserCalExp = 1084,
            ActionUserLogEvent = 1085,
            ActionUserTimeToExp = 1086,
            ActionUserPureProfessional = 1094,
            ActionSomethingRelatedToRebirth = 1095,
            ActionUserStatusCreate = 1096,
            ActionUserStatusCheck = 1098,

            //User -> Team
            ActionTeamBroadcast = 1101,
            ActionTeamAttr = 1102,
            ActionTeamLeavespace = 1103,
            ActionTeamItemAdd = 1104,
            ActionTeamItemDel = 1105,
            ActionTeamItemCheck = 1106,
            ActionTeamChgmap = 1107,
            ActionTeamChkIsleader = 1501,
            ActionTeamCreateDynamap = 1520,

            ActionFrozenGrottoEntranceChkDays = 1202,
            ActionUserCheckHpFull = 1203,
            ActionUserCheckHpManaFull = 1204,
            // 1205-1215 > Transfer server actions
            ActionIsChangeServerEnable = 1205,
            ActionCheckServerName = 1213,
            ActionIsChangeServerIdle = 1214,
            ActionIsAccountServerNormal = 1215,

            // User -> Events???
            ActionElitePKValidateUser = 1301,
            ActionElitePKUserInscribed = 1302,
            ActionElitePKCheck = 1303,

            ActionTeamPKInscribe = 1311,
            ActionTeamPKExit = 1312,
            ActionTeamPKCheck = 1313,
            ActionTeamPKUnknown1314 = 1314,
            ActionTeamPKUnknown1315 = 1315,

            ActionSkillTeamPKInscribe = 1321,
            ActionSkillTeamPKExit = 1322,
            ActionSkillTeamPKCheck = 1323,
            ActionSkillTeamPKUnknown1324 = 1324,
            ActionSkillTeamPKUnknown1325 = 1325,

            // User -> General
            ActionTeamChkIsLeader = 1501,
            ActionEudemonAttrChk = 1503,
            ActionGeneralLottery = 1508,
            ActionUserRandTrans = 1509,

            ActionMentorGodBlessChk = 1510,


            ActionOpenShop = 1511,
            ActionHunterChk = 1547,
            ActionHunterAttrib = 1550,

            ActionSubclassPromotion = 1551,
            ActionSubclassLevel = 1552,
            ActionAchievements = 1554,
            ActionAttachBuffStatus = 1555,
            ActionDetachBuffStatuses = 1556,
            ActionUserReturn = 1557, // data = opt ? ; param iterator index to save the value

            ActionMouseWaitClick = 1650, // 发消息通知客户端点选目标,data=后面操作的action_id，param=[鼠标图片id，对应客户端Cursor.ini的记录]
            ActionMouseJudgeType = 1651, // 判断点选目标的类型 data：1表示点npc，param=‘npc名字’;data：2表示点怪物param=‘怪物id’;data：3表示判断点选玩家性别判断param=‘性别id’ 1男，2女
            ActionMouseClearStatus = 1652, // 清除玩家当前指针选取状态 服务器新增清除玩家当前指针选取状态的action，服务器执行该action后，下发消息给客户端
            ActionMouseDeleteChosen = 1654, // 

            /// <summary>
            /// genuineqi set 3                 >= += == set Talent Status
            /// freecultivateparam              >= += set
            /// </summary>
            ActionJiangHuAttributes = 1705,
            ActionJiangHuInscribed = 1706,
            ActionJiangHuLevel = 1707, // data level to check
            ActionJiangHuExpProtection = 1709,  // param "+= 3600" seconds

            ActionAutoHuntIsActive = 1721,
            ActionCheckUserAttributeLimit = 1723,
            ActionAddProcessActivityTask = 1724,
            ActionAddProcessTaskSchedle = 1725, // Increase the progress of staged tasks (data fill task type)

            ActionUserLimit = 1999,

            //Events
            ActionEventFirst = 2000,
            ActionEventSetstatus = 2001,
            ActionEventDelnpcGenid = 2002,
            ActionEventCompare = 2003,
            ActionEventCompareUnsigned = 2004,
            ActionEventChangeweather = 2005,
            ActionEventCreatepet = 2006,
            ActionEventCreatenewNpc = 2007,
            ActionEventCountMonster = 2008,
            ActionEventDeleteMonster = 2009,
            ActionEventBbs = 2010,
            ActionEventErase = 2011,
            ActionEventMapUserChgMap = 2012,
            ActionEventCreatePetEx = 2013,
            ActionEventCountMonsterEx = 2014,
            ActionEventMapActionExec = 2016,

            ActionEventRegister = 2050,
            ActionEventExit = 2051,
            ActionEventCmd = 2052,
            ActionEventLimit = 2099,

            //Traps
            ActionTrapFirst = 2100,
            ActionTrapCreate = 2101,
            ActionTrapErase = 2102,
            ActionTrapCount = 2103,
            ActionTrapAttr = 2104,
            ActionTrapTypeDelete = 2105,
            ActionTrapLimit = 2199,

            // Detained Item
            ActionDetainFirst = 2200,
            ActionDetainDialog = 2205,
            ActionDetainLimit = 2299,

            //Wanted
            ActionWantedFirst = 3000,
            ActionWantedNext = 3001,
            ActionWantedName = 3002,
            ActionWantedBonuty = 3003,
            ActionWantedNew = 3004,
            ActionWantedOrder = 3005,
            ActionWantedCancel = 3006,
            ActionWantedModifyid = 3007,
            ActionWantedSuperadd = 3008,
            ActionPolicewantedNext = 3010,
            ActionPolicewantedOrder = 3011,
            ActionPolicewantedCheck = 3012,
            ActionWantedLimit = 3099,

            // Family
            ActionFamilyFirst = 3500,
            ActionFamilyAttr = 3501,
            ActionFamilyMemberAttr = 3510,
            ActionFamilyWarActivityCheck = 3521,
            ActionFamilyWarAuthorityCheck = 3523,
            ActionFamilyWarRegisterCheck = 3524,
            ActionFamilyLast = 3599,

            ActionMountRacingEventReset = 3601,

            // Progress
            ActionProgressBar = 3701,

            ActionCaptureTheFlagCheck = 3901,
            ActionCaptureTheFlagExit = 3902,

            //Magic
            ActionMagicFirst = 4000,
            ActionMagicAttachstatus = 4001,
            ActionMagicAttack = 4002,
            ActionMagicEudemonLearn = 4003,
            ActionMagicStatusChk = 4004,
            ActionMagicLimit = 4099,

            ActionVipFirst = 5000,
            ActionVipAttr = 5001,
            ActionVipLimit = 5099,

            ActionLuaLinkMain = 20001,
            ActionLuaExecute = 20002,
        }

        public enum OpenWindow
        {
            Compose = 1,
            Craft = 2,
            Warehouse = 4,
            ClanWindow = 64,
            DetainRedeem = 336,
            DetainClaim = 337,
            VipWarehouse = 341,
            Breeding = 368,
            PurificationWindow = 455,
            StabilizationWindow = 459,
            TalismanUpgrade = 347,
            GemComposing = 422,
            OpenSockets = 425,
            Blessing = 426,
            TortoiseGemComposing = 438,
            HorseRacingStore = 464,
            EditCharacterName = 489,
            GarmentTrade = 502,
            DegradeEquipment = 506,
            VerifyPassword = 568,
            SetNewPassword = 569,
            ModifyPassword = 570,
            BrowseAuction = 572,
            EmailInbox = 576,
            EmailIcon = 578,
            GiftRanking = 584,
            FriendRequest = 606,
            JiangHuJoinIcon = 618
        }
    }
}
