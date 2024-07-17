using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Long.Kernel.Database.Repositories
{
    public static class ActionRepository
    {
        public static async Task<List<DbAction>> GetAsync()
        {
            await using var db = new ServerDbContext();
            return await db.Actions.OrderBy(x => x.Id).ToListAsync();
        }
    }
}
