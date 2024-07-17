using Long.Network.Packets.Login;
using Serilog;

namespace Long.Game.Network.Login.Packets
{
    public sealed class MsgLoginRealmAuthEx : MsgLoginRealmAuthEx<LoginServer>
    {
        private static readonly ILogger logger = Log.ForContext<MsgLoginRealmAuthEx>();

        public override async Task ProcessAsync(LoginServer client)
        {
            switch (Data.Code)
            {
                case ResponseCode.Success:
                    {
                        logger.Information("Connected to account server!");
                        break;
                    }

                default:
                    {
                        logger.Error("Failed to connect to account server: {0}", Data.Code);
                        client.Disconnect(); // if not already disconnected
                        break;
                    }
            }
        }
    }
}
