using Long.Database.Entities;
using Long.Kernel.Database.Repositories;
using Long.Kernel.Managers;
using Long.Kernel.States;
using Long.Kernel.States.Registration;
using Long.Kernel.States.User;
using Long.Network.Packets.Game;

namespace Long.Kernel.Network.Game.Packets
{
    public class MsgConnect : MsgConnect<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgConnect>();

        public override async Task ProcessAsync(GameClientBase client)
        {
            TransferAuthArgs auth;
            if (client.Creation != null)
            {
                uint token = (uint)AccountId;
                if (token != client.Creation.Token)
                {
                    await client.DisconnectWithMessageAsync(MsgTalk.LoginInvalid);
                    logger.Warning("Invalid client creation Token: {0} from {1}", AccountId, client.IpAddress);
                    return;
                }

                auth = RoleManager.GetLoginRequest(token.ToString());
                if (auth == null)
                {
                    await client.DisconnectWithMessageAsync(MsgTalk.LoginInvalid);
                    logger.Warning("Invalid creation Token: {0} from {1}", AccountId, client.IpAddress);
                    return;
                }

                RoleManager.RemoveLoginRequest(token.ToString());
                client.Creation = null;
            }
            else
            {
                auth = RoleManager.GetLoginRequest(AccountId.ToString());
                if (auth == null)
                {
                    await client.DisconnectWithMessageAsync(MsgTalk.LoginInvalid);
                    logger.Warning("Invalid Login Token: {0} from {1}", AccountId, client.IpAddress);
                    return;
                }

                RoleManager.RemoveLoginRequest(AccountId.ToString());
            }

            client.AccountIdentity = auth.AccountID;
            client.AuthorityLevel = auth.AuthorityID;
            client.Seed = auth.Seed;
            client.Cipher?.GenerateKeys(new object[] { AccountId, Data });

            DbUser character = await UserRepository.FindAsync(auth.AccountID);
            if (character == null)
            {
                var delUser = await UserRepository.FindDeletedAsync(auth.AccountID);
                if (delUser != null)
                {
                    //await client.SendAsync(new MsgPlayerRestore()
                    //{
                    //    Identity = delUser.Identity,
                    //    Level = delUser.Level,
                    //    Profession = delUser.Profession,
                    //    Name = delUser.Name,
                    //});

                    client.Creation = new AwaitingCreation { AccountId = auth.AccountID, Token = (uint)AccountId };
                    RoleManager.Registration.Add(client.Creation.Token);
                    return;
                }

                // Create a new character
                client.Creation = new AwaitingCreation { AccountId = auth.AccountID, Token = (uint)AccountId };
                RoleManager.Registration.Add(client.Creation.Token);
                await client.SendAsync(MsgTalk.LoginNewRole);
                return;
            }

            // The character exists, so we will turn the timeout back.
            client.ReceiveTimeOutSeconds = 30; // 30 seconds or DC
            client.Character = new Character(client, character)
            {
                VipLevel = (uint)auth.VIPLevel
            };

            if (await RoleManager.LoginUserAsync(client))
            {
                await client.SendAsync(MsgTalk.LoginOk);
                await client.SendAsync(new MsgUserInfo(client.Character));

                try
                {
                    await OnUserLoginAsync(client.Character);

                    await client.Character.UserPackage.InitializeAsync();
                    await client.Character.Statistic.InitializeAsync();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Could not initialize user ({1} {2}) data! {0}", ex.Message, client.Character.Identity, client.Character.Name);
                    await client.DisconnectWithMessageAsync(MsgConnectEx.RejectionCode.DatabaseError);
                    return;
                }
            }
        }
    }
}
