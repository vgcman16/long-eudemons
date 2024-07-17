using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Long.Kernel.Database.Repositories
{
    public static class SoulValueRepository
    {
        public static async Task<List<DbSoulValueLev>> GetAsync()
        {
            await using var db = new ServerDbContext();
            return await db.SoulValueLevs.ToListAsync();
        }
    }
}
