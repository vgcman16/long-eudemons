using Long.Database.Entities;
using Long.Kernel.Managers;
using Long.Kernel.States;
using Long.Kernel.States.Items;
using Long.Kernel.States.Npcs;
using Long.Kernel.States.User;
using Long.Kernel.States.World;
using Long.Network.Packets;
using Long.Shared.Helpers;
using System.Drawing;

namespace Long.Kernel.Network.Game.Packets
{
    public sealed class MsgItem : MsgBase<GameClientBase>
    {
        private static readonly ILogger logger = Log.ForContext<MsgItem>();
        private static readonly ILogger shopPurchaseLogger = Logger.CreateLogger("shop_purchase");
        private static readonly ILogger shopSellLogger = Logger.CreateLogger("shop_sell");

        #region
        enum NEWITEMACT
        {
            ITEMACT_NONE = 0,
            ITEMACT_BUY,                    // to server, id: idNpc, data: idItemType
            ITEMACT_SELL,                   // to server, id: idNpc, data: idItem
            ITEMACT_DROP,                   // to server, x, y
            ITEMACT_USE,                    // to server, data: position
            ITEMACT_EQUIP = 5,              // to client£¬Í¨Öª×°±¸ÎïÆ·
            ITEMACT_UNEQUIP,                // to server, data: position
            ITEMACT_SPLITITEM,              // to server, data: num
            ITEMACT_COMBINEITEM,            // to server, data: id
            ITEMACT_QUERYMONEYSAVED,        // to server/client, id: idNpc, data: nMoney
            ITEMACT_SAVEMONEY = 10,         // to server, id: idNpc, data: nMoney
            ITEMACT_DRAWMONEY,              // to server, id: idNpc, data: nMoney
            ITEMACT_DROPMONEY,              // to server, x, y
            ITEMACT_SPENDMONEY,             // ÎÞÒâÒå£¬ÔÝ±£Áô
            ITEMACT_REPAIR,                 // to server, return MsgItemInfo
            ITEMACT_REPAIRALL = 15,         // to server, return ITEMACT_REPAIRAll, or not return if no money
            ITEMACT_IDENT,                  // to server, return MsgItemInfo
            ITEMACT_DURABILITY,             // to client, update durability
            ITEMACT_DROPEQUIPMENT,          // to client, delete equipment
            ITEMACT_IMPROVE,                // to server, improve equipment
            ITEMACT_UPLEV = 20,             // to server, upleve equipment
            ITEMACT_BOOTH_QUERY,            // to server, open booth, data is npc id
            ITEMACT_BOOTH_ADD,              // to server/client(broadcast in table), id is idItem, data is money
            ITEMACT_BOOTH_DEL,              // to server/client(broadcast), id is idItem, data is npc id
            ITEMACT_BOOTH_BUY,              // to server, id is idItem, data is npc id
            ITEMACT_SYNCHRO_AMOUNT = 25,    // to client(data is amount)
            ITEMACT_FIREWORKS,

            ITEMACT_COMPLETE_TASK,          // to server, Íê³ÉÓ¶±øÈÎÎñ, id=ÈÎÎñID, dwData=Ìá½»µÄÎïÆ·£¬Èç¹ûÃ»ÓÐÔòÎªID_NONE
            ITEMACT_EUDEMON_EVOLVE,         // to server, »ÃÊÞ½ø»¯, id=»ÃÊÞÎïÆ·ID£¬dwData=½ø»¯³ÉµÄÀàÐÍ£¬È¡Öµ·¶Î§1~2
            ITEMACT_EUDEMON_REBORN,         // to server, »ÃÊÞ¸´»î£¬id=»ÃÊÞÎïÆ·ID, data=±¦Ê¯ID
            ITEMACT_EUDEMON_ENHANCE = 30,   // to server, »ÃÊÞÇ¿»¯£¬id=»ÃÊÞÎïÆ·ID, data=±¦Ê¯ID
            ITEMACT_CALL_EUDEMON,           // to server, ÕÙ»½»ÃÊÞ£¬id=»ÃÊÞÎïÆ·ID
            ITEMACT_KILL_EUDEMON,           // to server, ÊÕ»Ø»ÃÊÞ£¬id=»ÃÊÞÎïÆ·ID
                                            // to client, ÊÕ»Ø»ÃÊÞµÄ¹âÐ§±íÏÖ£¬id=»ÃÊÞÎïÆ·ID, dwData=»ÃÊÞID
            ITEMACT_EUDEMON_EVOLVE2,        // to server, »ÃÊÞ¶þ´Î½ø»¯, id=»ÃÊÞÎïÆ·ID£¬dwData=½ø»¯³ÉµÄÀàÐÍ£¬È¡Öµ·¶Î§1~8
            ITEMACT_EUDEMON_ATKMODE,        // to server, ÉèÖÃ»ÃÊÞ¹¥»÷Ä£Ê½, id=»ÃÊÞÎïÆ·ID£¬dwData=¹¥»÷Ä£Ê½£¬È¡Öµ·¶Î§1~3
            ITEMACT_ATTACH_EUDEMON = 35,    // »ÃÊÞÓëÈËºÏÌå, id=»ÃÊÞÎïÆ·ID
            ITEMACT_DETACH_EUDEMON,         // ½â³ý»ÃÊÞÓëÈËºÏÌå×´Ì¬, id=»ÃÊÞÎïÆ·ID
        };
        #endregion

        public MsgItem()
        {
        }

        public MsgItem(uint identity, ItemActionType action, uint cmd = 0, uint param = 0)
        {
            Identity = identity;
            Command = cmd;
            Action = action;
            Data = param;
        }

        public uint Identity { get; set; }

        #region Union - Command
        public uint Command { get; set; }

        public ushort CommandX
        {
            get => (ushort)(Command - (CommandY << 16));
            set => Command = (uint)(CommandY << 16 | value);
        }

        public ushort CommandY
        {
            get => (ushort)(Command >> 16);
            set => Command = (uint)(value << 16) | Command;
        }
        #endregion

        public ItemActionType Action { get; set; }
        public ushort Param { get; set; }

        #region Union - Data

        public uint Data { get; set; }

        public ushort Amount
        {
            get => (ushort)(Data & 0xFFFF);
            set => Data = (Data & 0xFFFF0000) | value;
        }

        public byte Unknown
        {
            get => (byte)((Data >> 16) & 0xFF);
            set => Data = (Data & 0xFF00FFFF) | ((uint)value << 16);
        }

        public byte MoneyType
        {
            get => (byte)((Data >> 24) & 0xFF);
            set => Data = (Data & 0x00FFFFFF) | ((uint)value << 24);
        }

        #endregion

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
            Identity = reader.ReadUInt32();                 // 4
            Command = reader.ReadUInt32();                  // 8
            Action = (ItemActionType)reader.ReadUInt16();   // 12
            Param = reader.ReadUInt16();                    // 14
            Data = reader.ReadUInt32();                     // 16
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
            writer.Write((ushort)PacketType.MsgItem);
            writer.Write(Identity);         // 4
            writer.Write(Command);          // 8
            writer.Write((ushort)Action);   // 12
            writer.Write(Param);            // 14
            writer.Write(Data);             // 16
            return writer.ToArray();
        }

        /// <summary>
        ///     Enumeration type for defining item actions that may be requested by the user,
        ///     or given to by the server. Allows for action handling as a packet subtype.
        ///     Enums should be named by the action they provide to a system in the context
        ///     of the player item.
        /// </summary>
        public enum ItemActionType
        {
            ShopPurchase = 1,
            ShopSell,
            InventoryRemove,
            InventoryEquip,
            EquipmentWear,
            EquipmentRemove,
            EquipmentSplit,
            EquipmentCombine,
            BankQuery,
            BankDeposit,
            BankWithdraw,
            EquipmentRepair = 14,
            EquipmentRepairAll,
            Identify = 16,
            EquipmentImprove = 19,
            EquipmentLevelUp,
            BoothQuery,
            BoothSell,
            BoothRemove,
            BoothPurchase,
            EquipmentAmount,
            Fireworks,
            ClientPing = 27,
            EquipmentEnchant,
            RedeemEquipment = 32,
            DetainEquipment = 33,
            DetainRewardClose = 34,
            TalismanProgress = 35,
            TalismanProgressEmoney = 36,
            InventoryDropItem = 37,
            InventoryDropSilver = 38,
            GemCompose = 39,
            TortoiseCompose = 40,
            ActivateAccessory = 41,
            SocketEquipment = 43,
            MainEquipment = 44,
            AlternativeEquipment = 45,
            DisplayGears = 46,
            MergeItems = 48,
            SplitItems = 49,
            ComposeRefinedTortoiseGem = 51,
            BoothEmoneySell = 52,
            ForgingBuy = 55,

            EvolveEudemon = 28,
            EudemonReborn = 29,
            EudemonEnhance = 30,
            CallEudemon = 31,
            KillEudemon = 32,
            Evolve2Eudemon = 33,
            EudemonAtkMode = 34,
            AttatchEudemon = 35,
            DetatchEudemon = 36,

            Eudemon0 = 42, // remove probably. sent when hatched with type 255

            ExpCrystalRequest = 54,
            ForgeEudCrystal = 56,

            ExpBallRequest = 50,

            EquipmentOpenSocket = 59, // works for any basically.

            ItemDataAssert = 62,
            ItemDataOverUse = 63,

            SearchBoothAdvertise = 114,
            RequestBoothAdvertise = 115,
            BuyFromBoothAdvertise = 116,

            ItemLock = 120,
            ItemUnlock = 121,

            ActivateItem = 129,
        }

        public enum Moneytype
        {
            EudemonsPoints = 3,
            Silver = 4,

            EudemonsPointsBound,
            EudemonHacheryEx = 19,
        }

        public override async Task ProcessAsync(GameClientBase client)
        {
            Character user = client.Character;
            BaseNpc npc;
            ItemBase item;
           
            /*
             * Handled by modules skip.
             */
            if ((Action >= ItemActionType.BoothQuery && Action <= ItemActionType.BoothPurchase)
                || Action == ItemActionType.BoothEmoneySell)
            {
                return;
            }

            switch (Action)
            {
                default:
                    {
                        logger.Error($"MsgItem()->ProcessAsync(): Unknown action {Action}.");
                        break;
                    }
                case ItemActionType.ShopPurchase: // 1
                    {
                        int[] remoteShopping =
                        {
                            1207, // EmoneyShop
                            1997, // HairShop
                            1998  // AvatarShop
                        };
                        
                        // when requested from ExtraHatch the id becomes 0, so we set it to the emoney shop.
                        if (Identity == 0)
                        {
                            // when buy from here, the item must be set directly to extra hatch (respect the limit of the player set by max_eud_hatch_ex!)
                            Identity = 1207;
                        }

                        npc = user.Map.QueryRole<BaseNpc>(Identity);
                        if (npc == null)
                        {
                            npc = RoleManager.GetRole<BaseNpc>(Identity);
                            if (npc == null)
                            {
                                return;
                            }

                            if (npc.MapIdentity != GameMap.NPC_JAIL_ID && npc.MapIdentity != user.MapIdentity)
                            {
                                return;
                            }
                        }

                        if (npc.MapIdentity != GameMap.NPC_JAIL_ID 
                            && remoteShopping.All(x => x != npc.Identity) 
                            && npc.GetDistance(user) > Screen.VIEW_SIZE)
                        {
                            return;
                        }

                        DbGoods goods = npc.ShopGoods.FirstOrDefault(x => x.Itemtype == Command);
                        if (goods == null)
                        {
                            logger.Warning("Invalid goods itemtype {0} for Shop {1}", Command, Identity);
                            return;
                        }

                        DbItemtype itemtype = ItemManager.GetItemtype(Command);
                        if (itemtype == null)
                        {
                            logger.Warning("Invalid goods itemtype (not existent) {0} for Shop {1}", Command, Identity);
                            return;
                        }

                        var amount = (int)Math.Max(1, (int)Amount);
                        if (!user.UserPackage.IsPackSpare(amount, itemtype.Type))
                        {
                            await user.SendAsync(StrYourBagIsFull);
                            return;
                        }

                        int price;
                        string moneyTypeString = ((Moneytype)goods.PayType).ToString();
                        const byte MONOPOLY_NONE_B = 0;
                        const byte MONOPOLY_BOUND_B = ItemBase.ITEM_MONOPOLY_MASK;
                        byte monopoly = MONOPOLY_NONE_B;
                        switch ((Moneytype)goods.PayType)
                        {
                            case Moneytype.Silver:
                                {
                                    if (itemtype.Price == 0)
                                    {
                                        return;
                                    }

                                    price = (int)(itemtype.Price * amount);
                                    if (!await user.SpendMoneyAsync(price, true))
                                    {
                                        return;
                                    }
                                    break;
                                }

                            case (Moneytype)19://eudemons shop store emoney
                            case Moneytype.EudemonsPoints:
                                {
                                    if (MoneyType == 2)
                                    {
                                        if (itemtype.EmoneyPrice == 0)
                                        {
                                            return;
                                        }

                                        price = (int)(itemtype.EmoneyPrice * amount);
                                        if (!await user.SpendEudemonPointsAsync(price, true))
                                        {
                                            return;
                                        }
                                        await user.SaveEmoneyLogAsync(Character.EmoneyOperationType.Npc, 0, 0, (uint)price);
                                    }
                                    else if (MoneyType == 1)
                                    {
                                        if (itemtype.EmoneyPrice == 0)
                                        {
                                            return;
                                        }

                                        price = (int)(itemtype.EmoneyPrice * amount);
                                        if (!await user.SpendBoundEudemonPointsAsync(Character.EmoneyOperationType.EmoneyShop, price, true))
                                        {
                                            return;
                                        }

                                        monopoly = MONOPOLY_BOUND_B;
                                        moneyTypeString += "(B)";
                                    }
                                    else
                                    {
                                        logger.Warning("Invalid money type {0}", MoneyType);
                                        return;
                                    }
                                    break;
                                }

                            default:
                                {
                                    logger.Warning("Invalid moneytype {0}/{1}/{2} - {3}({4})", (Moneytype)MoneyType, Identity, Command, user.Identity, user.Name);
                                    return;
                                }
                        }

                        await user.UserPackage.AwardItemAsync(itemtype.Type, amount, monopoly != 0);

                        shopPurchaseLogger.Information($"Purchase,{user.Identity},{user.Name},{user.Level},{user.MapIdentity},{user.X},{user.Y},{goods.OwnerIdentity},{goods.Itemtype},{goods.PayType},{moneyTypeString},{amount},{price}");
                        break;
                    }
                
                case ItemActionType.ShopSell:
                    {
                        npc = user.Map.QueryRole<BaseNpc>(Identity);
                        if (npc == null)
                        {
                            return;
                        }

                        if (npc.MapIdentity != user.MapIdentity || npc.GetDistance(user) > Screen.VIEW_SIZE)
                        {
                            return;
                        }

                        item = user.UserPackage.FindItemByIdentity(Command);
                        if (item == null)
                        {
                            return;
                        }

                        if (item.IsLocked())
                        {
                            return;
                        }

                        int price = item.GetSellPrice();
                        if (!await user.UserPackage.SpendItemAsync(item))
                        {
                            return;
                        }

                        shopSellLogger.Information($"{user.Identity},{user.Name},{user.Level},{user.MapIdentity},{user.X},{user.Y},{item.Identity},{item.FullName},{item.Type},{price}");

                        await user.AwardMoneyAsync(price);

                        break;
                    }

                //case ItemActionType.InventoryDropItem: 
                //case ItemActionType.InventoryRemove:
                //    {
                //        await user.DropItemAsync(Identity, user.X, user.Y);
                //        break;
                //    }

                //case ItemActionType.InventoryDropSilver: 
                //    {
                //        await user.DropSilverAsync(Identity);
                //        break;
                //    }

                case ItemActionType.InventoryEquip:
                case ItemActionType.EquipmentWear:
                    {
                        if (Data == 0 || Data == user.Identity)
                        {
                            if (!await user.UserPackage.UseItemAsync(Identity, (ItemBase.ItemPosition)Command))
                            {
                                await user.SendAsync(StrUnableToUseItem, TalkChannel.TopLeft, Color.Red);
                            }
                        }
                        else
                        {
                            if (!await user.UserPackage.UseItemToAsync(Data, Identity))
                            {
                                await user.SendAsync(StrUnableToUseItem, TalkChannel.TopLeft, Color.Red);
                            }
                        }
                        break;
                    }

                case ItemActionType.EquipmentRemove:
                    {
                        if (!await user.UserPackage.UnEquipAsync((ItemBase.ItemPosition)Command))
                        {
                            await user.SendAsync(StrYourBagIsFull, TalkChannel.TopLeft, Color.Red);
                        }

                        break;
                    }

                case ItemActionType.BankQuery:
                    {
                        Command = user.StorageMoney;
                        await user.SendAsync(this);
                        break;
                    }

                case ItemActionType.BankDeposit:
                    {
                        if (user.Silvers < Command)
                        {
                            return;
                        }

                        if (Command + (long)user.StorageMoney > Role.MAX_STORAGE_MONEY)
                        {
                            await user.SendAsync(string.Format(StrSilversExceedAmount, int.MaxValue));
                            return;
                        }

                        if (!await user.SpendMoneyAsync((int)Command, true))
                        {
                            return;
                        }

                        user.StorageMoney += Command;

                        Action = ItemActionType.BankQuery;
                        Command = user.StorageMoney;
                        await user.SendAsync(this);
                        await user.SaveAsync();
                        break;
                    }

                case ItemActionType.BankWithdraw:
                    {
                        if (Command > user.StorageMoney)
                        {
                            return;
                        }

                        if (Command + user.Silvers > int.MaxValue)
                        {
                            await user.SendAsync(string.Format(StrSilversExceedAmount, int.MaxValue));
                            return;
                        }

                        user.StorageMoney -= Command;
                        await user.AwardMoneyAsync((int)Command);

                        Action = ItemActionType.BankQuery;
                        Command = user.StorageMoney;
                        await user.SendAsync(this);
                        await user.SaveAsync();
                        break;
                    }

                case ItemActionType.EquipmentRepair:
                    {
                        item = user.UserPackage.FindItemByIdentity(Identity);
                        if (item != null && item.Position == ItemBase.ItemPosition.Inventory)
                        {
                            await item.RepairItemAsync();
                        }
                        break;
                    }

                case ItemActionType.EquipmentRepairAll:
                    {
                        if (user.VipLevel < 2)
                        {
                            return;
                        }

                        for (ItemBase.ItemPosition pos = ItemBase.ItemPosition.EquipmentBegin; pos <= ItemBase.ItemPosition.EquipmentEnd; pos++)
                        {
                            item = user.GetEquipment(pos);
                            if (item != null && user.UserPackage.TryItem(item.Identity, item.Position))
                            {
                                await item.RepairItemAsync();
                            }
                            item = null;
                        }

                        break;
                    }

                case ItemActionType.Identify:
                    {
                        item = user.UserPackage.FindItemByIdentity(Identity);
                        if (item == null)
                        {
                            return;
                        }

                        if (await user.SpendMoneyAsync(100))
                        {
                            if (await NextAsync(100) < 20)
                            {
                                await user.UserPackage.RemoveFromInventoryAsync(item, UserPackage.RemovalType.Delete);
                                return;
                            }

                            await item.SetIdentAsync(false, true);
                            await user.SendAsync(new MsgItemInfo(item, MsgItemInfo.ItemMode.Update));
                            await user.SendAsync(this);
                        }
                        break;
                    }

                case ItemActionType.ItemLock:
                    {
                        item = user.UserPackage.FindItemByIdentity(Identity);
                        if (item != null && !item.IsLocked())
                        {
                            var lockItem = user.UserPackage.FindItemByIdentity(Command);
                            if (lockItem != null && !await user.UserPackage.SpendItemAsync(lockItem))
                            {
                                return;
                            }

                            await item.SetLockAsync();
                            await user.SendAsync(new MsgItemInfo(item, MsgItemInfo.ItemMode.Update));
                            await user.SendAsync(this);
                            await user.SendAsync(string.Format(StrItemLocked, item.Name));
                        }
                        break;
                    }

                case ItemActionType.ItemUnlock:
                    {
                        var lockItem = user.UserPackage.FindItemByIdentity(Command);
                        if (lockItem == null)
                        {
                            return;
                        }

                        item = user.UserPackage.FindItemByIdentity(Identity);
                        if (item != null && item.IsLocked())
                        {
                            if (!await user.UserPackage.SpendItemAsync(lockItem))
                            {
                                return;
                            }

                            await item.SetUnlockAsync();
                            await user.SendAsync(new MsgItemInfo(item, MsgItemInfo.ItemMode.Update));
                            await user.SendAsync(this);
                        }
                        break;
                    }
            }
        }
    }
}
