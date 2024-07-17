using Long.Database.Entities;
using Long.Database.Entities.Long.Database.Entities;
using Long.Kernel.Database.Repositories;
using Long.Kernel.States.Items;
using System.Collections.Concurrent;

namespace Long.Kernel.Managers
{
    public class ItemManager
    {
        private static readonly ILogger logger = Log.ForContext<ItemManager>();

        private static ConcurrentDictionary<uint, DbItemtype> itemtypes = new();
        private static ConcurrentDictionary<ulong, DbItemAddition> itemAdditions = new();
        private static ConcurrentDictionary<uint, DbItemLimit> itemLimits = new();

        private static ConcurrentDictionary<uint, DbGrade> eudemonGrade = new();
        private static ConcurrentDictionary<uint, DbEudemonRbnRqr> eudemonRbnRqr = new();
        private static ConcurrentDictionary<uint, DbEudemonRbnType> eudemonRbnType = new();
        private static ConcurrentDictionary<uint, List<DbOfficialType>> officialTypes = new();

        public static async Task InitializeAsync()
        {
            foreach (var item in ItemtypeRepository.Get())
            {
                itemtypes.TryAdd(item.Type, item);
            }

            foreach (var addition in ItemAdditionRepository.Get())
            {
                itemAdditions.TryAdd(AdditionKey(addition.TypeId, addition.Level), addition);
            }

            foreach (var grade in GradeRepository.Get())
            {
                eudemonGrade.TryAdd(grade.Id, grade);
            }

            foreach(var rbnType in await EudemonRbnRepository.GetRebornTypeAsync())
            {
                eudemonRbnType.TryAdd(rbnType.Id, rbnType);
            }

            foreach (var rbnRequire in await EudemonRbnRepository.GetRebornRqrAsync())
            {
                eudemonRbnRqr.TryAdd(rbnRequire.Id, rbnRequire);
            }

            foreach (var official in await OfficialTypeRepository.GetAsync())
            {
                if (officialTypes.TryGetValue(official.OfficialType, out var type))
                {
                    type.Add(official);
                }
                else
                {
                    officialTypes.TryAdd(official.OfficialType, new() { official });
                }
            }

            logger.Information($"Loaded {itemtypes.Count} itemtype`s...");
            logger.Information($"Loaded {itemAdditions.Count} itemadditions`s...");
            logger.Information($"Loaded {eudemonGrade.Count} grades`s...");
            logger.Information($"Loaded {eudemonRbnRqr.Count} reborn rqrs`s...");
            logger.Information($"Loaded {eudemonRbnType.Count} reborn type`s...");
            logger.Information($"Loaded {officialTypes.Count} official type`s...");
        }

        public static List<DbItemtype> GetByRange(int mobLevel, int tolerationMin, int tolerationMax, int maxLevel = 120)
        {
            return itemtypes.Values.Where(x =>
                x.ReqLevel >= mobLevel - tolerationMin && x.ReqLevel <= mobLevel + tolerationMax &&
                x.ReqLevel <= maxLevel).ToList();
        }

        public static DbItemtype GetItemtype(uint type)
        {
            return itemtypes.TryGetValue(type, out var item) ? item : null;
        }

        public static DbItemAddition GetItemAddition(uint type, byte level)
        {
            return itemAdditions.TryGetValue(AdditionKey(type, level), out var item) ? item : null;
        }

        public static DbGrade GetEudemonGrade(uint type)
        {
            return eudemonGrade.TryGetValue(type, out var grade) ? grade : null;
        }

        public static DbEudemonRbnRqr GetEudemonRbnRqr(uint type)
        {
            return eudemonRbnRqr.TryGetValue(type, out var rbn) ? rbn : null;
        }

        public static DbEudemonRbnType GetDbEudemonRbnType(uint type)
        {
            return eudemonRbnType.TryGetValue(type, out var rbn) ? rbn : null;
        }

        public static DbOfficialType GetOfficialType(uint type, int starLevel)
        {
            if (!officialTypes.TryGetValue(type, out var officialType))
            {
                return null;
            }

            return officialType
                .Where(x => x.ReqLevel < starLevel)
                .OrderBy(x => x.ReqLevel)
                .LastOrDefault();
        }

        private static ulong AdditionKey(uint type, byte level)
        {
            uint key = type/ 10;
            key = key * 10;
            return (key + ((ulong)level << 32));
            //Item.ItemSort sort = Item.GetItemSort(type);
            //if (sort == Item.ItemSort.ItemsortWeaponSingleHand && Item.GetItemSubType(type) != 421)
            //{
            //    key = type / 100000 * 100000 + type % 1000 + 44000 - type % 10;
            //}
            //else if (sort == Item.ItemSort.ItemsortWeaponDoubleHand && !Item.IsBow(type))
            //{
            //    key = type / 100000 * 100000 + type % 1000 + 55000 - type % 10;
            //}
            //else
            //{
            //    key = type / 1000 * 1000 + (type % 1000 - type % 10);
            //}

            //return (key + ((ulong)level << 32));
        }
    }
}
