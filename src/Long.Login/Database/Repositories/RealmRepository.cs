using Long.Login.Database.Entities;

namespace Long.Login.Database.Repositories
{
    public class RealmRepository
    {
        public static RealmData GetById(Guid realmId)
        {
            using var context = new ServerDbContext();
            return context.RealmDatas.FirstOrDefault(x => x.RealmID == realmId);
        }
    }
}
