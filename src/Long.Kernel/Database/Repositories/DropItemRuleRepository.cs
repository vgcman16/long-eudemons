using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Long.Kernel.Database.Repositories
{
    public class DropItemRuleRepository
    {
        public static async Task<List<DbDropItemRule>> GetAsync()
        {
            using var ctx = new ServerDbContext();
            return await ctx.DropItemRules
                .OrderBy(x => x.Identity)
                .ToListAsync();
        }

        public static async Task<List<DbDropItemRule>>GetRulesByGroupAsync(uint groupId)
        {
            using var ctx = new ServerDbContext();
            return await ctx.DropItemRules
                .Where(x => x.GroupdId == groupId)
                .OrderBy(x => x.Chance)
                .ToListAsync();
        }
    }
}
