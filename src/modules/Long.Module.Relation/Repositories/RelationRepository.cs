using Long.Database.Entities;
using Long.Kernel.Database;
using Microsoft.EntityFrameworkCore;

namespace Long.Module.Relation.Repositories
{
    public static class RelationRepository
    {
        public static async Task<List<DbFriend>> GetFriendsAsync(uint idUser)
        {
            await using var context = new ServerDbContext();
            return await context.Friends
                .Where(x => x.UserId == idUser)
                .ToListAsync();
        }

        public static async Task<List<DbEnemy>> GetEnemiesAsync(uint idUser)
        {
            await using var context = new ServerDbContext();
            return await context.Enemies
                .Where(x => x.UserId == idUser)
                .ToListAsync();
        }
    }
}
