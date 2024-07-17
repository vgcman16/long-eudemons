using Long.Login.Managers;
using Long.Login.Network.Game.Packets;
using Long.Network.Packets.Game;

namespace Long.Login.Network.Login.Packets
{
    public class MsgConnect : MsgConnect<LoginClient>
    {
        private static readonly ILogger logger = Log.ForContext<MsgConnect>();

        public override async Task ProcessAsync(LoginClient client)
        {
            logger.Information($"[{client.AccountID}] logged in using the client version: {Message}");
            client.Disconnect();
            //if (Message.Equals("ERROR"))
            //{
            //    var realm = RealmManager.GetRealm("Genesis");
            //    if (realm != null)
            //    {
            //        await realm.Client.SendAsync(new MsgLoginUserError(client.AccountID));
            //    }
            //}
            //else
            //{
            //    // log version
            //    // Log data
            //}
        }
    }
}
