using Long.Database.Entities;
using Long.Login.Database.Entities;

namespace Long.Login.Database.Repositories
{
    public class AccountRepository
    {
        //public static GameAccount GetByUsername(string username)
        //{
        //    using var context = new ServerDbContext();
        //    return context.GameAccounts.FirstOrDefault(x => x.UserName == username);
        //}
        public static DbAccount GetByUsername(string username)
        {
            using var context = new ServerDbContext();
            return context.Accounts.FirstOrDefault(x => x.Username == username);
        }
    }
}
