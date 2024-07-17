using Long.Database.Entities;
using Long.Kernel.Managers;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States;
using Long.Kernel.States.Items;
using Long.Kernel.States.User;
using System.Drawing;
using static Long.Kernel.Network.Game.Packets.MsgAction;

namespace Long.Kernel.Scripting.Action
{
    public partial class GameAction
    {
        public static async Task<bool> ProcessActionSysAsync(DbAction action, string param, Character user, Role role, ItemBase item, params string[] inputs)
        {
            switch ((TaskActionType)action.Type)
            {
                case TaskActionType.ActionMenutext:
                    {
                        if (user == null)
                        {
                            logger.Warning("Action[{0}] type 101 non character", action.Id);
                            return false;
                        }

                        int messages = (int)Math.Ceiling(param.Length / (double)byte.MaxValue);
                        for (int i = 0; i < messages; i++)
                        {
                            await user.SendAsync(new MsgTaskDialog
                            {
                                InteractionType = MsgTaskDialog.TaskInteraction.Dialog,
                                Text = param.Substring(i * byte.MaxValue, Math.Min(byte.MaxValue, param.Length - byte.MaxValue * i)),
                                Data = (ushort)action.Data
                            });
                        }
                        return true;
                    }
                case TaskActionType.ActionMenulink:
                    {
                        if (user == null)
                        {
                            logger.Warning("Action[{0}] type 102 not user", action.Id);
                            return false;
                        }

                        uint task = 0;
                        int align = 0;
                        string[] parsed = param.Split(' ');
                        if (parsed.Length > 1)
                        {
                            uint.TryParse(parsed[1], out task);
                        }

                        if (parsed.Length > 2)
                        {
                            int.TryParse(parsed[2], out align);
                        }

                        await user.SendAsync(new MsgTaskDialog
                        {
                            InteractionType = MsgTaskDialog.TaskInteraction.Option,
                            Text = parsed[0],
                            OptionIndex = user.PushTaskId(task.ToString()),
                            Data = (ushort)align
                        });
                        return true;
                    }
                case TaskActionType.ActionMenuedit:
                    {
                        if (user == null)
                        {
                            return false;
                        }

                        string[] paramStrings = SplitParam(param, 3);
                        if (paramStrings.Length < 3)
                        {
                            logger.Warning("Invalid input param length for {0}, param: {1}", action.Id, param);
                            return false;
                        }

                        await user.SendAsync(new MsgTaskDialog
                        {
                            InteractionType = MsgTaskDialog.TaskInteraction.Input,
                            OptionIndex = user.PushTaskId(paramStrings[1]),
                            Data = ushort.Parse(paramStrings[0]),
                            Text = paramStrings[2]
                        });

                        return true;
                    }
                case TaskActionType.ActionMenupic:
                    {
                        if (user == null)
                        {
                            return false;
                        }

                        string[] splitParam = SplitParam(param);
                        ushort x = ushort.Parse(splitParam[0]);
                        ushort y = ushort.Parse(splitParam[1]);

                        await user.SendAsync(new MsgTaskDialog
                        {
                            TaskIdentity = (uint)((x << 16) | y),
                            InteractionType = MsgTaskDialog.TaskInteraction.Avatar,
                            Data = ushort.Parse(splitParam[2])
                        });

                        return true;
                    }
                case TaskActionType.ActionMenuMessage:
                    {
                        if (user == null)
                        {
                            return false;
                        }

                        await user.SendAsync(new MsgTaskDialog
                        {
                            InteractionType = MsgTaskDialog.TaskInteraction.MessageBox,
                            Text = param,
                            OptionIndex = user.PushTaskId(action.Data.ToString())
                        });

                        return true;
                    }
                case TaskActionType.ActionMenucreate:
                    {
                        if (user == null)
                        {
                            return false;
                        }

                        await user.SendAsync(new MsgTaskDialog
                        {
                            InteractionType = MsgTaskDialog.TaskInteraction.Finish
                        });

                        return true;
                    }

                case TaskActionType.ActionSysUpdate:
                    {
                        await user.SendAsync(new MsgTaskDialog
                        {
                            InteractionType = MsgTaskDialog.TaskInteraction.UpdateWindow,
                            Text = param,
                        });

                        return true;
                    }

                case TaskActionType.ActionSysUpdateCreate:
                    {
                        await user.SendAsync(new MsgTaskDialog
                        {
                            InteractionType = MsgTaskDialog.TaskInteraction.UpdateWindowFinish
                        });

                        return true;
                    }

                case TaskActionType.ActionRand:
                    {
                        string[] paramSplit = SplitParam(param, 2);

                        int x = int.Parse(paramSplit[0]);
                        int y = int.Parse(paramSplit[1]);
                        double chance = 0.01;
                        if (x > y)
                        {
                            chance = 99;
                        }
                        else
                        {
                            chance = x / (double)y;
                            chance *= 100;
                        }

                        return await ChanceCalcAsync(chance);
                    }
                case TaskActionType.ActionRandaction:
                    {
                        string[] paramSplit = SplitParam(param);
                        if (paramSplit.Length == 0)
                        {
                            return false;
                        }

                        uint taskId = uint.Parse(paramSplit[await NextAsync(0, paramSplit.Length) % paramSplit.Length]);
                        await ExecuteActionAsync(taskId, user, role, item, inputs);
                        return true;
                    }

                case TaskActionType.ActionChktime:
                    {
                        if (string.IsNullOrEmpty(param))
                        {
                            return false;
                        }

                        DateTime now = DateTime.Now;

                        int nCurYear = now.Year;
                        int nCurMonth = now.Month;
                        int nCurDay = now.Day;
                        int nCurWeekDay = (int)now.DayOfWeek;
                        int nCurHour = now.Hour;
                        int nCurMinute = now.Minute;

                        string[] parts = param.Split(' ');

                        int nYear0, nMonth0, nDay0, nHour0, nMinute0;
                        int nYear1, nMonth1, nDay1, nHour1, nMinute1;

                        if (action.Data == 0 && parts.Length == 10)
                        {
                            if (int.TryParse(parts[0], out nYear0) &&
                                int.TryParse(parts[1], out nMonth0) &&
                                int.TryParse(parts[2], out nDay0) &&
                                int.TryParse(parts[3], out nHour0) &&
                                int.TryParse(parts[4], out nMinute0) &&
                                int.TryParse(parts[5], out nYear1) &&
                                int.TryParse(parts[6], out nMonth1) &&
                                int.TryParse(parts[7], out nDay1) &&
                                int.TryParse(parts[8], out nHour1) &&
                                int.TryParse(parts[9], out nMinute1))
                            {
                                DateTime time0 = new DateTime(nYear0, nMonth0, nDay0, nHour0, nMinute0, 0);
                                DateTime time1 = new DateTime(nYear1, nMonth1, nDay1, nHour1, nMinute1, 0);
                                if (now >= time0 && now <= time1)
                                {
                                    return true;
                                }
                            }
                        }
                        else if (action.Data == 1 && parts.Length == 8)
                        {
                            if (int.TryParse(parts[0], out nMonth0) &&
                                int.TryParse(parts[1], out nDay0) &&
                                int.TryParse(parts[2], out nHour0) &&
                                int.TryParse(parts[3], out nMinute0) &&
                                int.TryParse(parts[4], out nMonth1) &&
                                int.TryParse(parts[5], out nDay1) &&
                                int.TryParse(parts[6], out nHour1) &&
                                int.TryParse(parts[7], out nMinute1))
                            {
                                DateTime time0 = new DateTime(nCurYear, nMonth0, nDay0, nHour0, nMinute0, 0);
                                DateTime time1 = new DateTime(nCurYear, nMonth1, nDay1, nHour1, nMinute1, 0);
                                if (now >= time0 && now <= time1)
                                {
                                    return true;
                                }
                            }
                        }
                        else if (action.Data == 2 && parts.Length == 6)
                        {
                            if (int.TryParse(parts[0], out nDay0) &&
                                int.TryParse(parts[1], out nHour0) &&
                                int.TryParse(parts[2], out nMinute0) &&
                                int.TryParse(parts[3], out nDay1) &&
                                int.TryParse(parts[4], out nHour1) &&
                                int.TryParse(parts[5], out nMinute1))
                            {
                                DateTime time0 = new DateTime(nCurYear, nCurMonth, nDay0, nHour0, nMinute0, 0);
                                DateTime time1 = new DateTime(nCurYear, nCurMonth, nDay1, nHour1, nMinute1, 0);
                                if (now >= time0 && now <= time1)
                                {
                                    return true;
                                }
                            }
                        }
                        else if (action.Data == 3 && parts.Length == 6)
                        {
                            if (int.TryParse(parts[0], out nDay0) &&
                                int.TryParse(parts[1], out nHour0) &&
                                int.TryParse(parts[2], out nMinute0) &&
                                int.TryParse(parts[3], out nDay1) &&
                                int.TryParse(parts[4], out nHour1) &&
                                int.TryParse(parts[5], out nMinute1))
                            {
                                int timeNow = nCurWeekDay * 24 * 60 + nCurHour * 60 + nCurMinute;
                                if (timeNow >= nDay0 * 24 * 60 + nHour0 * 60 + nMinute0 &&
                                    timeNow <= nDay1 * 24 * 60 + nHour1 * 60 + nMinute1)
                                {
                                    return true;
                                }
                            }
                        }
                        else if (action.Data == 4 && parts.Length == 4)
                        {
                            if (int.TryParse(parts[0], out nHour0) &&
                                int.TryParse(parts[1], out nMinute0) &&
                                int.TryParse(parts[2], out nHour1) &&
                                int.TryParse(parts[3], out nMinute1))
                            {
                                int timeNow = nCurHour * 60 + nCurMinute;
                                if (timeNow >= nHour0 * 60 + nMinute0 &&
                                    timeNow <= nHour1 * 60 + nMinute1)
                                {
                                    return true;
                                }
                            }
                        }
                        else if (action.Data == 5 && parts.Length == 2)
                        {
                            if (int.TryParse(parts[0], out nMinute0) &&
                                int.TryParse(parts[1], out nMinute1))
                            {
                                if (nCurMinute >= nMinute0 && nCurMinute <= nMinute1)
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                case TaskActionType.ActionPostcmd:
                    {
                        if (user == null)
                        {
                            return false;
                        }

                        await user.SendAsync(new MsgAction
                        {
                            Identity = user.Identity,
                            CommandX = user.X,
                            CommandY = user.Y,
                            Argument = (byte)user.Direction,
                            Data = action.Data,
                            Action = EOActionType.actionPostCmd,
                        });

                        return true;
                    }

                case TaskActionType.ActionBrocastmsg:
                    {
                        await RoleManager.BroadcastWorldMsgAsync(param, (TalkChannel)action.Data, Color.White);
                        return true;
                    }

                case TaskActionType.ActionMessageBox:
                    {
                        if (user == null)
                        {
                            return false;
                        }

                        TalkChannel channel = TalkChannel.MessageBox;
                        if (action.Data > 0 && action.Data < 100)
                        {
                            channel = (TalkChannel)(2500 + action.Data);
                        }

                        await user.SendAsync(param, channel);
                        return true;
                    }
            }

            return false;
        }
    }
}
