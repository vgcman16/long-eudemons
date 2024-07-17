using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Long.Kernel.Database.Repositories
{
    public static class FamilyAttrRepository
    {
        public static async Task<List<DbFamilyAttr>> GetAsync(uint idFamily)
        {
            await using var ctx = new ServerDbContext();
            return await ctx.FamilyAttrs.Where(x => x.FamilyIdentity == idFamily).ToListAsync();
        }
    }
}
