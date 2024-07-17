using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Long.Kernel.Database.Repositories
{
    public static class DynaRankRepository
    {
        public static async Task CreateOrUpdateAsync(uint rankType, uint idUser, string szName, long value, string szData = "")
        {
            await using var context = new ServerDbContext();
            var userRank = await context.DynaRankRecs.FirstOrDefaultAsync(x => x.RankType == rankType && x.UserId == idUser);
            if (userRank == null)
            {
                context.DynaRankRecs.Add(new DbDynaRankRec
                {
                    ObjId = idUser,
                    UserId = idUser,
                    RankType = rankType,
                    Value = value,
                    DataStr = szData,
                    UserName = szName,
                });
            }
            else
            {
                userRank.Value = value;
            }
            await context.SaveChangesAsync();
        }

        public static async Task IncCreateOrUpdateAsync(uint rankType, uint idUser, string szName, long value, string szData = "")
        {
            await using var context = new ServerDbContext();
            var userRank = await context.DynaRankRecs.FirstOrDefaultAsync(x => x.RankType == rankType && x.UserId == idUser);
            if (userRank == null)
            {
                context.DynaRankRecs.Add(new DbDynaRankRec
                {
                    ObjId = idUser,
                    UserId = idUser,
                    RankType = rankType,
                    Value = value,
                    DataStr = szData,
                    UserName = szName,
                });
            }
            else
            {
                userRank.Value += value;
            }
            await context.SaveChangesAsync();
        }
    }
}
