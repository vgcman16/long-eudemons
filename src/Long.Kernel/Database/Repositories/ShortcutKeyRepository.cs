using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Long.Kernel.Database.Repositories
{
    public class ShortcutKeyRepository
    {
        public static async Task<DbShortcutKey> GetAsync(uint userId)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.ShortcutKeys.FirstOrDefaultAsync(x => x.PlayerId == userId);
        }
    }
}
