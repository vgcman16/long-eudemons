using Long.Kernel.Managers;
using Long.Shared.Threads;

namespace Long.Kernel.Threads
{
    public class RoleThread : ThreadBase
    {
        public RoleThread()
            : base("Role thread", 100)
        {
        }

        protected override async Task OnProcessAsync()
        {
            await RoleManager.OnRoleTimerAsync();
        }
    }
}
