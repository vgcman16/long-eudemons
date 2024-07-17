using Long.Database.Entities;
using Long.Kernel.Database;
using Long.Kernel.Database.Repositories;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.User;
using System.Collections.Concurrent;

namespace Long.Kernel.States
{
    public sealed class UserStatistic
    {
        private readonly ConcurrentDictionary<uint, DbStatistic> statistics = new();
        private readonly Character user;

        public UserStatistic(Character user)
        {
            this.user = user;
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                var list = await StatisticRepository.GetAsync(user.Identity);
                foreach (var st in list)
                {
                    statistics.TryAdd(st.EventType, st);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public DbStatistic GetStc(uint idEvent)
        {
            return statistics.TryGetValue(idEvent, out var stc) ? stc : null;
        }

        public uint GetValue(uint idEvent)
        {
            return statistics.TryGetValue(idEvent, out var stc) ? stc.Data : 0;
        }

        public Task<bool> IncreaseAsync(uint idEvent, uint data = 1, bool bUpdate = false)
        {
            var stc = GetStc(idEvent);
            if (stc != null)
            {
                stc.Data += data;
                if (bUpdate)
                {
                    return ServerDbContext.UpdateAsync(stc);
                }

                return Task.FromResult(true);
            }
            else
            {
                stc = new DbStatistic
                {
                    Data = data,
                    EventType = idEvent,
                    PlayerId = user.Identity,
                };

                statistics.TryAdd(stc.EventType, stc);
                return ServerDbContext.CreateAsync(stc);
            }
        }

        public Task<bool> AddOrUpdateAsync(uint idEvent, uint data, bool bUpdate = false)
        {
            var stc = GetStc(idEvent);
            if (stc != null)
            {
                stc.Data = data;
                if (bUpdate)
                {
                    return ServerDbContext.UpdateAsync(stc);
                }

                return Task.FromResult(true);
            }
            else
            {
                stc = new DbStatistic
                {
                    Data = data,
                    EventType = idEvent,
                    PlayerId = user.Identity,
                };

                statistics.TryAdd(stc.EventType, stc);
                return ServerDbContext.CreateAsync(stc);
            }
        }

        public async Task SendListAsync(Character target, ushort page = 1)
        {
            //var msg = new MsgStatistic()
            //{
            //    Identity = user.Identity,
            //    Action = MsgStatistic.StatisticType.Response,
            //    Page = page,
            //};

            //msg.Data = statistics.Values
            //    .OrderBy(x => x.EventType)
            //    .Select(x => new MsgStatistic.StatisticData()
            //    {
            //        TypeId = x.EventType,
            //        Amount = x.Data
            //    })
            //    .ToList();

            //await target.SendAsync(msg);
        }

        public Task SaveAsync()
        {
            return ServerDbContext.UpdateRangeAsync(statistics.Values.ToList());
        }
    }
}
