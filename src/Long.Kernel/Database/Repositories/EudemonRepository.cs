using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Long.Kernel.Database.Repositories
{
    public class EudemonRepository
    {
        public static async Task<DbEudemon> GetAsync(uint itemId)
        {
            using var ctx = new ServerDbContext();
            return await ctx.Eudemons.FirstOrDefaultAsync(e => e.ItemIdentity == itemId);
        }

        public static async Task<List<DbEudemon>> GetUserEudemonsAsync(uint userId)
        {
            using var ctx = new ServerDbContext();
            return await ctx.Eudemons
                .Where(x => x.OwnerId == userId && x.PlayerId == userId)
                .ToListAsync();
        }
    }
}
