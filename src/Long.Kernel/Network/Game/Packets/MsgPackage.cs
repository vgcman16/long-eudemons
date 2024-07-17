using Long.Kernel.Managers;
using Long.Kernel.States;
using Long.Kernel.States.Items;
using Long.Kernel.States.Npcs;
using Long.Kernel.States.User;
using Long.Network.Packets;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgPackage : MsgBase<GameClientBase>
    {
        public MsgPackage()
        {
        }

        public MsgPackage(ItemBase item, WarehouseMode action, StorageType mode)
        {
            Identity = item.OwnerIdentity;
            Action = action;
            Mode = mode;
            if (action == WarehouseMode.CheckOut)
            {
                Param = item.Identity;
            }
            else
            {
                Items = new List<WarehouseItem>
                {
                    new WarehouseItem
                    {
                        Identity = item.Identity,
                        Type = item.Type,
                        SocketOne = (byte)item.SocketOne,
                        SocketTwo = (byte)item.SocketTwo,
                        Ident = item.Ident,
                        Magic1 = (byte)item.Magic1,
                        Magic2 = item.Magic2,
                        Magic3 = item.Plus,
                        Amount = item.Amount,
                        AmountLimit = item.AmountLimit,
                        LockType = item.Locked,
                        WarGhostExp = item.WarGhostExp,
                        ActivationTime = (uint)item.AvailableTime,
                        EarthAttr = item.EarthAttr,
                        FireAttr = item.FireAttr,
                        WaterAttr = item.WaterAttr,
                        AirAttr = item.AirAttr,
                        SpecialAttr = item.SpecialAttr,
                        GiftFlag = (byte)(item.IsBound ? 2 : 0),                        
                        Name = item.Name
                    }
                };
            }
        }

        public List<WarehouseItem> Items = new();
        public uint Identity { get; set; }
        public WarehouseMode Action { get; set; }
        public StorageType Mode { get; set; }
        public ushort Size { get; set; }
        public uint Data { get; set; }
        public uint Param { get; set; }

        public override void Decode(byte[] bytes)
        {
            using var reader = new PacketReader(bytes);
            Length = reader.ReadUInt16();
            Type = (PacketType)reader.ReadUInt16();
            Identity = reader.ReadUInt32();             // 4
            Action = (WarehouseMode)reader.ReadByte();  // 8
            Mode = (StorageType)reader.ReadByte();      // 9
            Size = reader.ReadUInt16();                 // 10
            Data = reader.ReadUInt32();                 // 12
            Param = reader.ReadUInt32();                // 16
        }

        public override byte[] Encode()
        {
            using var writer = new PacketWriter();
            writer.Write((ushort)PacketType.MsgPackage);
            writer.Write(Identity);             // 4
            writer.Write((byte)Action);         // 8
            writer.Write((byte)Mode);           // 9
            writer.Write(Size);                 // 10
            writer.Write(Data);                 // 12
            if (Items.Count > 0)
            {
                writer.Write(Param = (uint)Items.Count);      // 16
                foreach (WarehouseItem item in Items)         // 80
                {
                    writer.Write(item.Identity);              // 0
                    writer.Write(item.Type);                  // 4
                    writer.Write(item.Amount);                // 8
                    writer.Write(item.AmountLimit);           // 10
                    writer.Write(item.Ident);                 // 12
                    writer.Write(item.SocketOne);             // 13
                    writer.Write(item.SocketTwo);             // 14
                    writer.Write(item.Magic1);                // 15
                    writer.Write(item.Magic2);                // 16
                    writer.Write(item.Magic3);                // 17
                    writer.Write((ushort)0);                  // 18
                    writer.Write(item.Data);                  // 20
                    writer.Write(item.LockType);              // 24
                    writer.Write(item.WarGhostExp);           // 28
                    writer.Write(item.GemAtkType);            // 32
                    writer.Write(item.ActivationTime);        // 36
                    writer.Write(item.EarthAttr);             // 40
                    writer.Write(item.FireAttr);              // 41
                    writer.Write(item.WaterAttr);             // 42
                    writer.Write(item.AirAttr);               // 43
                    writer.Write(item.SpecialAttr);           // 44
                    writer.Write(item.GiftFlag);              // 45
                    writer.Write(new byte[18]);               // 46
                    writer.Write(item.Name, 16);              // 64
                }
            }
            else
            {
                writer.Write(Param);                          // 16
            }

            return writer.ToArray();
        }

        public struct WarehouseItem
        {
            public uint Identity;
            public uint Type;
            public ushort Amount;
            public ushort AmountLimit;
            public byte Ident;
            public byte SocketOne;
            public byte SocketTwo;
            public byte Magic1;
            public byte Magic2;
            public byte Magic3;
            public uint Data;
            public uint LockType;
            public uint WarGhostExp;
            public uint GemAtkType;
            public uint ActivationTime;
            public byte EarthAttr;
            public byte FireAttr;
            public byte WaterAttr;
            public byte AirAttr;
            public byte SpecialAttr;
            public byte GiftFlag;
            public string Name;
        }

        public enum StorageType : byte
        {
            None = 0,
            Storage = 10,
            Trunk = 20,
            Chest = 30,
            EudemonBrooder = 40,
            EudemonStorage = 50,
            AuctionStorage = 60,
            EudemonBrooderEx = 70, // User Incubator
            EudemonExtraBrooder = 120,
        }

        public enum WarehouseMode : byte
        {
            Query = 0,
            CheckIn,
            CheckOut,
            Query2
        }

        public override async Task ProcessAsync(GameClientBase client)
        {
            Character user = client.Character;
            if (!user.IsUnlocked())
            {
                await user.SendSecondaryPasswordInterfaceAsync();
                return;
            }

            BaseNpc npc = null;
            if (Mode == StorageType.Storage || Mode == StorageType.Trunk)
            {
                npc = RoleManager.GetRole(Identity) as BaseNpc;
                if (npc == null)
                {
                    if (user.IsPm())
                    {
                        await user.SendAsync($"Could not find Storage NPC, {Identity}");
                    }

                    return;
                }
            }
            else if (Mode == StorageType.Chest && Action == WarehouseMode.CheckIn)
            {
                return;
            }

            if (Action == WarehouseMode.Query)
            {
                Data = user.Identity;
                Size = (ushort)(npc.Data3 != 0 ? npc.Data3 : Package.PACKAGE_LIMIT);
                var storageItems = user.Packages.GetStorageItems(Identity, Mode);
                foreach (var expiredItem in storageItems.Values.Where(x => x.HasExpired()).ToList())
                {
                    storageItems.TryRemove(expiredItem.Identity, out _);
                    await expiredItem.DeleteAsync();
                }

                foreach (ItemBase item in storageItems.Values)
                {
                    Items.Add(new WarehouseItem
                    {
                        Identity = item.Identity,
                        Type = item.Type,
                        SocketOne = (byte)item.SocketOne,
                        SocketTwo = (byte)item.SocketTwo,
                        Ident = item.Ident,
                        Magic1 = (byte)item.Magic1,
                        Magic2 = item.Magic2,
                        Magic3 = item.Plus,
                        Amount = item.Amount,
                        AmountLimit = item.AmountLimit,
                        Data = item.Data,
                        LockType = item.Locked,
                        WarGhostExp = item.WarGhostExp,
                        ActivationTime = (uint)item.AvailableTime,
                        EarthAttr = item.EarthAttr,
                        FireAttr = item.FireAttr,
                        WaterAttr = item.WaterAttr,
                        AirAttr = item.AirAttr,
                        SpecialAttr = item.SpecialAttr,
                        GiftFlag = item.GiftFlag,
                        Name = item.Name
                    });

                    if (Items.Count >= 7)
                    {
                        await user.SendAsync(this);
                        Items.Clear();
                        Action = WarehouseMode.Query2;
                        continue;
                    }
                }

                await user.SendAsync(this);
            }
            else if (Action == WarehouseMode.CheckIn)
            {
                ItemBase storeItem = user.UserPackage.GetItem(Param);
                if (storeItem == null)
                {
                    await user.SendAsync(StrItemNotFound);
                    return;
                }

                if (!storeItem.CanBeStored())
                {
                    await user.SendAsync(StrItemCannotBeStored);
                    return;
                }

                if (storeItem.HasExpired())
                {
                    await storeItem.ExpireAsync();
                    return;
                }

                //if (Mode == StorageType.Storage && npc?.IsStorageNpc() != true)
                //{
                //    return;
                //}

                //if (Mode == StorageType.Trunk && npc?.Type != BaseNpc.TRUNCK_NPC)
                //{
                //    return;
                //}

                await user.Packages.AddItemAsync(Identity, storeItem, Mode, true);
            }
            else if (Action == WarehouseMode.CheckOut)
            {
                await user.Packages.RemoveItemAsync(Identity, Param, Mode, true);
            }
        }
    }
}
