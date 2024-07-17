using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Long.Kernel.Database.Repositories
{
    public static class OfficialTypeRepository
    {
        public static async Task<List<DbOfficialType>> GetAsync()
        {
            using var ctx = new ServerDbContext();
            return await ctx.OfficialTypes.ToListAsync();
        }
    }
}
