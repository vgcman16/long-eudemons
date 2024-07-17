using Long.Database.Entities;
using Long.Kernel.States.User;

namespace Long.Kernel.Modules.Systems.DynaRanking
{
    public interface IDynaRank
    {
        public const int DEFAULT_RANKSIZE = 120;

        void LoadData(DbDynaRankRec record);

        uint RankType { get; }
        int MaxSize { get; }

        int GetRankRecPos(uint userId);
        DbDynaRankRec GetRankRec(uint userId);
        DbDynaRankRec GetRankRecByPos(uint userId, int index);
        List<DbDynaRankRec> GetRanking(int limit = DEFAULT_RANKSIZE);

        Task<bool> CreateOrUpdateAsync(Character user, long data);
        Task<bool> CreateOrUpdateAsync(uint userId, string userName, string dataStr, long data);
        Task<bool> IncreaseOrUpdateAsync(uint userId, string userName, string dataStr, long data);
        Task<bool> DeleteRankRecAsync(uint userId);
        bool RemoveRankRec(uint userId);
        Task<bool> ClearRankRecAsync(uint userId);
        Task ResetAsync();
    }
}
