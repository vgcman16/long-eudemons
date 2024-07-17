using Long.Database.Entities;

namespace Long.Kernel.Database.Repositories
{
    public static class SyndicateRepository
    {
        public static async Task<List<DbSyndicate>> GetAsync()
        {
            await using var db = new ServerDbContext();
            return db.Syndicates.ToList();
        }
    }
}
