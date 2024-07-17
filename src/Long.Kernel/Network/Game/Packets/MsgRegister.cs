using Long.Kernel.Managers;
using Long.Kernel.States;
using Long.Network.Packets;
using static Long.Kernel.States.User.Character;
using static Long.Kernel.Network.Game.Packets.MsgTalk;
using Long.Kernel.Database.Repositories;
using Long.Database.Entities;
using Long.Kernel.Database;
using Long.Kernel.States.Items;
using Serilog;
using Long.Kernel.States.User;

namespace Long.Kernel.Network.Game.Packets
{
    /// <remarks>Packet Type 1001</remarks>
    /// <summary>
    ///     Message containing character creation details, such as the new character's name,
    ///     body size, and profession. The character name should be verified, and may be
    ///     rejected by the server if a character by that name already exists.
    /// </summary>
    public sealed class MsgRegister : MsgBase<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgRegister>();

        // Registration constants
        private static readonly byte[] Hairstyles =
        {
            10, 11, 13, 14, 15, 24, 30, 35, 37, 38, 39, 40
        };

        public string FirstTag { get; set; }
        public string Name { get; set; }
        public string SeccondTag { get; set; }
        public string MACAddress { get; set; }
        public uint Version { get; set; }
        public uint Lookface { get; set; }
        public ushort Class { get; set; }
        public uint Token { get; set; }
        public uint Param0 { get; set; }
        public uint Param1 { get; set; }

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
            FirstTag = reader.ReadString(16);               // 4
            Name = reader.ReadString(16);                   // 20
            SeccondTag = reader.ReadString(16);             // 36
            MACAddress = reader.ReadString(16);             // 52
            reader.BaseStream.Seek(28, SeekOrigin.Current); // 68
            Token = reader.ReadUInt32();                    // 98
            Lookface = reader.ReadUInt32();                 // 102
            Class = reader.ReadUInt16();                    // 104
            Version = reader.ReadUInt16();                  // 106
            Param0 = reader.ReadUInt32();                   // 108
            Param1 = reader.ReadUInt32();                   // 112
        }

        /// <summary>
        ///     Process can be invoked by a packet after decode has been called to structure
        ///     packet fields and properties. For the server implementations, this is called
        ///     in the packet handler after the message has been dequeued from the server's
        ///     <see cref="PacketProcessor{TClient}" />.
        /// </summary>
        /// <param name="client">Client requesting packet processing</param>
        public override async Task ProcessAsync(GameClientBase client)
        {
            // Validate that the player has access to character creation
            if (client.Creation == null || Token != client.Creation.Token || !RoleManager.Registration.Contains(Token))
            {
                await client.SendAsync(RegisterInvalid);
                client.Disconnect();
                return;
            }

            // Check character name availability
            if (await UserRepository.ExistsAsync(Name))
            {
                await client.SendAsync(RegisterNameTaken);
                return;
            }

            if (!RoleManager.IsValidName(Name))
            {
                await client.SendAsync(RegisterInvalid);
                return;
            }

            BaseClassType baseClass = (BaseClassType)Class;
            if (!Enum.IsDefined(typeof(BaseClassType), baseClass))
            {
                await client.SendAsync(RegisterInvalidProfession);
                return;
            }

            if (!Enum.IsDefined(typeof(BodyType), Lookface))
            {
                await client.SendAsync(RegisterInvalidBody);
                return;
            }

            DbPointAllot allot = PointAllotRepository.Get((ushort)((int)baseClass / 10), 1);
            if (allot == null)
            {
                logger.Error($"Could not load attribute points for profession [{baseClass}]");
                allot = new DbPointAllot
                {
                    Force = 4,
                    Dexterity = 6,
                    Health = 12,
                    Soul = 0
                };
            }

#if DEBUG
            if (Name.Length + 4 > 15)
            {
                Name = Name[..11];
            }
            Name += "[PM]";
#endif

            // Create the character
            var character = new DbUser
            {
                AccountIdentity = client.Creation.AccountId,
                Name = Name,
                Hairstyle = 101,
                Mate = "Nenhum",
                Medal = "Nenhum",
                Lookface = Lookface,

                MapId = 1000,
                X = 178,
                Y = 416,

                Profession = (byte)baseClass,
                Level = 1,
                AutoAllot = 1,
                Money = 10_000,
                MedalSelect = 2, // default eudemon summon!
                TutorLevel = 0,

                Power = allot.Force,
                Agility = allot.Dexterity,
                Vitality = allot.Health,
                Spirit = allot.Soul,
                HealthPoints = (ushort)(allot.Health * 10),
                ManaPoints = (ushort)(allot.Soul * 20),
                RegisterTime = uint.Parse(DateTime.Now.ToString("yyMMddHHmm")),
                EudPackSize = 3,
                ExtraHatchSize = 5,
                LastLoginTime = (uint)UnixTimestamp.Now,
            };

            try
            {
                await UserRepository.CreateAsync(character);
                RoleManager.Registration.Remove(client.Creation.Token);

                var args = new TransferAuthArgs
                {
                    AccountID = client.AccountIdentity,
                    AuthorityID = client.AuthorityLevel,
                    IPAddress = client.IpAddress
                };
                RoleManager.SaveLoginRequest(Token.ToString(), args);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                await client.SendAsync(RegisterTryAgain);
                return;
            }

            await client.SendAsync(RegisterOk);

            client.ReceiveTimeOutSeconds = 30; 
            client.Character = new Character(client, character);
            if (await RoleManager.LoginUserAsync(client))
            {
                await client.SendAsync(LoginOk);
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
