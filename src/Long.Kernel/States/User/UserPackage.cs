using Long.Database.Entities;
using Long.Kernel.Database;
using Long.Kernel.Database.Repositories;
using Long.Kernel.Managers;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.Scripting.Action;
using Long.Kernel.States.Items;
using Long.Kernel.States.Npcs;
using Long.Shared.Helpers;
using Long.Shared.Managers;
using System.Collections.Concurrent;
using System.Drawing;
using static Long.Kernel.Network.Game.Packets.MsgAction;
using static Long.Kernel.Network.Game.Packets.MsgPackage;
using static Long.Kernel.States.Items.ItemBase;

namespace Long.Kernel.States.User
{
    public sealed class UserPackage
    {
        private static readonly ILogger logger = Log.ForContext<UserPackage>();
        private static readonly ILogger spendMeteorLogger = Logger.CreateLogger("spend_meteor");
        private static readonly ILogger spendDragonBallLogger = Logger.CreateLogger("spend_dragonball");
        private static readonly ILogger awardItemErrorLogger = Logger.CreateLogger("award_item_error");

        private const int USERITEM_PAKC_ID = (int)ItemPosition.Inventory - 50;
        private const int GHOSTGEM_PAKC_ID = (int)ItemPosition.GhostGemPack - 50;
        private const int EUDEMONGG_PAKC_ID = (int)ItemPosition.EudemonGGPack - 50;
        private const int EUDEMON_PAKC_ID = (int)ItemPosition.EudemonPack - 50;
        private const int MAX_PACK_TYPE = (ItemPosition.PackEnd - ItemPosition.PackBegin);

        public const int MAX_USERITEMSIZE = 40;
        public const int MAX_GHOSTGEMSIZE = 40;
        public const int MAX_EUDEMONEGGSIZE = 6;
        public const int MAX_EUDEMONSIZE = 12;

        private static int[] MAX_PACK_SIZES = new int[]
        {
            MAX_USERITEMSIZE,
            MAX_GHOSTGEMSIZE,
            MAX_EUDEMONEGGSIZE,
            MAX_EUDEMONSIZE,
        };

        private readonly ConcurrentDictionary<uint, ItemBase>[] packages = new ConcurrentDictionary<uint, ItemBase>[MAX_PACK_TYPE];
        private readonly ConcurrentDictionary<ItemPosition, ItemBase> equipments = new();

        private readonly TimeOut checkItemsTimer = new();
        private readonly Character user;

        public uint LastAddItemIdentity { get; set; }

        public UserPackage(Character user)
        {
            this.user = user;
            for (int i = 0; i < MAX_PACK_TYPE; i++)
            {
                packages[i] = new ConcurrentDictionary<uint, ItemBase>();
            }
        }

        public ItemBase this[ItemPosition position] => equipments.TryGetValue(position, out var item) ? item : null;

        public ItemBase this[string name] => packages[USERITEM_PAKC_ID].Values.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public async Task InitializeAsync()
        {
            var delItems = new List<DbItem>();
            var items = ItemRepository.Get(user.Identity);
            foreach (var dbItem in items.OrderBy(x => x.OwnerId).ThenBy(x => x.Position).ThenBy(x => x.Id))
            {
                Item item = new(user);
                if (!await item.CreateAsync(dbItem))
                {
                    logger.Error($"Failed to load item {dbItem.Id} to user {user.Identity}");
                    delItems.Add(dbItem);
                    continue;
                }

                if (item.HasExpired())
                {
                    await item.ExpireAsync();
                    //await item.DeleteAsync();// delete is being set twice...
                    continue;
                }

                if (item.IsSuspicious() && item.Position >= ItemPosition.EquipmentBegin && item.Position <= ItemPosition.EquipmentEnd)
                {
                    item.Position = ItemPosition.Inventory;
                }

                if (item.Position >= ItemPosition.EquipmentBegin && item.Position <= ItemPosition.EquipmentEnd)
                {
                    if (!equipments.TryAdd(item.Position, item))
                    {
                        packages[USERITEM_PAKC_ID].TryAdd(item.Identity, item);
                    }
                }
                else if (item.Position == ItemPosition.Inventory)
                {
                    if (!packages[USERITEM_PAKC_ID].TryAdd(item.Identity, item))
                    {
                        logger.Warning("Failed to insert inventory item {0}: duplicate???", item.Identity);
                    }
                }
                else if (item.Position == ItemPosition.Floor)
                {
                    await item.DeleteAsync();
                }
                else if (item.Position == ItemPosition.Storage || item.Position == ItemPosition.Trunk)
                {
                    BaseNpc npc = RoleManager.GetRole(item.OwnerIdentity) as BaseNpc;
                    if (npc == null)
                    {
                        logger.Error($"Unexistent warehouse {item.OwnerIdentity} for item {item.Identity}");
                        continue;
                    }

                    await user.Packages.AddItemAsync(npc.Identity, item, StorageType.Storage, false);
                }
                else if (item.Position == ItemPosition.Chest)
                {
                    await user.Packages.AddItemAsync(item.OwnerIdentity, item, StorageType.Chest, false);
                }
                else if (item.Position == ItemPosition.Auction || item.Position == ItemPosition.AuctionEudStorage)
                {
                    // skip
                }
                else
                {
                    logger.Warning("Item {0} on '{1}' cannot be loaded (unhandled)", item.Identity, item.Position);
                }
            }

            if (delItems.Count > 0)
            {
                await ServerDbContext.DeleteRangeAsync(delItems);
            }

            checkItemsTimer.Startup(60);
        }

        public ItemBase GetItem(uint itemId)
        {
            for (int i = 0; i < MAX_PACK_TYPE; i++)
            {
                if (packages[i].TryGetValue(itemId, out var item))
                {
                    return item;
                }
            }

            return null;
        }

        public ItemBase GetItemByType(uint type)
        {
            for (int i = 0; i < MAX_PACK_TYPE; i++)
            {
                var item = packages[i].Values.FirstOrDefault(x => x.Type == type);
                if (item != null)
                {
                    return item;
                }
            }

            return null;
        }

        public ItemBase GetEquipment(ItemPosition position)
        {
            return this[position];
        }

        public ItemBase GetEquipmentById(uint idItem)
        {
            return equipments.Values.FirstOrDefault(x => x.Identity == idItem);
        }

        public ItemBase FindItemByIdentity(uint itemId)
        {
            return GetEquipmentById(itemId) ?? GetItem(itemId);
        }

        public ItemBase FindAppointedPos(uint pos)
        {
            return packages[EUDEMON_PAKC_ID].Values.Where(x => x.EudemonOfficialPos == pos).FirstOrDefault();
        }

        public ItemBase FindAppointedPosIdx(byte idx)
        {
            return packages[EUDEMON_PAKC_ID].Values.Where(x => x.EudemonOfficialIndex == idx).FirstOrDefault();
        }

        public List<ItemBase> FindMayAppointPosIdx(byte idx)
        {
            return packages[EUDEMON_PAKC_ID].Values.Where(x => x.GetOfficialPos(idx) != 0).ToList();
        }
        public List<ItemBase> QueryAppointedPos()
        {
            return packages[EUDEMON_PAKC_ID].Values.Where(x => x.EudemonOfficialIndex != 0).ToList();
        }

        public ItemPosition GetItemPosition(ItemBase item)
        {
            if (item == null)
            {
                logger.Error($"UserPackage()->GetItemPosition(): Nulled item!");
                return ItemPosition.None;
            }

            return GetItemPositionByType(item.Type);
        }

        public ItemPosition GetItemPositionByType(uint typeId)
        {
            if (typeId == 0)
            {
                logger.Error($"UserPackage()->GetItemPositionByType(): item type 0!");
                return ItemPosition.None;
            }

            /*if (IsGhostGem(typeId))
            {
                return ItemPosition.GhostGemPack;
            }
            else*/ if (IsEudemon(typeId))
            {
                return ItemPosition.EudemonPack;
            }
            else if (IsEudemonEgg(typeId))
            {
                return ItemPosition.EudemonGGPack;
            }

            return ItemPosition.Inventory;
        }

        public bool IsPackSpare(int size, uint type = 0)
        {
            if (type != 0)
            {
                var itemType = ItemManager.GetItemtype(type);
                int free = 0;
                if (itemType != null)
                {
                    foreach (var item in packages[USERITEM_PAKC_ID].Values.Where(x => x.Type == type && x.AccumulateNum < x.MaxAccumulateNum))
                    {
                        free += (int)(item.MaxAccumulateNum - item.AccumulateNum);
                        if (free >= size)
                        {
                            return true;
                        }
                    }

                    int add = (int)((MAX_USERITEMSIZE - packages[USERITEM_PAKC_ID].Count) * Math.Max(1u, itemType.AmountLimit));
                    return free + add >= size;
                }
            }

            return packages[USERITEM_PAKC_ID].Count + size <= MAX_USERITEMSIZE;
        }

        public bool IsGGPackSpare(int amount)
        {
            return packages[EUDEMONGG_PAKC_ID].Count + amount <= MAX_EUDEMONEGGSIZE;
        }

        public bool IsPackFull(ItemPosition position = ItemPosition.Inventory)
        {
            int packType = position - ItemPosition.PackBegin;
            if (packType >= 0 && packType < MAX_PACK_TYPE)
            {
                return packages[packType].Count >= MAX_PACK_SIZES[packType];
            }

            logger.Error($"UserPackage()->IsPackFull(): Unknown packtype {packType}");
            return true;
        }

        #region Usage

        public async Task<bool> UseItemAsync(uint idItem, ItemPosition position)
        {
            if (!user.IsAlive)
            {
                return true;
            }

            if (!TryItem(idItem, position))
            {
                return false;
            }

            ItemBase item = GetItem(idItem);
            if (item == null)
            {
                return false;
            }

            if (item.IsSuspicious())
            {
                return false;
            }

            if (item.Type == ItemBase.TYPE_EXP_BALL)
            {
                if (user.Map != null && user.Map.IsNoExpMap())
                {
                    return false;
                }

                if (!user.CanUseExpBall())
                {
                    return false;
                }

                await user.IncrementExpBallAsync();
                await user.AwardExperienceAsync(user.CalculateExpBall());
                await SpendItemAsync(item);
                return true;
            }

            if (item.Type == ItemBase.TYPE_EXP_POTION)
            {
                if (user.ExperienceMultiplier > 2 && user.RemainingExperienceSeconds > 0)
                {
                    await user.SendAsync(StrExpPotionInUse);
                    return true;
                }

                await user.SetExperienceMultiplierAsync(3600);
                await SpendItemAsync(item);
                return true;
            }

            if (item.IsEquipEnable())
            {
                return await EquipItemAsync(item, position);
            }

            if (item.IsMedicine())
            {
                int nAddLife = item.Itemtype.Life;
                int nAddMana = item.Itemtype.Mana;
                int nTimes = item.Itemtype.AtkSpeed;
                if (!await SpendItemAsync(item))
                {
                    return false;
                }

                await user.AddSlowRealLifeAsync(nAddLife, nAddMana, nTimes);
                return true;
            }

            if (item.Itemtype.IdAction > 0)
            {
                user.InteractingNpc = 0;
                user.InteractingItem = item.Identity;
                user.ClearTaskId();

                bool result = await GameAction.ExecuteActionAsync(item.Itemtype.IdAction, user, null, item, string.Empty) && item.Itemtype.IdAction > 0;

                // todo validate auto
                if (item.GetItemSubType() == 60)
                {
                    await SpendItemAsync(item);
                }

                return result;
            }

            return false;
        }

        public async Task<bool> UseItemToAsync(uint targetId, uint itemId)
        {
            if (targetId == 0)
            {
                return false;
            }

            //if (!this->TryItem(idItem, ITEMPOSITION_GHOSTGEM_PACK))
            //{
            //    return false;
            //}

            var item = GetItem(itemId);
            if (item == null)
            {
                return false;
            }
            // ÅÐ¶ÏÊÇ·ñÊÇ¿ÉÒÔ¶ÔÆäËû½ÇÉ«Ê¹ÓÃµÄÄ§»ê±¦Ê¯
            /*
                if (!(pItem->IsGhostGem()		// Ä§»ê±¦Ê¯
                        && ((pItem->GetInt(ITEMDATA_TYPE)%100000/1000*1000 == ITEMTYPE_GHOSTGEM_ACTIVE_ATK) ||	// ×´Ì¬¹¥»÷Àà
                            (pItem->GetInt(ITEMDATA_TYPE)%100000/1000*1000 == ITEMTYPE_GHOSTGEM_TRACE)))			// ×·É±Àà
                        )
                {
            #ifdef _DEBUG
                ::LogSave("Can not use item %s [%u] to target.", pItem->GetStr(ITEMDATA_NAME), idItem);
            #endif
                    return false;
                }
            */
            Role target = RoleManager.GetRole(targetId);
            if (target == null)
            {
                return false;
            }

            if (!ChkUseItem(item as Item, target))
            {
                return false;
            }

            if (item.Itemtype.IdAction > 0 || item.IsGhostGem())
            {
                user.InteractingNpc = 0;
                user.InteractingItem = item.Identity;
                user.ClearTaskId();

                bool result = await GameAction.ExecuteActionAsync(item.Itemtype.IdAction, user, target, item, "");  //??? mast last code, may be user chgmap to another mapgroup
                return result;
            }
            return false;
        }

        public bool ChkUseItem(Item pItem, Role pTarget)
        {
            if (pItem == null || pTarget == null)
            {
                return false;
            }

            int usTarget = pItem.Itemtype.Target;
            if (usTarget == 0)
            {
                if (pTarget.Identity == user.Identity)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if ((usTarget & (int)TargetMask.TargetForbidden) != 0)
            {
                return false;
            }

            Character pUser = pTarget as Character;
            if (pUser != null)
            {
                if ((usTarget & (int)TargetMask.TargetSelf) != 0 && pUser.Identity != user.Identity
                    || (usTarget & (int)TargetMask.TargetOthers) != 0 && pUser.Identity == user.Identity)
                {
                    return false;
                }

                if (((usTarget & (int)TargetMask.TargetBody) != 0 && pUser.IsAlive)
                    || ((usTarget & (int)TargetMask.TargetForbidden) == 0 && !pUser.IsAlive))
                {
                    return false;
                }
            }
            else
            {
                //Monster pMonster = pTarget as Monster;
                //if (pMonster == null)
                //{
                //    return false;
                //}

                //if (!((usTarget & (int)TargetMask.TargetMonster) != 0 && pMonster.IsMonster())
                //    && !((usTarget & (int)TargetMask.TargetMonster) != 0 && pMonster.IsEudemon()))
                //{
                //    return false;
                //}

                //Character pOwner = RoleManager.GetUser(pMonster.OwnerIdentity);
                //if (pOwner != null)
                //{
                //    if ((usTarget & (int)TargetMask.TargetSelf) != 0 && pOwner.Identity != user.Identity
                //        || (usTarget & (int)TargetMask.TargetOthers) != 0 && pOwner.Identity == user.Identity)
                //    {
                //        return false;
                //    }
                //}

                //if (((usTarget & (int)TargetMask.TargetBody) != 0 && pMonster.IsAlive)
                //    || ((usTarget & (int)TargetMask.TargetBody) == 0 && !pMonster.IsAlive))
                //{
                //    return false;
                //}
            }
            return true;
        }

        #endregion

        #region Equip/UnEquip

        public bool TryItem(uint idItem, ItemPosition position)
        {
            ItemBase item = FindItemByIdentity(idItem);
            if (item == null)
            {
                return false;
            }

            if (item.IsSuspicious())
            {
                return false;
            }

            if (item.RequiredLevel > user.Level)
            {
                return false;
            }

            if (item.RequiredGender > 0 && item.RequiredGender != user.Gender)
            {
                return false;
            }

            if (item.Amount == 0 && !item.IsExpend())
            {
                return false;
            }

            if (user.Metempsychosis > 0 && user.Level >= 70)
            {
                return true;
            }

            if (item.RequiredProfession > 0)
            {
                int nRequireProfSort = item.RequiredProfession / 10;
                int nRequireProfLevel = item.RequiredProfession % 10;
                int nProfSort = user.ProfessionSort;
                int nProfLevel = user.ProfessionLevel;
                if (nRequireProfSort != nProfSort)
                {
                    return false;
                }


                if (nProfLevel < nRequireProfLevel)
                {
                    return false;
                }
            }

            if (item.RequiredForce > user.Strength
                || item.RequiredAgility > user.Speed
                || item.RequiredVitality > user.Vitality
                || item.RequiredSpirit > user.Spirit)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> EquipItemAsync(ItemBase item, ItemPosition position)
        {
            if (item == null)
            {
                return false;
            }

            //user.BattleSystem.ResetBattle();
            //await user.MagicData.AbortMagicAsync(false);

            if (position == ItemPosition.Inventory)
            {
                position = item.GetPosition();
            }

            switch (position)
            {
                case ItemPosition.Weapon:
                    {
                        if (!item.IsHoldEnable())
                        {
                            return false;
                        }

                        ItemPosition rightHandPosition = position;
                        if (item.IsWeaponOneHand())
                        {
                            await UnEquipAsync(rightHandPosition);
                        }

                        break;
                    }

                default:
                    await UnEquipAsync(position);
                    break;
            }

            if (!await RemoveFromInventoryAsync(item, RemovalType.UnEquip))
            {
                return false;
            }

            if (!equipments.TryAdd(position, item))
            {
                await AddItemAsync(item);
                return false;
            }

            item.Position = position;
            await user.SendAsync(new MsgItem(item.Identity, MsgItem.ItemActionType.EquipmentWear, (uint)position));
            await item.SaveAsync();
            switch (position)
            {
                case ItemPosition.Helmet:
                case ItemPosition.Weapon:
                case ItemPosition.Armor:
                case ItemPosition.Sprite:
                    await user.Screen.BroadcastRoomMsgAsync(new MsgPlayer(user), false);
                    break;
            }

            //if (user.Team != null)
            //{
            //    await user.Team.SyncFamilyBattlePowerAsync();
            //}

            //if (user.ApprenticeCount > 0)
            //{
            //    await user.SynchroApprenticesSharedBattlePowerAsync();
            //}
            return true;
        }

        public async Task<bool> UnEquipAsync(ItemPosition position, RemovalType mode = RemovalType.RemoveOnly)
        {
            ItemBase item = this[position];
            if (item == null)
            {
                return false;
            }

            //user.BattleSystem.ResetBattle();
            //await user.MagicData.AbortMagicAsync(false);
            if (!IsPackSpare(1) && mode != RemovalType.Delete)
            {
                return false;
            }

            equipments.TryRemove(position, out _);
            item.Position = ItemPosition.Inventory;
            if (mode != RemovalType.Delete && mode != RemovalType.RemoveAndDisappear)
            {
                await user.SendAsync(new MsgItem(item.Identity, MsgItem.ItemActionType.EquipmentRemove, (uint)position));
                await user.SendAsync(new MsgItemInfo(item));
            }
            else
            {
                await user.SendAsync(new MsgItem(item.Identity, MsgItem.ItemActionType.EquipmentRemove, (uint)position));
                await user.SendAsync(new MsgItemInfo(item));
                await user.SendAsync(new MsgItem(item.Identity, MsgItem.ItemActionType.InventoryRemove));
            }

            if (mode == RemovalType.Delete)
            {
                await item.DeleteAsync();
            }
            else if (mode != RemovalType.RemoveAndDisappear)
            {
                await AddItemAsync(item);
                await item.SaveAsync();
            }
            else
            {
                await item.SaveAsync();
            }

            switch (position)
            {
                case ItemPosition.Weapon:
                case ItemPosition.Armor:
                case ItemPosition.Sprite:
                    {
                        await user.Screen.BroadcastRoomMsgAsync(new MsgPlayer(user), false);
                        break;
                    }
            }

            //if (user.Team != null)
            //{
            //    await user.Team.SyncFamilyBattlePowerAsync();
            //}

            //if (user.ApprenticeCount > 0)
            //{
            //    await user.SynchroApprenticesSharedBattlePowerAsync();
            //}
            return true;
        }

        #endregion

        #region Award/Add

        public async Task<bool> AwardItemAsync(uint type, int amount, bool monopoly = false, bool autoActivate = false, bool enableUnident = false)
        {
            DbItemtype itemtype = ItemManager.GetItemtype(type);
            if (itemtype == null)
            {
                return false;
            }

            if (!IsPackSpare(amount, type))
            {
                return false;
            }

            byte ident = 0;
            if (enableUnident && await NextAsync(100) < 70)
            {
                ident = ItemBase.ITEM_STATUS_NOT_IDENT;
            }

            int tempAmount = amount;
            List<DbItem> items = new();
            while (tempAmount > 0)
            {
                if (IsEquipment(type) || IsGarment(type))
                {
                    DbItem item = Item.CreateEntity(type, monopoly);
                    item.Amount = itemtype.Amount;
                    item.AmountLimit = itemtype.AmountLimit;
                    item.Ident = ident;
                    items.Add(item);
                    tempAmount--;
                }
                else
                {
                    int createAmount = (int)Math.Min(amount, itemtype.AmountLimit);
                    DbItem item = Item.CreateEntity(type, monopoly);
                    item.Amount = (ushort)createAmount;
                    item.Ident = ident;
                    items.Add(item);
                    tempAmount -= Math.Max(1, createAmount);
                }
            }

            if (await ServerDbContext.CreateRangeAsync(items))
            {
                for (int i = 0; i < items.Count; i++)
                {
                    DbItem dbItem = items[i];
                    ItemBase item = new Item(user);
                    if (!await item.CreateAsync(dbItem))
                    {
                        awardItemErrorLogger.Information("Could not create item {0}/{1} of type {2} for user {3} {4}", i, amount, type, user.Identity, user.Name);
                        continue;
                    }
                    await AddItemAsync(item);
                }
            }
            return false;
        }

        public async Task<bool> AwardItemAsync(uint type, ItemPosition pos = ItemPosition.Inventory, bool monopoly = false, bool autoActivate = false)
        {
            DbItemtype itemtype = ItemManager.GetItemtype(type);
            if (itemtype == null)
            {
                return false;
            }

            ItemBase item = new Item(user);
            if (!await item.CreateAsync(itemtype, pos, monopoly))
            {
                return false;
            }

            if (item.IsCountable())
            {
                item.AccumulateNum = (ushort)Math.Max(1, (int)item.AccumulateNum);
            }

            if (item.IsActivable() && autoActivate)
            {
                await item.ActivateAsync();
            }

            return await AddItemAsync(item);
        }

        public async Task<bool> AddItemAsync(ItemBase item, bool combineEnable = true, ItemPosition position = ItemPosition.Inventory)
        {
            position = GetItemPosition(item);
            if (position > ItemPosition.Inventory && IsPackFull(position))
            {
                return false;
            }

            if (position == ItemPosition.Inventory && !IsPackSpare((int)item.AccumulateNum, item.Type))
            {
                return false;
            }

            if (item.PlayerIdentity != user.Identity)
            {
                item.ChangeOwner(user);
            }

            item.Position = position;
            if (item.IsCountable() && combineEnable)
            {
                ItemBase combine = FindCombineItem(item);
                if (combine != null)
                {
                    do
                    {
                        await CombineItemAsync(item, combine);
                    }
                    while (item.AccumulateNum > 1 && item.AccumulateNum > item.MaxAccumulateNum && (combine = FindCombineItem(item)) != null);

                    if (item.AccumulateNum == 0)
                    {
                        await RemoveFromInventoryAsync(item, RemovalType.Delete);
                        return true;
                    }
                }
            }

            if (item.AccumulateNum > item.MaxAccumulateNum)
            {
                var amount = item.AccumulateNum - item.MaxAccumulateNum;
                item.AccumulateNum = item.MaxAccumulateNum;

                DbItem newDbItem = Item.CreateEntity(item.Type, item.IsBound);
                newDbItem.Amount = (ushort)amount;
                if (!item.IsPileEnable())
                {
                    newDbItem.Data = item.Data;
                    if (item.DeleteTime > 0)
                    {
                        newDbItem.DeleteTime = item.DeleteTime;
                    }

                    if (item.AvailableTime > 0)
                    {
                        newDbItem.AvailableTime = (uint)item.AvailableTime;
                    }
                }

                ItemBase newItem = new Item(user);
                if (await newItem.CreateAsync(newDbItem))
                {
                    await AddItemAsync(newItem);
                }
            }

            packages[position - ItemPosition.PackBegin].TryAdd(item.Identity, item);
            await item.SaveAsync();
            await user.SendAsync(new MsgItemInfo(item));
            return true;
        }

        #endregion

        #region Spend/Remove

        public async Task<bool> SpendItemAsync(uint type, int amount = 1, bool denyBound = false)
        {
            return await MultiSpendItemAsync(type, type, amount, denyBound);
        }

        public async Task<bool> SpendItemAsync(ItemBase item, bool deleteAll = false)
        {
            if (item == null)
            {
                return false;
            }

            if (!deleteAll && item.IsPileEnable() && item.AccumulateNum > 1)
            {
                item.AccumulateNum -= 1;
                await user.SendAsync(new MsgItemInfo(item, MsgItemInfo.ItemMode.Update));
                await item.SaveAsync();
                return true;
            }

            return item.Position == ItemPosition.Inventory && await RemoveFromInventoryAsync(item.Identity, RemovalType.Delete);
        }

        public async Task<bool> RemoveFromInventoryAsync(uint idItem, RemovalType mode = RemovalType.RemoveOnly)
        {
            foreach(var pack in packages)
            {
                if (pack.TryGetValue(idItem, out var item))
                {
                    return await RemoveFromInventoryAsync(item, mode);
                }
            }

            return false;
        }

        public async Task<bool> RemoveFromInventoryAsync(ItemBase item, RemovalType mode = RemovalType.RemoveOnly)
        {
            var packType = GetItemPosition(item);
            if (!packages[packType - ItemPosition.PackBegin].TryRemove(item.Identity, out _) && mode != RemovalType.Delete)
            {
                return false;
            }

            switch (mode)
            {
                case RemovalType.DropItem:
                    item.Position = ItemBase.ItemPosition.Floor;
                    break;

                case RemovalType.Delete:
                    await item.DeleteAsync();
                    break;
            }

            if (mode != RemovalType.UnEquip)
            {
                await user.SendAsync(new MsgItem(item.Identity, MsgItem.ItemActionType.InventoryRemove));
            }

            return true;
        }

        #endregion

        #region Combine/Split

        public ItemBase FindCombineItem(ItemBase item)
        {
            foreach (var pack in packages)
            {
                var target = pack.Values
                        .FirstOrDefault(x => x.Type == item.Type
                        && x.AccumulateNum < x.MaxAccumulateNum
                        && x.IsBound == item.IsBound);

                if (target != null)
                {
                    return target;
                }
            }

            return null;
        }

        public async Task<bool> CombineItemAsync(uint idItem, uint idCombine)
        {
            return packages[USERITEM_PAKC_ID].TryGetValue(idItem, out var item)
                   && packages[USERITEM_PAKC_ID].TryGetValue(idCombine, out var combine)
                   && await CombineItemAsync(item, combine);
        }

        public async Task<bool> CombineItemAsync(ItemBase item, ItemBase combine)
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
                await user.SendAsync(new MsgItemInfo(combine, MsgItemInfo.ItemMode.Update));
                await user.SendAsync(new MsgItemInfo(item, MsgItemInfo.ItemMode.Update));
                await item.SaveAsync();
                await combine.SaveAsync();
            }
            else
            {
                item.AccumulateNum = 0;
                await RemoveFromInventoryAsync(item, RemovalType.Delete);
                combine.AccumulateNum = (ushort)newNum;
                await user.SendAsync(new MsgItemInfo(combine, MsgItemInfo.ItemMode.Update));
                await combine.SaveAsync();
            }
            return true;
        }

        public async Task<bool> SplitItemAsync(uint idItem, int amount)
        {
            return packages[USERITEM_PAKC_ID].TryGetValue(idItem, out var item)
                   && await SplitItemAsync(item, amount);
        }

        public async Task<bool> SplitItemAsync(ItemBase item, int amount)
        {
            if (item == null || !item.IsCountable() || !item.IsPileEnable())
            {
                return false;
            }

            if (amount <= 0)
            {
                return false;
            }

            if (amount >= item.MaxAccumulateNum)
            {
                return false;
            }

            if (!IsPackSpare(amount, item.Type))
            {
                return false;
            }

            DbItem split = Item.CreateEntity(item.Type, item.IsBound);
            split.PlayerId = user.Identity;

            item.AccumulateNum -= (ushort)amount;
            await user.SendAsync(new MsgItemInfo(item, MsgItemInfo.ItemMode.Update));
            await item.SaveAsync();

            split.Amount = (ushort)amount;

            ItemBase splitItem = new Item(user);
            if (!await splitItem.CreateAsync(split))
            {
                return false;
            }

            return await AddItemAsync(splitItem, false);
        }

        #endregion

        #region MultiSpend/MultiGet

        public async Task<bool> MultiSpendItemAsync(uint idFirst, uint idLast, int nNum, bool refuseMonopoly = false, bool saveTime = false)
        {
            int temp = nNum;
            List<ItemBase> items = new();
            if (MultiGetItem(idFirst, idLast, nNum, ref items, refuseMonopoly, saveTime) < nNum)
            {
                return false;
            }

            foreach (var item in items)
            {
                if (refuseMonopoly && item.IsBound && (item.Itemtype.Monopoly & 1) == 0)
                {
                    continue;
                }

                if (item.IsPileEnable() && item.AccumulateNum > nNum)
                {
                    item.AccumulateNum -= (ushort)nNum;
                    await item.SaveAsync();
                    await user.SendAsync(new MsgItemInfo(item, MsgItemInfo.ItemMode.Update));
                    return true;
                }

                nNum -= (int)item.AccumulateNum;
                await RemoveFromInventoryAsync(item, RemovalType.Delete);
            }

            if (nNum > 0)
            {
                logger.Error("Something went wrong when MultiSpendItem({0}, {1}, {2}) {3} left???", idFirst, idLast, temp, nNum);
            }

            return nNum == 0;
        }

        public int MultiGetItem(uint idFirst, uint idLast, int num, ref List<ItemBase> items, bool refuseMonopoly = false, bool saveTime = false)
        {
            int amount = 0;
            foreach (var item in packages[USERITEM_PAKC_ID]
                .Values
                .Where(x => x.Type >= idFirst && x.Type <= idLast)
                .OrderBy(x => x.Type))
            {
                if (refuseMonopoly && item.IsBound && (item.Itemtype.Monopoly & 1) == 0)
                {
                    continue;
                }

                if (saveTime && item.IsActivable())
                {
                    continue;
                }

                items.Add(item);
                amount += (int)item.AccumulateNum;

                if (num > 0 && amount >= num)
                {
                    return amount;
                }
            }

            return amount;
        }

        #endregion

        #region RandomDrop

        public async Task<int> RandDropItemAsync(int mapType, int nChance)
        {
            if (user == null)
            {
                return 0;
            }

            int nDropNum = 0;
            foreach (var item in packages[USERITEM_PAKC_ID].Values)
            {
                if (await ChanceCalcAsync(nChance))
                {
                    if (item.IsNeverDropWhenDead() || item.IsSuspicious())
                    {
                        continue;
                    }

                    switch (mapType)
                    {
                        case 0: // NONE
                            break;
                        case 1: // PK
                        case 2: // SYN
                            if (!item.IsArmor())
                            {
                                continue;
                            }

                            break;
                        case 3: // PRISON
                            break;
                    }

                    var pos = new Point(user.X, user.Y);
                    if (user.Map.FindDropItemCell(5, ref pos))
                    {
                        if (!await RemoveFromInventoryAsync(item.Identity, RemovalType.DropItem))
                        {
                            continue;
                        }

                        //var pMapItem = new MapItem((uint)IdentityManager.MapItem.GetNextIdentity);
                        //if (pMapItem.Create(user.Map, pos, item, user.Identity))
                        //{
                        //    await pMapItem.EnterMapAsync();
                        //    await item.SaveAsync();
                        //}
                        //else
                        //{
                        //    IdentityManager.MapItem.ReturnIdentity(pMapItem.Identity);
                        //}

                        nDropNum++;
                    }
                }
            }

            return nDropNum;
        }

        public async Task<int> RandDropItemAsync(int nDropNum)
        {
            if (user == null)
            {
                return 0;
            }

            var temp = new List<ItemBase>();
            foreach (var item in packages[USERITEM_PAKC_ID].Values)
            {
                if (item.IsNeverDropWhenDead() || item.IsSuspicious())
                {
                    continue;
                }

                temp.Add(item);
            }

            int nTotalItemCount = temp.Count;
            if (nTotalItemCount == 0)
            {
                return 0;
            }

            int nRealDropNum = 0;
            for (int i = 0; i < Math.Min(nDropNum, nTotalItemCount); i++)
            {
                int nIdx = await NextAsync(nTotalItemCount);
                ItemBase item;
                try
                {
                    item = temp[nIdx];
                }
                catch
                {
                    continue;
                }

                var pos = new Point(user.X, user.Y);
                if (user.Map.FindDropItemCell(5, ref pos))
                {
                    if (!await RemoveFromInventoryAsync(item.Identity, RemovalType.DropItem))
                    {
                        continue;
                    }

                    //var pMapItem = new MapItem((uint)IdentityManager.MapItem.GetNextIdentity);
                    //if (pMapItem.Create(user.Map, pos, item, user.Identity))
                    //{
                    //    await pMapItem.EnterMapAsync();
                    //    await item.SaveAsync();
                    //}
                    //else
                    //{
                    //    IdentityManager.MapItem.ReturnIdentity(pMapItem.Identity);
                    //}

                    nRealDropNum++;
                }
            }

            return nRealDropNum;
        }

        #endregion

        public async Task ClearInventoryAsync()
        {
            foreach (var item in packages[USERITEM_PAKC_ID].Values)
            {
                await RemoveFromInventoryAsync(item, RemovalType.RemoveAndDisappear);
                await item.DeleteAsync();
            }
        }

        /// <summary>
        ///     Sent only on login!!!
        /// </summary>
        public async Task SendAsync()
        {
            foreach (var item in equipments.Values)
            {
                await user.SendAsync(new MsgItemInfo(item));
                await item.TryUnlockAsync();
            }

            for (int i = 0; i < MAX_PACK_TYPE; i++)
            {
                foreach (var item in packages[i].Values)
                {
                    await user.SendAsync(new MsgItemInfo(item));
                    await item.TryUnlockAsync();
                }
            }
        }

        public async Task OnTimerAsync()
        {
            if (!checkItemsTimer.ToNextTime())
            {
                return;
            }

            for (ItemPosition p = ItemPosition.EquipmentBegin; p <= ItemPosition.EquipmentEnd; p++)
            {
                ItemBase testItem = this[p];
                if (testItem == null)
                {
                    continue;
                }

                if (testItem.HasExpired())
                {
                    await testItem.ExpireAsync();
                    continue;
                }

                if (testItem.IsUnlocking())
                {
                    await testItem.TryUnlockAsync();
                }
            }

            for (int i = 0; i < MAX_PACK_TYPE; i++)
            {
                foreach (var testItem in packages[i].Values)
                {
                    if (testItem.HasExpired())
                    {
                        await testItem.ExpireAsync();
                        continue;
                    }

                    if (testItem.IsUnlocking())
                    {
                        await testItem.TryUnlockAsync();
                    }
                }
            }
        }

        public enum RemovalType
        {
            /// <summary>
            ///     Item will be removed and disappear, but wont be deleted.
            /// </summary>
            RemoveAndDisappear,

            /// <summary>
            ///     Item will be internally removed only. No client interaction and also wont be deleted.
            /// </summary>
            RemoveOnly,

            /// <summary>
            ///     Item will be removed and deleted.
            /// </summary>
            Delete,

            /// <summary>
            ///     Item will be set to floor and will be updated. No delete.
            /// </summary>
            DropItem,
            UnEquip
        }
    }
}
