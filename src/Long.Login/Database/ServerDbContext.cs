using Long.Database;
using Long.Database.Entities;
using Long.Login.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Long.Login.Database
{
    public class ServerDbContext : AbstractDbContext
    {
        private static readonly ILogger logger = Log.ForContext<ServerDbContext>();

        public virtual DbSet<DbAccount> Accounts { get; set; }
        public virtual DbSet<GameAccount> GameAccounts { get; set; }
        public virtual DbSet<GameAccountAuthority> GameAccountsAuthority { get; set; }
        public virtual DbSet<GameAccountVip> GameAccountVips { get; set; }
        public virtual DbSet<RealmData> RealmDatas { get; set; }
    }
}