using Long.Login.Network.Login;
using Long.Network.Packets.Login;

namespace Long.Login.Network.Game.Packets
{
    public class MsgLoginUserError : MsgLoginUserError<GameClient>
    {
        public MsgLoginUserError(uint accountId)
        {
            AccountId = accountId;
        }
    }
}
