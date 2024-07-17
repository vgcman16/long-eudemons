using Long.Kernel.Managers;
using Long.Network.Packets.Login;

namespace Long.Game.Network.Login.Packets
{
    public class MsgLoginUserError : MsgLoginUserError<LoginServer>
    {
        public override Task ProcessAsync(LoginServer client)
        {
            var user = RoleManager.GetUserByAccount(AccountId);
            if (user != null)
            {
                return user.OnLogoutAsync();
            }

            return Task.CompletedTask;
        }
    }
}
