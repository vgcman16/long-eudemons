using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Long.Kernel.Database.Repositories
{
    public static class TrapTypeRepository
    {
        public static async Task<List<DbTrap>> GetAsync()
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.Traps
                .Include(x => x.Type)
                .ToListAsync();
        }

        public static async Task<List<DbTrapType>> GetTrapTypesAsync()
        {
            await using ServerDbContext ctx = new ServerDbContext();
            return await ctx.TrapTypes.ToListAsync();
        }
    }
}
