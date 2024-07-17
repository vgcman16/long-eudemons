using Long.Database.Entities;
using Long.Kernel.Database;
using Long.Kernel.Database.Repositories;

namespace Long.Kernel.States.User
{
    public partial class Character
    {
        #region Status
        
        public async Task LoadStatusAsync()
        {
            List<DbStatus> statusList = await StatusRepository.GetAsync(Identity);
            await using var serverDbContext = new ServerDbContext();
            foreach (DbStatus status in statusList)
            {
                if (UnixTimestamp.ToDateTime(status.EndTime) < DateTime.Now)
                {
                    serverDbContext.Status.Remove(status);
                    continue;
                }

                await AttachStatusAsync(status);
            }
            await serverDbContext.SaveChangesAsync();
            await CheckPkStatusAsync();
        }

        #endregion
    }
}
