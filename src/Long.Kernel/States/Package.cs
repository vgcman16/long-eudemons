using Long.Kernel.Managers;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.Items;
using Long.Kernel.States.Npcs;
using Long.Kernel.States.User;
using System.Collections.Concurrent;
using static Long.Kernel.Network.Game.Packets.MsgPackage;
using static Long.Kernel.States.User.UserPackage;

namespace Long.Kernel.States
{
    public class Package
    {
        public const int PACKAGE_LIMIT = 50;
        public const int PACKAGE_AUCTION_LIMIT = 5;

        private readonly ConcurrentDictionary<uint, ConcurrentDictionary<uint, ItemBase>> warehouses = new();
        private readonly ConcurrentDictionary<uint, ConcurrentDictionary<uint, ItemBase>> sashes = new();

        private readonly Character user;

        public Package(Character user)
        {
            this.user = user;
        }

        public ConcurrentDictionary<uint, ItemBase> GetStorageItems(uint storeId, StorageType type)
        {
            switch (type)
            {
                case StorageType.AuctionStorage:
                case StorageType.Storage:
                case StorageType.Trunk:
                    return warehouses.TryGetValue(storeId, out var storage) ? storage : new();
                case StorageType.Chest:
                    return sashes.TryGetValue(storeId, out var chest) ? chest : new();
            }

            return new();
        }

        public int StorageSize(uint storeId, StorageType type)
        {
            switch (type)
            {
                case StorageType.AuctionStorage:
                case StorageType.Storage:
                    return warehouses.TryGetValue(storeId, out var storage) ? storage.Count : 0;
                case StorageType.Chest:
                    return sashes.TryGetValue(storeId, out var chest) ? chest.Count : 0;
            }

            return 0;
        }

        #region Merge

        public ItemBase FindStorageCombineItem(ItemBase item)
        {
            return GetStorageItems(item.OwnerIdentity, (StorageType)((int)item.Position % 10 * 10)).Values
                        .FirstOrDefault(
                            x => x.Type == item.Type
                            && x.AccumulateNum < x.MaxAccumulateNum
                            && x.IsBound == item.IsBound);
        }

        public async Task<bool> CombineStorageItemAsync(ItemBase item, ItemBase combine)
        {
            if (item == null || combine == null || !item.IsPileEnable() || item.Type != combine.Type)
            {
                return false;
            }

            if (item.IsBound && !combine.IsBound)
            {
                return false;
            }

            int newNum = item.AccumulateNum + combine.AccumulateNum;
            if (newNum > item.MaxAccumulateNum)
            {
                item.AccumulateNum = (ushort)(newNum - combine.MaxAccumulateNum);
                combine.AccumulateNum = item.MaxAccumulateNum;
                await user.SendAsync(new MsgPackage(item, WarehouseMode.Query, (StorageType)((int)item.Position % 10 * 10)));
                await user.SendAsync(new MsgPackage(combine, WarehouseMode.Query, (StorageType)((int)item.Position % 10 * 10)));
                await item.SaveAsync();
                await combine.SaveAsync();
            }
            else
            {
                item.AccumulateNum = 0;
                await user.UserPackage.RemoveFromInventoryAsync(item, RemovalType.Delete);
                combine.AccumulateNum = (ushort)newNum;
                await user.SendAsync(new MsgPackage(combine, WarehouseMode.Query, (StorageType)((int)item.Position % 10 * 10)));
                await combine.SaveAsync();
            }
            return true;
        }

        #endregion

        public async Task<bool> AddItemAsync(uint storeId, ItemBase item, StorageType mode, bool sync)
        {
            if (item.Position != ItemBase.ItemPosition.Inventory && sync)
            {
                return false;
            }

            ItemBase chestItem;
            BaseNpc npc = null;
            ConcurrentDictionary<uint, ItemBase> items = null;
            int maxStorage;
            if (mode == StorageType.Storage || mode == StorageType.Trunk || mode == StorageType.EudemonBrooder)
            {
                npc = RoleManager.GetRole(storeId) as BaseNpc;
                if (npc == null)
                {
                    return false;
                }

                if (npc.Data3 != 0)
                {
                    maxStorage = npc.Data3;
                }
                else
                {
                    maxStorage = PACKAGE_LIMIT;
                }

                if (!warehouses.TryGetValue(storeId, out items))
                {
                    warehouses.TryAdd(storeId, items = new());
                }
            }
            else if (mode == StorageType.Chest)
            {
                chestItem = user.UserPackage.GetItem(storeId);
                if (chestItem == null || chestItem.GetItemSort() != (ItemBase.ItemSort)11)
                {
                    return false;
                }

                maxStorage = (int)(chestItem.Type % 1000);
                if (maxStorage == 9)
                {
                    maxStorage += 3;
                }
                if (!sashes.TryGetValue(storeId, out items))
                {
                    sashes.TryAdd(storeId, items = new());
                }
            }
            else if (mode == StorageType.AuctionStorage)
            {
                maxStorage = PACKAGE_AUCTION_LIMIT;
                if (!warehouses.TryGetValue(storeId, out items))
                {
                    warehouses.TryAdd(storeId, items = new());
                }
            }
            else
            {
                if (user.IsPm())
                {
                    await user.SendAsync($"AddToStorageAsync::Invalid storage type: {mode}");
                }

                return false;
            }

            if (!item.CanBeStored())
            {
                return false;
            }

            if (items == null || StorageSize(storeId, mode) >= maxStorage)
            {
                return false;
            }

            if (sync && !await user.UserPackage.RemoveFromInventoryAsync(item, RemovalType.RemoveAndDisappear))
            {
                return false;
            }

            item.OwnerIdentity = storeId;
            item.Position = (ItemBase.ItemPosition)(200 + (byte)mode / 10);
            if (item.IsCountable() && mode != StorageType.AuctionStorage)
            {
                ItemBase combine = FindStorageCombineItem(item);
                if (combine != null)
                {
                    do { await CombineStorageItemAsync(item, combine); }
                    while (item.AccumulateNum > 1 && item.AccumulateNum > item.MaxAccumulateNum && (combine = FindStorageCombineItem(item)) != null);

                    if (item.AccumulateNum == 0)
                    {
                        await user.UserPackage.RemoveFromInventoryAsync(item, RemovalType.Delete);
                        return true;
                    }
                }
            }

            items.TryAdd(item.Identity, item);
            await item.SaveAsync();
            if (sync)
            {
                await user.SendAsync(new MsgPackage(item, WarehouseMode.CheckIn, mode));
                await item.TryUnlockAsync();
            }
            return true;
        }

        public async Task<bool> RemoveItemAsync(uint storeId, uint itemId, StorageType mode, bool sync)
        {
            ConcurrentDictionary<uint, ItemBase> storage = null;
            if (mode == StorageType.Storage || mode == StorageType.Trunk || mode == StorageType.AuctionStorage)
            {
                if (!warehouses.TryGetValue(storeId, out storage))
                {
                    return false;
                }
            }
            else if (mode == StorageType.Chest)
            {
                if (!sashes.TryGetValue(storeId, out storage))
                {
                    return false;
                }
            }

            if (storage == null)
            {
                return false;
            }

            if (!storage.ContainsKey(itemId))
            {
                return false;
            }

            if (!user.UserPackage.IsPackSpare(1))
            {
                return false;
            }

            ItemBase item;
            if (storage == null)
            {
                return false;
            }

            if (!storage.TryRemove(itemId, out item))
            {
                return false;
            }

            if (sync)
            {
                await user.SendAsync(new MsgPackage(item, WarehouseMode.CheckOut, mode));
            }

            await user.UserPackage.AddItemAsync(item);
            if (item.HasExpired())
            {
                await item.ExpireAsync();
                return false;
            }

            return true;
        }
    }
}
