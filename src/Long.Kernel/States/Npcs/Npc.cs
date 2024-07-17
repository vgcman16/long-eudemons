using Long.Database.Entities;
using Long.Kernel.Database;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.User;
using System.Drawing;

namespace Long.Kernel.States.Npcs
{
    public sealed class Npc : BaseNpc
    {
        private readonly DbNpc npc;

        public Npc(DbNpc npc)
            : base(npc.Id)
        {
            this.npc = npc;

            idMap = npc.Mapid;
            X = npc.Cellx;
            Y = npc.Celly;

            Name = npc.Name;
        }

        #region Type

        public override ushort Type => npc.Type;

        public override NpcSort Sort => (NpcSort)npc.Sort;

        public override uint OwnerIdentity
        {
            get => npc.Ownerid;
            set => npc.Ownerid = value;
        }

        #endregion

        #region Map and Position

        public override async Task<bool> ChangePosAsync(uint idMap, ushort x, ushort y)
        {
            if (await base.ChangePosAsync(idMap, x, y))
            {
                npc.Mapid = idMap;
                npc.Celly = y;
                npc.Cellx = x;
                await SaveAsync();
                return true;
            }

            return false;
        }

        #endregion

        #region Task and Data

        public override uint Task0
        {
            get => npc.Task0;
            protected set => npc.Task0 = value;
        }

        public override uint Task1
        {
            get => npc.Task1;
            protected set => npc.Task1 = value;
        }

        public override uint Task2
        {
            get => npc.Task2;
            protected set => npc.Task2 = value;
        }

        public override uint Task3
        {
            get => npc.Task3;
            protected set => npc.Task3 = value;
        }

        public override uint Task4
        {
            get => npc.Task4;
            protected set => npc.Task4 = value;
        }

        public override uint Task5
        {
            get => npc.Task5;
            protected set => npc.Task5 = value;
        }

        public override uint Task6
        {
            get => npc.Task6;
            protected set => npc.Task6 = value;
        }

        public override uint Task7
        {
            get => npc.Task7;
            protected set => npc.Task7 = value;
        }

        public override int Data0
        {
            get => npc.Data0;
            set => npc.Data0 = value;
        }

        public override int Data1
        {
            get => npc.Data1;
            set => npc.Data1 = value;
        }

        public override int Data2
        {
            get => npc.Data2;
            set => npc.Data2 = value;
        }

        public override int Data3
        {
            get => npc.Data3;
            set => npc.Data3 = value;
        }

        public override string DataStr
        {
            get => npc.Datastr;
            set => npc.Datastr = value;
        }

        #endregion

        #region Socket

        public override async Task SendSpawnToAsync(Character player)
        {
            await player.SendAsync(new MsgNpcInfo
            {
                Identity = Identity,
                Lookface = (ushort)npc.Lookface,
                Sort = npc.Sort,
                PosX = X,
                PosY = Y,
                OwnerId = OwnerIdentity,
                NpcType = npc.Type,
                Data = (uint)npc.Data0,
            });
        }

        #endregion

        #region Database

        public override async Task<bool> SaveAsync()
        {
            return await ServerDbContext.UpdateAsync(npc);
        }

        public override async Task<bool> DeleteAsync()
        {
            return await ServerDbContext.DeleteAsync(npc);
        }

        #endregion
    }
}
