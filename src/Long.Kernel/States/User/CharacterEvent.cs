using Long.Database.Entities;
using Long.Kernel.Managers;
using Long.Kernel.Modules.Systems.TaskDetail;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.Items;
using Long.Shared.Helpers;
using System.Drawing;

namespace Long.Kernel.States.User
{
    public partial class Character
    {
        public Task SendMenuMessageAsync(string message)
        {
            return SendAsync(new MsgTalk(TalkChannel.MessageBox, message));
        }

        #region Game Action

        public long Iterator { get; set; } = -1;
        public long[] VarData { get; } = new long[MAX_VAR_AMOUNT];
        public string[] VarString { get; } = new string[MAX_VAR_AMOUNT];

        private readonly List<string> setTaskId = new();

        public uint LastAddItemIdentity { get; set; }
        public uint LastDelItemIdentity { get; set; }

        public uint InteractingItem { get; set; }
        public uint InteractingNpc { get; set; }
        public uint InteractingMouseAction { get; set; }
        public string InteractingMouseFunction { get; set; }

        public bool CheckItem(DbTask task)
        {
            if (!string.IsNullOrEmpty(task.Itemname1) && !StrNone.Equals(task.Itemname1))
            {
                if (UserPackage[task.Itemname1] == null)
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(task.Itemname2) && !StrNone.Equals(task.Itemname2))
                {
                    if (UserPackage[task.Itemname2] == null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void CancelInteraction()
        {
            setTaskId.Clear();
            InteractingItem = 0;
            InteractingNpc = 0;
        }

        public byte PushTaskId(string idTask)
        {
            if (!"0".Equals(idTask) && setTaskId.Count < MAX_MENUTASKSIZE)
            {
                setTaskId.Add(idTask);
                return (byte)setTaskId.Count;
            }
            return 0;
        }

        public void ClearTaskId()
        {
            setTaskId.Clear();
        }

        public string GetTaskId(int idx)
        {
            return idx > 0 && idx <= setTaskId.Count ? setTaskId[idx - 1] : "0";
        }

        public bool TestTask(DbTask task)
        {
            if (task == null)
            {
                return false;
            }

            try
            {
                if (!CheckItem(task))
                {
                    return false;
                }

                if (Silvers < task.Money)
                {
                    return false;
                }

                if (task.Profession != 0 && Profession != task.Profession)
                {
                    return false;
                }

                if (task.Sex != 0 && task.Sex != 999 && task.Sex != Gender)
                {
                    return false;
                }

                if (PkPoints < task.MinPk || PkPoints > task.MaxPk)
                {
                    return false;
                }

                if (task.Marriage >= 0)
                {
                    if (task.Marriage == 0 && MateIdentity != 0)
                    {
                        return false;
                    }

                    if (task.Marriage == 1 && MateIdentity == 0)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Test task error", ex.Message);
                return false;
            }

            return true;
        }

        public async Task AddTaskMaskAsync(int idx)
        {
            if (idx < 0 || idx >= 32)
            {
                return;
            }

            user.TaskMask |= 1u << idx;
            await SaveAsync();
        }

        public async Task ClearTaskMaskAsync(int idx)
        {
            if (idx < 0 || idx >= 32)
            {
                return;
            }

            user.TaskMask &= ~(1u << idx);
            await SaveAsync();
        }

        public bool CheckTaskMask(int idx)
        {
            if (idx < 0 || idx >= 32)
            {
                return false;
            }

            return (user.TaskMask & (1u << idx)) != 0;
        }

        #endregion

        #region Statistic and Task Detail

        public UserStatistic Statistic { get; set; }
        public ITaskDetail TaskDetail { get; set; }

        #endregion
    }
}
