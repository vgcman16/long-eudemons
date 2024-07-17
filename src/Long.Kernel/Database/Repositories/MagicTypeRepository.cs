using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Long.Kernel.Database.Repositories
{
    public static class MagicTypeRepository
    {
        public static async Task<List<DbMagictype>> GetAsync()
        {
            await using var db = new ServerDbContext();
            return await db.Magictypes.ToListAsync();
        }

        public static async Task<List<DbTrack>> GetTracksAsync()
        {
            await using var db = new ServerDbContext();
            return await db.Tracks.ToListAsync();
        }
    }
}
