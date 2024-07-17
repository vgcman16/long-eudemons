using Long.Kernel.Database;
using Long.Kernel.Modules.Interfaces;
using Long.Kernel.Modules.Systems.DynaRanking;
using Long.Kernel.States.User;

namespace Long.Kernel.Modules.Managers
{
    public interface IDynaRankManager : IInitializeSystem
    {
        int GetUserRankPos(uint userId, uint rankType);
        IDynaRank QueryRank(uint rankType);
        Task<bool> CreateOrUpdateAsync(uint rankType, uint idUser, string szName, long value, string szData = "");
        Task<bool> IncreaseOrUpdateAsync(uint rankType, uint idUser, string szName, long value, string szData = "");
        Task OnUserDeleteAsync(Character user, ServerDbContext ctx);
    }
}
