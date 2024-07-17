using Long.Database.Entities;
using Long.Kernel.Modules.Interfaces;
using Long.Kernel.States.User;

namespace Long.Kernel.Modules.Managers
{
    public interface IFlowerManager : IInitializeSystem
    {
        /// <summary>
        /// USED ONLY FOR USER DELETE!
        /// </summary>
        bool RemoveUser(uint userId);

        DbFlower GetUser(uint userId);
        int GetUserPos(uint userId, uint flowerType);
        Task<bool> SendFlowerAsync(Character sender, Character target, uint flowerType, uint amount, uint itemType = 0);
        Task SendRankDisplayAsync(Character target, uint flowerType, int page, int ipp = 10);
        Task DailyResetAsync();
    }
}
