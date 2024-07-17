using Long.Database.Entities;
using Long.Kernel.Database;
using Long.Kernel.Database.Repositories;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.Auction;
using Long.Kernel.States.Items;
using Long.Kernel.States.User;
using Long.Shared.Helpers;
using System.Drawing;

namespace Long.Kernel.Managers
{
    public class AuctionManager
    {
        public const byte STATE_PLAYER_ITEM = 1;

        public const int AUCTION_REVENUE = 3;
        public const int DEFAULT_MAIL_TIME = 60 * 60 * 24 * 30;
        private static readonly ILogger logger = Log.ForContext<AuctionManager>();
        private static readonly ILogger gmLogger = Logger.CreateLogger("auction_manager");
        private static readonly TimeOut checkAuctionTimer = new(10);

        private static readonly Dictionary<uint, AuctionItem> AuctionItems = new();

        private static readonly object Lock = new();

        public static async Task InitializeAsync()
        {
            logger.Information($"Initializing Auction data");

            foreach (var dbAuction in await AuctionRepository.GetAsync())
            {
                AuctionItem auctionItem = new(dbAuction);
                if (!await auctionItem.InitializeAsync())
                {
                    continue;
                }

                if (auctionItem.RemainingTime == AuctionRemainingTime.Expired)
                {
                    await auctionItem.TimeOutAsync();
                    await auctionItem.DeleteAsync();
                    continue;
                }

                AuctionItems.Add(dbAuction.ItemId, auctionItem);
            }
        }

        public static bool IsItemAuctionable(ItemBase item)
        {
            if (item == null)
            {
                return false;
            }

            if (item.IsLocked())
            {
                return false;
            }

            if (item.IsMonopoly())
            {
                return false;
            }

            if (item.IsSuspicious())
            {
                return false;
            }

            if (item.IsBound)
            {
                return false;
            }

            if (item.Position != ItemBase.ItemPosition.Inventory)
            {
                return false;
            }

            if (item.SyndicateIdentity != 0)
            {
                return false;
            }

            lock (Lock)
            {
                if (AuctionItems.ContainsKey(item.Identity))
                {
                    return false;
                }
            }
            return true;
        }

        public static async Task AddNewItemAsync(Character sender, uint npcId, uint itemId, uint price, int duration)
        {
            ItemBase item = sender.UserPackage.GetItem(itemId);
            if (!IsItemAuctionable(item))
            {
                logger.Warning("User[{0},{1}] tried to auction invalid item {2} - {3}", sender.Identity, sender.Name, item.Identity, item.FullName);
                return;
            }

            if (price < 100)//min value
            {
                await sender.SendAsync("O valor mínimo para colocar em leilão é de 100 pratas!");
                return;
            }

            if (item.IsGift)
            {
                await sender.SendAsync("Você não pode leiloar um presente!");
                return;
            }

            // the auction full package size for the player is 50 for what i can see, need to test and check!
            //if (m_pAuctionInfo->m_SetAuctionQueue.size() >= AUCTION_PACKAGE_MAX)
            //{
            //    //O armazém está cheio, por favor aguarde até haver espaço antes de colocar os seus itens de licitação.
            //    pUser->SendSysMsg(STR_AUCTION_BID_PACKAGE_FULL);
            //    return false;
            //}

            if (sender.Silvers < price * AUCTION_REVENUE / 100)
            {
                // message less money?
                await sender.SendAsync("Você não tem dinheiro suficiente para pagar a comissão do leilão.");
                return;
            }

            DbAuction dbAuction = new()
            {
                ItemId = item.Identity,
                UserId = sender.Identity,
                UserName = sender.Name,
                State = STATE_PLAYER_ITEM,
                Price = price,
                TimeOut = UnixTimestamp.Now + (duration * 60 * 60)
            };

            AuctionItem auctionItem = new(dbAuction);
            if (!await auctionItem.InitializeAsync())
            {
                logger.Error("Error when initialize auction item!");
                return;
            }

            await ServerDbContext.CreateAsync(dbAuction);
            lock (Lock)
            {
                AuctionItems.Add(item.Identity, auctionItem);
            }

            await sender.Packages.AddItemAsync(npcId, item, MsgPackage.StorageType.AuctionStorage, true);
        }

        public static List<AuctionItem> GetCurrentBid(Character user)
        {
            List<AuctionItem> list = new();
            List<AuctionItem> snapshot = GetCurrentItems();
            foreach (var auctionItem in snapshot
                .Where(x => x.RemainingTime != AuctionRemainingTime.Expired)
                .OrderBy(x => x.RemainingTime))
            {
                if (auctionItem.UserHasBid(user.Identity) != null)
                {
                    list.Add(auctionItem);
                }
            }
            return list;
        }

        public static List<AuctionItem> GetActiveSales(Character user)
        {
            List<AuctionItem> list = new();
            List<AuctionItem> snapshot = GetCurrentItems();
            foreach (var auctionItem in snapshot
                .Where(x => x.RemainingTime != AuctionRemainingTime.Expired)
                .OrderBy(x => x.RemainingTime))
            {
                if (auctionItem.SellerId == user.Identity)
                {
                    list.Add(auctionItem);
                }
            }
            return list;
        }

        public static List<AuctionItem> QueryItems(int page, AuctionMoneyType moneyType, string itemName, byte quality,
            byte sockets, byte composition, ushort category, ushort levelLow, ushort levelHigh, out int total)
        {
            const int ipp = 50;
            List<AuctionItem> list = new();
            var snapshot = GetCurrentItems().Where(x => x.RemainingTime != AuctionRemainingTime.Expired && moneyType == x.MoneyType);
            if (!string.IsNullOrEmpty(itemName))
            {
                snapshot = snapshot.Where(x => x.ItemName.Contains(itemName, StringComparison.InvariantCultureIgnoreCase));
            }

            if (quality != 0)
            {
                quality += 4;
                quality %= 10;
                snapshot = snapshot.Where(x =>
                {
                    var item = x.GetItem();

                    if (!item.IsEquipment())
                    {
                        return false;
                    }

                    if (item.GetQuality() < quality)
                    {
                        return false;
                    }

                    return true;
                });
            }

            if (sockets != 0)
            {
                snapshot = snapshot.Where(x => x.SocketNum >= sockets);
            }

            if (composition != 0)
            {
                snapshot = snapshot.Where(x => x.GetItem().Plus >= composition);
            }

            if (category != 0)
            {
                snapshot = snapshot.Where(x => x.Category == category);
            }

            if (levelLow != 0)
            {
                snapshot = snapshot.Where(x => x.GetItem().RequiredLevel >= levelLow);
            }

            if (levelHigh != 0)
            {
                snapshot = snapshot.Where(x => x.GetItem().RequiredLevel <= levelHigh);
            }

            total = snapshot.Count();

            foreach (var auctionItem in snapshot
                .Skip(page * ipp).Take(ipp))
            {
                list.Add(auctionItem);
            }
            return list;
        }

        public static async Task<bool> PlaceBidAsync(Character user, uint itemId, uint value, int amount = 1)
        {
            AuctionItem auctionItem;
            lock (Lock)
            {
                if (!AuctionItems.TryGetValue(itemId, out auctionItem))
                {
                    return false;
                }
            }

            if (auctionItem.CurrentPrice > value)
            {
                // hummmm
                return false;
            }
            else if (auctionItem.CurrentPrice == value && auctionItem.BiggestBid != null)
            {
                return false;
            }

            await auctionItem.PlaceBidAsync(user, value, amount);

            AuctionBid currentBid = auctionItem.BiggestBid;
            return true;
        }

        public static async Task<bool> BuyAtFixedPriceAsync(Character user, uint itemId)
        {
            AuctionItem auctionItem;
            lock (Lock)
            {
                if (!AuctionItems.TryGetValue(itemId, out auctionItem))
                {
                    return false;
                }
            }

            ItemBase item = auctionItem.GetItem();
            if (!user.UserPackage.IsPackSpare((int)item.AccumulateNum, item.Type))
            {
                await user.SendAsync(string.Format(StrNotEnoughSpaceN, item.AccumulateNum));
                return false;
            }

            if (auctionItem.MoneyType == AuctionMoneyType.Money)
            {
                if (user.Silvers < auctionItem.Buyout)
                {
                    return false;
                }
            }
            else
            {
                if (user.EudemonPoints < auctionItem.Buyout)
                {
                    return false;
                }
            }

            if (await auctionItem.BuyoutAsync(user))
            {
                return true;
            }
            return false;
        }

        private static List<AuctionItem> GetCurrentItems()
        {
            List<AuctionItem> items;
            lock (Lock)
            {
                items = AuctionItems.Values.ToList();
            }
            return items;
        }

        public static async Task<bool> CancelAsync(Character user, uint itemId)
        {
            AuctionItem auctionItem;
            lock (Lock)
            {
                if (!AuctionItems.TryGetValue(itemId, out auctionItem))
                {
                    return false;
                }
            }

            if (user.Identity != auctionItem.SellerId)
            {
                return false;
            }

            await auctionItem.CancelAsync();

            lock (Lock)
            {
                AuctionItems.Remove(itemId);
            }
            return true;
        }

        public static async Task OnTimerAsync()
        {
            if (!checkAuctionTimer.ToNextTime())
            {
                return;
            }

            List<uint> expired = new();
            lock (Lock)
            {
                foreach (var auctionItem in AuctionItems)
                {
                    if (auctionItem.Value.RemainingTime == AuctionRemainingTime.Expired || auctionItem.Value.Sold)
                    {
                        expired.Add(auctionItem.Key);
                    }
                }
            }

            foreach (var expAuctionId in expired)
            {
                AuctionItem auctionItem;
                lock (Lock)
                {
                    if (!AuctionItems.Remove(expAuctionId, out auctionItem))
                    {
                        continue;
                    }
                }
                await auctionItem.TimeOutAsync();
                await auctionItem.DeleteAsync();
            }
        }

        public enum AuctionRemainingTime
        {
            VeryLong,
            Long,
            Medium,
            Short,
            Expired
        }

        public enum AuctionMoneyType
        {
            None,
            Money,
            Emoney
        }
    }
}
