using Long.Database.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Long.Kernel.Database.Repositories
{
    public static class EudemonRbnRepository
    {
        public static async Task<List<DbEudemonRbnRqr>> GetRebornRqrAsync()
        {
            using var ctx = new ServerDbContext();
            return await ctx.EudemonRbnRqrs.ToListAsync();
        }

        public static async Task<List<DbEudemonRbnType>> GetRebornTypeAsync()
        {
            using var ctx = new ServerDbContext();
            return await ctx.EudemonRbnTypes.ToListAsync();
        }
    }
}
