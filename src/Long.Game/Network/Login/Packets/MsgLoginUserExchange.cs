using Long.Kernel.Managers;
using Long.Kernel.States;
using Long.Kernel.States.User;
using Long.Network.Packets.Login;
using Serilog;
using System.Security.Cryptography;

namespace Long.Game.Network.Login.Packets
{
    public sealed class MsgLoginUserExchange : MsgLoginUserExchange<LoginServer>
    {
        private static readonly ILogger logger = Log.ForContext<MsgLoginUserExchange>();

        public override async Task ProcessAsync(LoginServer client)
        {
            Character user = RoleManager.GetUserByAccount(Data.AccountId);
            if (user != null)
            {
                logger.Warning("User {0} is already logged in.", Data.AccountId);
                RoleManager.KickOutAsync(user.Identity, "Duplicated login").GetAwaiter().GetResult();
                //await client.SendAsync(new MsgLoginUserExchangeEx
                //{
                //    Data = new MsgLoginUserExchangeEx.LoginExchangeData
                //    {
                //        Response = MsgLoginUserExchangeEx.SUCCESS
                //    }
                //});
            }

            // Generate the access token
            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            var token = BitConverter.ToUInt32(bytes);
            var args = new TransferAuthArgs
            {
                AccountID = Data.AccountId,
                AuthorityID = Data.AuthorityId,
                IPAddress = Data.IpAddress,
                VIPLevel = Data.VipLevel,
                Seed = Data.Seed,
            };
            RoleManager.SaveLoginRequest(token.ToString(), args);

#if DEBUG
            logger.Debug("Account {0} from {1} is queued (with token: {2}) for a transfer request.", Data.AccountId, Data.IpAddress, token);
#endif

            await client.SendAsync(new MsgLoginUserExchangeEx
            {
                Data = new MsgLoginUserExchangeEx.LoginExchangeData
                {
                    AccountId = Data.AccountId,
                    Request = Data.Request,
                    Response = MsgLoginUserExchangeEx.SUCCESS,
                    Token = token
                }
            });
        }
    }
}