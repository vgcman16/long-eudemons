using Long.Database.Entities;
using Long.Kernel.Database;
using Long.Kernel.Managers;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.User;
using System.Text;

namespace Long.Kernel.States.Items
{
    public sealed class Item : ItemBase
    {
        public Item()
            : base(null)
        {

        }

        public Item(Character user)
            : base(user)
        {

        }

        #region Creation

        public override async Task<bool> CreateAsync(DbItemtype type, ItemPosition position = ItemPosition.Inventory, bool monopoly = false)
        {
            if (type == null)
            {
                return false;
            }

            item = new DbItem
            {
                PlayerId = user.Identity,
                Type = type.Type,
                Position = (byte)position,
                Amount = type.Amount,
                AmountLimit = type.AmountLimit,
                Data = type.Type == TYPE_EXP_BALL ? 1000000u : 0u,
                Magic1 = (byte)type.Magic1,
                Magic2 = type.Magic2,
                Magic3 = type.Magic3,
                Monopoly = (byte)(monopoly ? 3 : 0)
            };
            item.ChkSum = CalculateCheckSum(item);

            itemType = type;
            itemAddition = ItemManager.GetItemAddition(Type, Plus);

            return await SaveAsync() && (user.LastAddItemIdentity = Identity) != 0;
        }

        public override async Task<bool> CreateAsync(DbItem item)
        {
            if (item == null)
            {
                return false;
            }

            this.item = item;
            itemType = ItemManager.GetItemtype(item.Type);
            if (itemType == null)
            {
                return false;
            }

            itemAddition = ItemManager.GetItemAddition(item.Type, item.Magic3);
            uint chksum = CalculateCheckSum(item);

            if (item.ChkSum == 0) // initialization for old or newly created items
            {
                await SaveAsync();
            }

            if (this.item.Id == 0)
            {
                item.ChkSum = chksum;
                await SaveAsync();
                user.LastAddItemIdentity = Identity;
            }
            else if (item.ChkSum != chksum)
            {
                deleteChkSumItemLogger.Warning("Invalid chksum [expected: {0}, got: {1}] for item {2}, user {3} {4}", chksum, item.ChkSum, item.Id, user?.Identity ?? 0, user?.Name ?? string.Empty);
                await DeleteAsync();
                return false;
            }

            return true;
        }

        #endregion

        #region Change Data

        public override async Task<bool> ChangeTypeAsync(uint newType)
        {
            DbItemtype itemtype = ItemManager.GetItemtype(newType);
            if (itemtype == null)
            {
                logger.Error($"ChangeType() Invalid itemtype id {newType}");
                return false;
            }

            if (eudemon != null)
            {
                grade = ItemManager.GetEudemonGrade(newType);
                itemType = itemtype;
                eudemon.ItemType = itemtype.Type;
                return true;
            }

            item.Type = itemtype.Type;
            itemType = itemtype;

            itemAddition = ItemManager.GetItemAddition(newType, item.Magic3);
            await user.SendAsync(new MsgItemInfo(this, MsgItemInfo.ItemMode.Update));
            await SaveAsync();
            return true;
        }

        public override bool DecAddition()
        {
            if (Plus == 0)
            {
                return false;
            }

            DbItemAddition add = null;
            add = ItemManager.GetItemAddition(Type, (byte)(Plus - 1));
            if (add == null)
            {
                return false;
            }

            Plus--;
            itemAddition = add;
            return true;
        }

        public override bool ChangeAddition(int level = -1)
        {
            if (level < 0)
            {
                level = (byte)(Plus + 1);
            }

            DbItemAddition add = null;
            if (level > 0)
            {
                uint type = Type;
                add = ItemManager.GetItemAddition(type, (byte)level);
                if (add == null)
                {
                    return false;
                }
            }

            Plus = (byte)level;
            itemAddition = add;
            return true;
        }

        public override async Task<bool> ConvertToCrystalAsync()
        {
            if (!IsEquipment())
            {
                return false;
            }

            int data = Plus * 100;
            switch (GetQuality())
            {
                case 0: break;
                case 1: data += 10; break;
                case 2: data += 40; break;
                case 3: data += 200; break;
                case 4: data += 1800; break;
                case 5: data += 2400; break;
                case 6: data += 3000; break;
                case 7: data += 3600; break;
            }

            if (SocketOne != SocketGem.NoSocket && SocketTwo == SocketGem.NoSocket)
            {
                data += 800;
            }
            else if (SocketTwo != SocketGem.NoSocket)
            {
                data += 2000;
            }

            deleteItemLogger.Information($"[Owner: {OwnerIdentity}][{Identity} - {Name}] has been decomposed to crystal: {ToJson()}");

            uint savedId = Identity;
            this.item = new DbItem
            {
                Id = savedId,
                PlayerId = user.Identity,
                Type = 500002,
                Position = (byte)ItemBase.ItemPosition.Inventory,
                Data = (uint)data,
            };

            item.ChkSum = CalculateCheckSum(item);
            itemType = ItemManager.GetItemtype(500002);
            itemAddition = ItemManager.GetItemAddition(Type, Plus);

            await user.SendAsync(new MsgItemInfo(this, MsgItemInfo.ItemMode.Update));
            return await SaveAsync();
        }

        #endregion

        #region Attributes

        public override uint Identity => item.Id;

        public override string Name 
        {
            get { return itemType?.Name ?? StrNone; }
        }

        public override string ForgenName
        {
            get => item.ForgenName;
            set => item.ForgenName = value;
        }

        public override string FullName
        {
            get
            {
                StringBuilder builder = new();
                switch (GetQuality())
                {
                    case 4: builder.Append("Super"); break;
                    case 3: builder.Append("Elite"); break;
                    case 2: builder.Append("Unique"); break;
                    case 1: builder.Append("Refined"); break;
                }
                builder.Append(Name);
                if (Plus > 0)
                {
                    builder.Append($"(+{Plus})");
                }
                if (SocketOne != SocketGem.NoSocket)
                {
                    if (SocketTwo != SocketGem.NoSocket)
                    {
                        builder.Append(" 2-Socketed");
                    }
                    else
                    {
                        builder.Append(" 1-Socketed");
                    }
                }
                if (Effect != ItemEffect.None)
                {
                    builder.Append($" {Effect}");
                }
                return builder.ToString();
            }
        }

        public override uint Type => itemType?.Type ?? 0;

        /// <summary>
        ///     May be an NPC or Sash ID.
        /// </summary>
        public override uint OwnerIdentity
        {
            get => item.OwnerId ;
            set => item.OwnerId = value;
        }

        /// <summary>
        ///     The current owner of the equipment.
        /// </summary>
        public override uint PlayerIdentity
        {
            get => item.PlayerId;
            set => item.PlayerId = value;
        }

        public override ushort Amount
        {
            get => item.Amount;
            set => item.Amount = Math.Min(AmountLimit, value);
        }

        public override ushort AmountLimit
        {
            get => item.AmountLimit;
            set => item.AmountLimit = value;
        }

        public override byte Ident
        {
            get => item.Ident;
            set => item.Ident = (byte)value;
        }

        public override SocketGem SocketOne
        {
            get => (SocketGem)item.Gem1;
            set => item.Gem1 = (byte)value;
        }

        public override SocketGem SocketTwo
        {
            get => (SocketGem)item.Gem2;
            set => item.Gem2 = (byte)value;
        }

        public override SocketGem SocketThree
        {
            get => (SocketGem)item.Gem3;
            set => item.Gem3 = (byte)value;
        }

        public override ItemPosition Position
        {
            get => (ItemPosition)item.Position;
            set => item.Position = (byte)value;
        }

        public override byte Plus
        {
            get => item.Magic3;
            set => item.Magic3 = value;
        }

        public override uint Progress
        {
            get => item.Progress;
            set => item.Progress = value;
        }

        public override ItemEffect Effect
        {
            get => (ItemEffect)item.Magic1;
            set => item.Magic1 = (ushort)value;
        }

        public override ushort Magic1
        {
            get => item.Magic1;
            set => item.Magic1 = value;
        }

        public override byte Magic2
        {
            get => item.Magic2;
            set => item.Magic2 = value;
        }

        public override uint Locked
        {
            get => item.Locked;
            set => item.Locked = value;
        }

        public override byte Monopoly => item.Monopoly;

        public override bool IsUnident()
        {
            return (item.Ident & ITEM_STATUS_NOT_IDENT) == ITEM_STATUS_NOT_IDENT;
        }

        public override bool IsIdent()
        {
            return !IsUnident();
        }

        public override async Task SetIdentAsync(bool ident = false, bool update = false)
        {
            if (ident)
            {
                item.Ident |= ITEM_STATUS_NOT_IDENT;
            }
            else
            {
                int dataident = item.Ident;
                dataident &= ~ITEM_STATUS_NOT_IDENT;
                item.Ident = (byte)dataident;
            }

            if (update)
            {
                await SaveAsync();
            }
        }

        public override bool IsBound
        {
            get => item.Monopoly != 0;
            set
            {
                if (value)
                {
                    item.Monopoly |= ITEM_MONOPOLY_MASK;
                }
                else
                {
                    int monopoly = item.Monopoly;
                    monopoly &= ~ITEM_MONOPOLY_MASK;
                    item.Monopoly = (byte)monopoly;
                }
            }
        }

        public override uint Data
        {
            get => item.Data;
            set => item.Data = value;
        }

        public override uint SyndicateIdentity
        {
            get;
            set;
        }

        public override ushort AccumulateNum
        {
            get
            {
                if (IsPileEnable())
                {
                    return Amount;
                }

                return (ushort)Math.Max(1u, Amount);
            }
            set => Amount = (ushort)value;
        }

        public override ushort MaxAccumulateNum => (ushort)Math.Max(itemType?.AmountLimit ?? 1u, 1);


        #endregion

        #region Battle Attributes

        public override int BattlePower
        {
            get
            {
                if (!IsEquipment() || IsGarment())
                {
                    return 0;
                }

                if (IsBroken())
                {
                    return 0;
                }

                int ret = Math.Max(0, (int)Type % 10);
                ret += Plus;
                
                if (SocketOne != SocketGem.NoSocket)
                {
                    ret += 1 + (SocketOne.ToString().Contains("Super") ? 1 : 0);
                    switch (SocketOne)
                    {
                        case SocketGem.NormalAmber: ret += 1; break;
                        case SocketGem.RefinedAmber: ret += 3; break;
                        case SocketGem.SuperAmber: ret += 5; break;
                    }
                }

                if (SocketTwo != SocketGem.NoSocket)
                {
                    ret += 1 + (SocketTwo.ToString().Contains("Super") ? 1 : 0); 
                    switch (SocketTwo)
                    {
                        case SocketGem.NormalAmber: ret += 1; break;
                        case SocketGem.RefinedAmber: ret += 3; break;
                        case SocketGem.SuperAmber: ret += 5; break;
                    }

                    if (SocketOne == SocketTwo && SocketTwo == SocketGem.SuperAmber)
                    {
                        ret += 2;
                    }
                }

                return ret;
            }
        }

        public override int Life
        {
            get
            {
                int result = itemType?.Life ?? 0;
                result += itemAddition?.Life ?? 0;

                return result;
            }
        }

        public override int Mana => itemType?.Mana ?? 0;

        public override int MinAttack
        {
            get
            {
                if (IsBroken())
                {
                    return 0;
                }

                int result = itemType?.AttackMin ?? 0;
                result += itemAddition?.AttackMin ?? 0;

                return result;
            }
        }

        public override int MaxAttack
        {
            get
            {
                if (IsBroken())
                {
                    return 0;
                }

                int result = itemType?.AttackMax ?? 0;
                result += itemAddition?.AttackMax ?? 0;

                return result;
            }
        }

        public override int MagicAttackMin
        {
            get
            {
                if (IsBroken())
                {
                    return 0;
                }

                int result = itemType?.MgcAttackMin ?? 0;
                result += itemAddition?.MagicAtkMin ?? 0;

                return result;
            }
        }

        public override int MagicAttackMax
        {
            get
            {
                if (IsBroken())
                {
                    return 0;
                }

                int result = itemType?.MgcAttackMax ?? 0;
                result += itemAddition?.MagicAtkMax ?? 0;

                return result;
            }
        }

        public override int Defense
        {
            get
            {
                if (IsBroken())
                {
                    return 0;
                }

                int result = itemType?.Defense ?? 0;
                if (IsArrowSort())
                {
                    return result;
                }

                result += itemAddition?.Defense ?? 0;
                return result;
            }
        }

        public override int MagicDefense
        {
            get
            {
                if (IsBroken())
                {
                    return 0;
                }

                if (Position == ItemPosition.Armor || Position == ItemPosition.Helmet || Position == ItemPosition.Necklace)
                {
                    return itemAddition?.MagicDef ?? 0;
                }

                return itemType?.MagicDef ?? 0;
            }
        }

        public override int MagicDefenseBonus
        {
            get
            {
                if (IsBroken())
                {
                    return 0;
                }

                if (Position == ItemPosition.Armor || Position == ItemPosition.Helmet)
                {
                    int result = itemType?.MagicDef ?? 0;
                    return result;
                }

                return 0;
            }
        }

        public override int Agility
        {
            get
            {
                if (IsBroken())
                {
                    return 0;
                }

                //int result = Itemtype?.Af ?? 0;
                //return result;
                return 0;
            }
        }

        public override int Accuracy
        {
            get
            {
                if (IsBroken())
                {
                    return 0;
                }

                int result = itemAddition?.Dexterity ?? 0;
                return result;
            }
        }

        public override int Dodge
        {
            get
            {
                if (IsBroken())
                {
                    return 0;
                }

                int result = itemType?.Dodge ?? 0;
                result += itemAddition?.Dodge ?? 0;
                return result;
            }
        }

        public override byte EarthAttr
        {
            get => item.EudemonAttack1;
            set => item.EudemonAttack1 = value;
        }

        public override byte FireAttr
        {
            get => item.EudemonAttack2;
            set => item.EudemonAttack2 = value;
        }

        public override byte WaterAttr
        {
            get => item.EudemonAttack3;
            set => item.EudemonAttack3 = value;
        }

        public override byte AirAttr
        {
            get => item.EudemonAttack4;
            set => item.EudemonAttack4 = value;
        }

        public override byte SpecialAttr
        {
            get => item.SpecialEffect;
            set => item.SpecialEffect = value;
        }

        public int AttackRange => itemType?.AtkRange ?? 1;

        public override int AmethystGemEffect
        {
            get
            {
                int data = 0;
                switch (SocketOne)
                {
                    case SocketGem.NormalAmethyst: data += 20; break;
                    case SocketGem.RefinedAmethyst: data += 50; break;
                    case SocketGem.SuperAmethyst: data += 100; break;
                }

                switch (SocketTwo)
                {
                    case SocketGem.NormalAmethyst: data += 20; break;
                    case SocketGem.RefinedAmethyst: data += 50; break;
                    case SocketGem.SuperAmethyst: data += 100; break;
                }

                return data;
            }
        }

        public override int SapphireGemEffect
        {
            get
            {
                int data = 0;
                switch (SocketOne)
                {
                    case SocketGem.NormalSapphire: data += 10; break;
                    case SocketGem.RefinedSapphire: data += 15; break;
                    case SocketGem.SuperSapphire: data += 25; break;
                }

                switch (SocketTwo)
                {
                    case SocketGem.NormalSapphire: data += 10; break;
                    case SocketGem.RefinedSapphire: data += 15; break;
                    case SocketGem.SuperSapphire: data += 25; break;
                }

                return data;
            }
        }

        public override int BerylGemEffect
        {
            get
            {
                int data = 0;
                switch (SocketOne)
                {
                    case SocketGem.NormalBeryl: data += 5; break;
                    case SocketGem.RefinedBeryl: data += 10; break;
                    case SocketGem.SuperBeryl: data += 15; break;
                }

                switch (SocketTwo)
                {
                    case SocketGem.NormalBeryl: data += 5; break;
                    case SocketGem.RefinedBeryl: data += 10; break;
                    case SocketGem.SuperBeryl: data += 15; break;
                }

                return data;
            }
        }

        public override int CitrineGemEffect
        {
            get
            {
                int data = 0;
                switch (SocketOne)
                {
                    case SocketGem.NormalCitrine: data += 5; break;
                    case SocketGem.RefinedCitrine: data += 10; break;
                    case SocketGem.SuperCitrine: data += 20; break;
                }

                switch (SocketTwo)
                {
                    case SocketGem.NormalCitrine: data += 5; break;
                    case SocketGem.RefinedCitrine: data += 10; break;
                    case SocketGem.SuperCitrine: data += 20; break;
                }

                return data;
            }
        }

        public override int AmberGemEffect
        {
            get
            {
                int data = 0;
                switch (SocketOne)
                {
                    case SocketGem.NormalAmber: data += 1; break;
                    case SocketGem.RefinedAmber: data += 3; break;
                    case SocketGem.SuperAmber: data += 5; break;
                }

                switch (SocketTwo)
                {
                    case SocketGem.NormalAmber: data += 1; break;
                    case SocketGem.RefinedAmber: data += 3; break;
                    case SocketGem.SuperAmber: data += 5; break;
                }

                if (SocketOne == SocketTwo && SocketTwo == SocketGem.SuperAmber)
                {
                    data += 2;
                }

                return data;
            }
        }

        #endregion

        #region Activable

        public override int DeleteTime
        {
            get => item.DeleteTime;
            set => item.DeleteTime = value;
        }

        public override int AvailableTime
        {
            get => (int)item.AvailableTime;
            set => item.AvailableTime = (uint)value;
        }

        public override int RemainingSeconds => item.DeleteTime != 0 ? item.DeleteTime - UnixTimestamp.Now : 0;

        public override bool IsActivable()
        {
            return item.DeleteTime == 0 && (item.AvailableTime != 0 || itemType.SaveTime != 0);
        }

        public override bool HasExpired()
        {
            return item.DeleteTime != 0 && UnixTimestamp.Now > item.DeleteTime;
        }

        public override async Task<bool> ActivateAsync()
        {
            if (!IsActivable())
            {
                return false;
            }

            uint saveTime = item.AvailableTime;
            if (saveTime == 0 && itemType.SaveTime != 0)
            {
                saveTime = itemType.SaveTime;
            }

            item.DeleteTime = (int)(UnixTimestamp.Now + saveTime * 60);
            await SaveAsync();

            itemActivateLog.Information($"{PlayerIdentity},{OwnerIdentity},{Identity},{Name},{FullName},{AvailableTime},{DeleteTime}");
            return true;
        }

        public override async Task ExpireAsync()
        {
            if (!HasExpired())
            {
                return;
            }

            if (Position == ItemPosition.Inventory)
            {
                await user.UserPackage.RemoveFromInventoryAsync(this, UserPackage.RemovalType.Delete);
            }
            else if (Position is >= ItemPosition.EquipmentBegin and <= ItemPosition.EquipmentEnd)
            {
                await user.UserPackage.UnEquipAsync(Position, UserPackage.RemovalType.Delete);
            }
            else
            {
                return;
            }

            itemExpireLog.Information($"{PlayerIdentity},{OwnerIdentity},{Identity},{Name},{FullName},{AvailableTime},{DeleteTime}");
        }

        #endregion

        #region Equip Lock

        public override async Task<bool> TryUnlockAsync()
        {
            if (HasUnlocked())
            {
                await DoUnlockAsync();
                await user.SendAsync(new MsgItemInfo(this, MsgItemInfo.ItemMode.Update));
                return true;
            }

            if (IsUnlocking())
            {
                return false;
            }

            return true;
        }

        public override Task SetLockAsync()
        {
            item.Locked = SPECIAL_FLAG_LOCKED;
            return SaveAsync();
        }

        public override async Task SetUnlockAsync(uint unlockDate = 0)
        {
            if (IsUnlocking())
            {
                return;
            }

            if (unlockDate == 0)
            {
                unlockDate = uint.Parse(DateTime.Now.AddDays(5).ToString("yyMMdd"));
            }

            item.Locked = (unlockDate * 10000) + SPECIAL_FLAG_UNLOCKING;
            await SaveAsync();
        }

        public override async Task DoUnlockAsync()
        {
            item.Locked = 0;
            await SaveAsync();
        }

        public override bool HasUnlocked()
        {
            if (!IsUnlocking())
            {
                return false;
            }

            return (int)(Locked / 10000) < int.Parse(DateTime.Now.ToString("yyMMdd"));
        }

        public override bool IsLocked()
        {
            return (item.Locked % 10) == SPECIAL_FLAG_LOCKED;
        }

        public override bool IsUnlocking()
        {
            return (item.Locked % 10) == SPECIAL_FLAG_UNLOCKING;
        }

        #endregion

        #region WarGhost

        public override uint WarGhostExp
        {
            get => item.WarGhostExp;
            set => item.WarGhostExp = value;
        }

        public override int WarGhostLevel
        {
            get
            {
                uint exp = item.WarGhostExp;
                double warLevel = Math.Pow((double)exp / 2.0d, 1.0d / 3.0d);
                return (int)warLevel;
            }
        }

        #endregion

        #region Durability

        public override async Task<bool> RecoverDurabilityAsync()
        {
            AmountLimit = itemType.AmountLimit;
            await user.SendAsync(new MsgItemInfo(this, MsgItemInfo.ItemMode.Update));
            await SaveAsync();
            return true;
        }

        public override async Task RepairItemAsync()
        {
            if (user == null)
            {
                return;
            }

            if (!IsEquipment() && !IsWeapon())
            {
                return;
            }

            if (IsBroken())
            {
                //if (!await user.UserPackage.SpendMeteorsAsync(5))
                //{
                //    await user.SendAsync(StrItemErrRepairMeteor);
                //    return;
                //}

                Amount = AmountLimit;
                await SaveAsync();
                await user.SendAsync(new MsgItemInfo(this, MsgItemInfo.ItemMode.Update));
                repairItemLogger.Information(string.Format("User [{2}] repaired broken [{0}][{1}] with 5 meteors.", Type, Identity, PlayerIdentity));
                return;
            }

            if (Amount > AmountLimit)
            {
                Amount = AmountLimit;
                await SaveAsync();
                await user.SendAsync(new MsgItemInfo(this, MsgItemInfo.ItemMode.Update));
                return;
            }

            var nRecoverDurability = (ushort)(Math.Max(0u, AmountLimit) - Amount);
            if (nRecoverDurability == 0)
            {
                return;
            }

            int nRepairCost = GetRecoverDurCost() - 1;
            if (!await user.SpendMoneyAsync(Math.Max(1, nRepairCost), true))
            {
                return;
            }

            Amount = AmountLimit;
            await SaveAsync();
            await user.SendAsync(new MsgItemInfo(this, MsgItemInfo.ItemMode.Update));
            repairItemLogger.Information(string.Format("User [{2}] repaired broken [{0}][{1}] with {3} silvers.", Type, Identity, PlayerIdentity, nRepairCost));
        }

        #endregion

        #region Static

        public static DbItem CreateEntity(uint type, bool bound = false)
        {
            DbItemtype itemtype = ItemManager.GetItemtype(type);
            if (itemtype == null)
            {
                return null;
            }

            DbItem entity = new()
            {
                Magic1 = (byte)itemtype.Magic1,
                Magic2 = itemtype.Magic2,
                Magic3 = itemtype.Magic3,
                Type = type,
                Amount = itemtype.Amount,
                Data = type == 500001 ? 1000000u : 0u,
                AmountLimit = itemtype.AmountLimit,
                Gem1 = itemtype.Gem1,
                Gem2 = itemtype.Gem2,
                Monopoly = (byte)(bound ? 3 : 0),
            };
            entity.ChkSum = CalculateCheckSum(entity);
            return entity;
        }

        public static uint CalculateCheckSum(DbItem item)
        {
            return CalculateCheckSum(item.AmountLimit, item.Gem1, item.Gem2, item.Magic1, item.Magic3, item.Data);//, item.ReduceDmg, item.AddLife, item.AddlevelExp);
        }

        private static uint CalculateCheckSum(params uint[] values)
        {
            uint result = 0;
            foreach (uint value in values)
            {
                result ^= value;
                result = (result + 0x7ed55d16) + (result << 12);
                result = (result ^ 0xc761c23c) ^ (result >> 19);
                result = (result + 0x165667b1) + (result << 5);
                result = (result + 0xd3a2646c) ^ (result << 9);
                result = (result + 0xfd7046c5) + (result << 3);
                result = (result ^ 0xb55a4f09) ^ (result >> 16);
            }
            return result;
        }

        #endregion

        #region Database

        public override Task<bool> SaveAsync()
        {
            if (Identity == 0)
            {
                return ServerDbContext.CreateAsync(item);
            }

            item.ChkSum = CalculateCheckSum(item);
            return ServerDbContext.UpdateAsync(item);
        }

        public override Task<bool> DeleteAsync()
        {
            if (Identity == 0)
            {
                return Task.FromResult(false);
            }

            if (user != null)
            {
                user.LastDelItemIdentity = Identity;
            }
            //item.OwnerId = 0;
            //item.PlayerId = 0;
            //return SaveAsync();
            deleteItemLogger.Information($"{Identity},{Name},{OwnerIdentity},{PlayerIdentity},{Amount},{AmountLimit},{Plus},{SocketOne},{SocketTwo},{Effect}");
            // todo delete item status?
            return ServerDbContext.DeleteAsync(item);
        }

        #endregion
    }
}
