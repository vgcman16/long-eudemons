using Long.Login.Database.Repositories;
using Long.Login.Managers;
using Long.Login.Network.Game.Packets;
using Long.Login.States;
using Long.Network.Packets;
using Long.Network.Security;
using System.Security.Cryptography;
using System.Text;

namespace Long.Login.Network.Login.Packets
{
    public sealed class MsgAccount : MsgBase<LoginClient>
    {
        private static readonly ILogger logger = Log.ForContext<MsgAccount>();

        // Packet Properties
        public string Username { get; private set; }
        public byte[] Password { get; private set; }
        public string Realm { get; private set; }

        /// <summary>
        ///     Decodes a byte packet into the packet structure defined by this message class.
        ///     Should be invoked to structure data from the client for processing. Decoding
        ///     follows TQ Digital's byte ordering rules for an all-binary protocol.
        /// </summary>
        /// <param name="bytes">Bytes from the packet processor or client socket</param>
        public override void Decode(byte[] bytes)
        {
            using var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            reader.BaseStream.Seek(4, SeekOrigin.Begin);
            Username = reader.ReadString(16);
            reader.BaseStream.Seek(132, SeekOrigin.Begin);
            Password = reader.ReadBytes(16);
            reader.BaseStream.Seek(260, SeekOrigin.Begin);
            Realm = reader.ReadString(16);
        }

        public override async Task ProcessAsync(LoginClient client)
        {
            LoginStatisticManager.IncreaseLogin();

            try
            {
                var gameAccount = AccountRepository.GetByUsername(Username);
                if (gameAccount == null)
                {
                    logger.Warning("Username {0} do not exist.", Username);
                    await client.DisconnectWithRejectionCodeAsync(MsgConnectEx.RejectionCode.InvalidAccount);
                    return;
                }

                string password = Encoding.UTF8.GetString(Password).TrimEnd('\0');
                if (!gameAccount.Password.Equals(password))
                {
                    logger.Warning("User {0} has attempted to login with an invalid password.", Username);
                    await client.DisconnectWithRejectionCodeAsync(MsgConnectEx.RejectionCode.InvalidPassword);
                    return;
                }

                if (gameAccount.Authority == 12)
                {
                    logger.Warning("User {0} is locked.", Username);
                    await client.DisconnectWithRejectionCodeAsync(MsgConnectEx.RejectionCode.AccountBanned);
                    return;
                }

#if DEBUG
                if (gameAccount.Authority != 2)
                {
                    logger.Warning("User {0} non cooperator account.", Username);
                    await client.DisconnectWithRejectionCodeAsync(MsgConnectEx.RejectionCode.NonCooperatorAccount);
                    return;
                }
#endif

                var realm = RealmManager.GetRealm(Realm);
                if (realm == null)
                {
                    logger.Warning("User {0} attempted to connecto to unexistent {1} realm.", Username, Realm);
                    await client.DisconnectWithRejectionCodeAsync(MsgConnectEx.RejectionCode.ServerDown);
                    return;
                }

                if (!realm.IsProduction && gameAccount.Authority == 1)
                {
                    logger.Warning("User {0} non cooperator account on not production realm.", Username);
                    await client.DisconnectWithRejectionCodeAsync(MsgConnectEx.RejectionCode.NonCooperatorAccount);
                    return;
                }

                client.AccountID = (uint)gameAccount.Identity;
                client.Username = gameAccount.Username;

                User user = UserManager.GetUser(client.AccountID);
                if (user != null)
                {
                    // duplicated login request
                    logger.Warning("User {0} is already awaiting for a login response.", Username);
                    await client.DisconnectWithRejectionCodeAsync(MsgConnectEx.RejectionCode.PleaseTryAgainLater);
                    return;
                }

                user = new User(client, gameAccount, realm);
                UserManager.AddUser(user);
                await realm.Client.SendAsync(new MsgLoginUserExchange
                {
                    Data = new MsgLoginUserExchange.LoginExchangeData
                    {
                        AccountId = client.AccountID,
                        AuthorityId = gameAccount.Authority,
                        IpAddress = client.IpAddress,
                        Request = client.Guid.ToString(),
                        Seed = client.Seed,
#if DEBUG
                        //VipLevel = 6
#endif
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error on MsgAccount processing! {0}", ex.Message);
                if (client.Socket.Connected)
                {
                    await client.DisconnectWithRejectionCodeAsync(MsgConnectEx.RejectionCode.PleaseTryAgainLater);
                }
            }
        }

        /// <summary>
        ///     Decrypts the password from read in packet bytes for the <see cref="Decode" />
        ///     method. Trims the end of the password string of null terminators.
        /// </summary>
        /// <param name="buffer">Bytes from the packet buffer</param>
        /// <param name="seed">Seed for generating RC5 keys</param>
        /// <returns>Returns the decrypted password string.</returns>
        private string DecryptPassword(byte[] buffer, uint seed)
        {
            var rc5 = new RC5(seed);
            var scanCodes = new ScanCodeCipher(Username);
            var password = new byte[16];
            rc5.Decrypt(buffer, password);
            scanCodes.Decrypt(password, password);
            return Encoding.ASCII.GetString(password).Trim('\0');
        }
    }
}
