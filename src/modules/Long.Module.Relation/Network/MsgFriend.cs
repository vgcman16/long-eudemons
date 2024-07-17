using Long.Kernel.Managers;
using Long.Kernel.Network.Game;
using Long.Kernel.States.User;
using Long.Network.Packets;
using static Long.Kernel.States.User.Character;
using static Long.Kernel.StrRes;

namespace Long.Module.Relation.Network
{
    public sealed class MsgFriend : MsgBase<GameClientBase>
    {
        public uint Identity { get; set; }
        /// <summary>
        /// Can be battle power or lookface, on add the server send the lookface.
        /// </summary>
        public uint Data { get; set; }
        public MsgFriendAction Action { get; set; }
        public bool Online { get; set; }
        public ushort Level { get; set; }
        public int Nobility { get; set; }
        public int Gender { get; set; }
        public string Name { get; set; }

#if DEBUG_PALADIN
        /// <summary>
        ///     Decodes a byte packet into the packet structure defined by this message class.
        ///     Should be invoked to structure data from the client for processing. Decoding
        ///     follows TQ Digital's byte ordering rules for an all-binary protocol.
        /// </summary>
        /// <param name="bytes">Bytes from the packet processor or client socket</param>
        public override void Decode(byte[] bytes)
        {
            using var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();                   // 0
            Type = (PacketType)reader.ReadUInt16();         // 2
            Identity = reader.ReadUInt32();                 // 4
            Data = reader.ReadUInt32();                     // 8
            Action = (MsgFriendAction)reader.ReadByte();    // 12
            Online = reader.ReadBoolean();                  // 13
            Level = reader.ReadUInt16();                    // 14
            Name = reader.ReadString(16);                   // 16
            //reader.ReadInt32();                             // 32
        }

        /// <summary>
        ///     Encodes the packet structure defined by this message class into a byte packet
        ///     that can be sent to the client. Invoked automatically by the client's send
        ///     method. Encodes using byte ordering rules interoperable with the game client.
        /// </summary>
        /// <returns>Returns a byte packet of the encoded packet.</returns>
        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgFriend);
            writer.Write(Identity);                 // 4
            writer.Write(Data);                     // 8
            writer.Write((byte)Action);             // 12
            writer.Write(Online);                   // 13
            writer.Write(Level);                    // 14
            writer.Write(Name ?? string.Empty, 16); // 16
            //writer.Write(0);                        // 32
            return writer.ToArray();
        }
#else

        /// <summary>
        ///     Decodes a byte packet into the packet structure defined by this message class.
        ///     Should be invoked to structure data from the client for processing. Decoding
        ///     follows TQ Digital's byte ordering rules for an all-binary protocol.
        /// </summary>
        /// <param name="bytes">Bytes from the packet processor or client socket</param>
        public override void Decode(byte[] bytes)
        {
            using var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();                   // 0
            Type = (PacketType)reader.ReadUInt16();         // 2
            Identity = reader.ReadUInt32();                 // 4
            Data = reader.ReadUInt32();                     // 8
            Action = (MsgFriendAction)reader.ReadByte();    // 12
            Online = reader.ReadBoolean();                  // 13
            Level = reader.ReadUInt16();                    // 14
            Name = reader.ReadString(16);                   // 16
            reader.ReadInt32();                             // 32
        }

        /// <summary>
        ///     Encodes the packet structure defined by this message class into a byte packet
        ///     that can be sent to the client. Invoked automatically by the client's send
        ///     method. Encodes using byte ordering rules interoperable with the game client.
        /// </summary>
        /// <returns>Returns a byte packet of the encoded packet.</returns>
        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgFriend);
            writer.Write(Identity);                 // 4
            writer.Write(Data);                     // 8
            writer.Write((byte)Action);             // 12
            writer.Write(Online);                   // 13
            writer.Write(Level);                    // 14
            writer.Write(Name ?? string.Empty, 16); // 16
            writer.Write(0);                        // 32
            return writer.ToArray();
        }
#endif
        public enum MsgFriendAction : byte
        {
            RequestFriend = 10,
            NewFriend = 11,
            SetOnlineFriend = 12,
            SetOfflineFriend = 13,
            RemoveFriend = 14,
            AddFriend = 15,
            SetOnlineEnemy = 16,
            SetOfflineEnemy = 17,
            RemoveEnemy = 18,
            AddEnemy = 19
        }

        public override async Task ProcessAsync(GameClientBase client)
        {
            Character user = client.Character;
            Character target = RoleManager.GetUser(Identity);

            switch (Action)
            {
                case MsgFriendAction.RequestFriend:
                    {
                        if (target == null)
                        {
                            await user.SendAsync(StrTargetNotOnline);
                            return;
                        }

                        if (user.Relation.FriendAmount >= user.Relation.MaximumFriendAmount)
                        {
                            await user.SendAsync(StrFriendListFull);
                            return;
                        }

                        if (target.Relation.FriendAmount >= target.Relation.MaximumFriendAmount)
                        {
                            await user.SendAsync(StrTargetFriendListFull);
                            return;
                        }

                        uint request = target.QueryRequest(RequestType.Friend);
                        if (request == user.Identity)
                        {
                            target.PopRequest(RequestType.Friend);
                            await target.Relation.AddFriendAsync(user.Identity);
                        }
                        else
                        {
                            user.SetRequest(RequestType.Friend, target.Identity);
                            await target.SendAsync(new MsgFriend
                            {
                                Action = MsgFriendAction.RequestFriend,
                                Identity = user.Identity,
                                Data = (uint)user.BattlePower,
                                Level = user.Level,
                                Name = user.Name,
                            });
                            await user.SendAsync(StrMakeFriendSent);
                        }

                        break;
                    }

                case MsgFriendAction.NewFriend:
                    {
                        if (target == null)
                        {
                            await user.SendAsync(StrTargetNotOnline);
                            return;
                        }

                        if (user.Relation.FriendAmount >= user.Relation.MaximumFriendAmount)
                        {
                            await user.SendAsync(StrFriendListFull);
                            return; 
                        }

                        if (target.Relation.FriendAmount >= target.Relation.MaximumFriendAmount)
                        {
                            await user.SendAsync(StrTargetFriendListFull);
                            return;
                        }

                        uint request = target.QueryRequest(RequestType.Friend);
                        if (request == user.Identity)
                        {
                            target.PopRequest(RequestType.Friend);
                            await target.Relation.AddFriendAsync(user.Identity);
                        }
                        break;
                    }

                case MsgFriendAction.RemoveFriend:
                    {
                        await user.Relation.DeleteFriendAsync(Identity);
                        break;
                    }

                case MsgFriendAction.RemoveEnemy:
                    {
                        await user.Relation.DeleteEnemyAsync(Identity);
                        break;
                    }
                case (MsgFriendAction)21:
                    {
                        user.PopRequest(RequestType.Friend);
                        await user.SendAsync(this);
                        if (target != null)
                        {
                            target.PopRequest(RequestType.Friend);
                            await target.SendAsync(this);
                        }
                        break;
                    }
                default:
                    {
                        Console.WriteLine($"MsgFriend()->ProcessAsync() Unknown action: {Action}");
                        break;
                    }
            }
        }
    }
}
