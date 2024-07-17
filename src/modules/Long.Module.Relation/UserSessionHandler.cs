using Long.Kernel.Database;
using Long.Kernel.Modules.Interfaces;
using Long.Kernel.States.User;
using Long.Module.Relation.States;

namespace Long.Module.Relation
{
    public sealed class UserSessionHandler : IUserSessionHandler, IUserDeletedHandler
    {
        public Task OnUserLoginAsync(Character user)
        {
            user.Relation = new Relationship(user);
            return user.Relation.InitializeAsync();
        }

        public Task OnUserLoginCompleteAsync(Character user)
        {
            if (user.Relation == null)
            {
                return Task.CompletedTask;
            }

            return user.Relation.DoOnlineNotificationAsync();
        }

        public Task OnUserLogoutAsync(Character user)
        {
            if (user.Relation == null)
            {
                return Task.CompletedTask;
            }

            return user.Relation.DoOfflineNotificationAsync();
        }

        public Task OnUserDeletedAsync(Character user, ServerDbContext ctx)
        {
            return user.Relation.OnUserDeleteAsync(ctx);
        }
    }
}
