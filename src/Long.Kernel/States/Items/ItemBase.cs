using Long.Database.Entities;
using Long.Kernel.Managers;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.User;
using Long.Shared.Helpers;
using Newtonsoft.Json;
using System.Text;

namespace Long.Kernel.States.Items
{
    public abstract class ItemBase
    {
        protected static readonly ILogger logger = Log.ForContext<ItemBase>();
        protected static readonly ILogger itemExpireLog = Logger.CreateLogger("item_expire");
        protected static readonly ILogger itemActivateLog = Logger.CreateLogger("item_activate");
        protected static readonly ILogger repairItemLogger = Logger.CreateLogger("repair_item");
        protected static readonly ILogger deleteItemLogger = Logger.CreateLogger("delete_item");
        protected static readonly ILogger deleteChkSumItemLogger = Logger.CreateConsoleLogger("delete_item_chksum");

        protected Character user;

        protected DbItem item;
        protected DbItemtype itemType;
        protected DbItemAddition itemAddition;

        protected DbGrade grade;
        protected DbEudemon eudemon;
        protected DbMonstertype monsterType;

        #region Constant's

        public const uint SPECIAL_FLAG_UNLOCKING = 0x2;
        public const uint SPECIAL_FLAG_LOCKED = 0x1;

        /// <summary>
        /// Item is owned by the holder. Cannot be traded or dropped.
        /// </summary>
        public const int ITEM_MONOPOLY_MASK = 1;
        /// <summary>
        /// Item cannot be stored.
        /// </summary>
        public const int ITEM_STORAGE_MASK = 2;
        /// <summary>
        /// Item cannot be dropped.
        /// </summary>
        public const int ITEM_DROP_HINT_MASK = 4;
        /// <summary>
        /// Item cannot be sold.
        /// </summary>
        public const int ITEM_SELL_HINT_MASK = 8;
        public const int ITEM_NEVER_DROP_WHEN_DEAD_MASK = 16;
        public const int ITEM_SELL_DISABLE_MASK = 32;
        public const int ITEM_STATUS_NONE = 0;
        public const int ITEM_STATUS_NOT_IDENT = 1;
        public const int ITEM_STATUS_CANNOT_REPAIR = 2;
        public const int ITEM_STATUS_NEVER_DAMAGE = 4;
        public const int ITEM_STATUS_MAGIC_ADD = 8;

        public const int WARGHOSTVALUE_LIMIT = 1024;
        public const int MAX_LEVEL_WARLEVEL = 9;

        //////////////////////////////////////////////////////////////////////
        public const int ITEMREPAIR_PERCENT = 200;             // ÐÞÀí·ÑÎªÔ­¼ÛµÄ200%
        public const int ITEMIDENT_PERCENT = 10;               // ¼ø¶¨·Ñ10%
        public const int ERROR_WEIGHT = 123456789;     // ´íÎóµÄÖØÁ¿
                                                       //---Æ·ÖÊ---begin
        public const int QUALITY_ZERO = 0;             // ÆÕÍ¨Æ·
        public const int QUALITY_ONE = 1;              // Á¼Æ·
        public const int QUALITY_TWO = 2;              // ÉÏÆ·
        public const int QUALITY_THREE = 3;                // ¾«Æ·
        public const int QUALITY_FOUR = 4;             // ¼«Æ·
                                                       //---Æ·ÖÊ---end
        public const int EUDEMON_EVOLVE_LEVEL1 = 20;               // »ÃÊÞµÚÒ»´Î½ø»¯µÈ¼¶
        public const int EUDEMON_EVOLVE_LEVEL2 = 40;               // »ÃÊÞµÚ¶þ´Î½ø»¯µÈ¼¶
        public const int EUDEMON_EVOLVE_MAXTYPE = 2;               // »ÃÊÞµÚÒ»´Î½ø»¯µÄ×î´óÀàÐÍ±àºÅ

        //////////////////////////////////////////////////////////////////////
        //
        public const int EUDEMON_GROUP_SAINT = 1;          // Ê¥ÊÞ
        public const int EUDEMON_GROUP_EVIL = 2;           // Ä§ÊÞ

        public const int MIN_DIVINE_ID = 1;                    // ×îÐ¡ÉñÊ¶±àºÅ
        public const int MAX_DIVINE_ID = 8;                    // ×î´óÉñÊ¶±àºÅ
        public const int EUDEMON_HATCH_SECS = 1 * 60 * 60;         // »ÃÊÞ·õ»¯ÐèÒª24Ð¡Ê±
        public const int EUDEMON_STORAGE_HATCH_SECS = 12 * 60 * 60;         // »ÃÊÞ·õ»¯ÐèÒª24Ð¡Ê±

        public const int ADD_POTENTIAL_RELATIONSHIP = 6;

        public const uint GEM_EUDEMON_REBORN = 1033020;   // »ÃÊÞ¸´»îÓÃ±¦Ê¯
        public const uint GEM_EUDEMON_ENHANCE = 1033030;    // »ÃÊÞÇ¿»¯ÓÃ±¦Ê¯
        public const uint GHOST_GEM_AVOID_DEATH = 1033040;        // ÒÆ»ê±¦Ê¯¡ª¡ªÃâËÀ£¬»Ö¸´È«²¿ÉúÃü
        public const uint GHOST_GEM_AMULET = 1036000;     // »¤Éí±¦Ê¯¡ª¡ªÃâËÀ£¬»Ö¸´1µãÉúÃü
        public const uint GHOST_GEM_SCAPEGOAT = 1036030;      // ÌæÉí±¦Ê¯¡ª¡ª±»É±ËÀ²»µôÎïÆ·ºÍ¾­Ñé
        public const uint GHOST_GEM_REBORN = 1036020;     // ¸´»î±¦Ê¯¡ª¡ªËÀÍö20Ãëºó¸´»î
        public const uint GHOST_GEM_AVOID_STEAL = 1034020;      // ·´ÍµµÁ±¦Ê¯¡ª¡ª±ÜÃâ±»ÍµµÁ

        public const int ITEMTYPE_SCROLL = 20000;//¾íÖá
        public const int ITEMTYPE_SCROLL_SPECIAL = 20000;//ÌØÊâ¾íÖá£¬Èç£º»Ø³Ç¾í¡¢×£¸£¾íÖáµÈ
        public const int ITEMTYPE_SCROLL_MSKILL = 21000;//Ä§·¨Ê¦¼¼ÄÜ¾íÖá
        public const int ITEMTYPE_SCROLL_SSKILL = 22000;//Õ½Ê¿¼¼ÄÜ¾íÖá
        public const int ITEMTYPE_SCROLL_BSKILL = 23000;//¹­¼ýÊÖ¼¼ÄÜ¾íÖá <== ¸ÄÎªÒìÄÜÕß

        public const int ITEMTYPE_GHOSTGEM = 30000;        // Ä§»ê±¦Ê¯
        public const int ITEMTYPE_GHOSTGEM_ACTIVE_ATK = 31000; // ×´Ì¬¹¥»÷Àà
        public const int ITEMTYPE_GHOSTGEM_PASSIVE_ATK = 32000;    // ×´Ì¬±»¶¯Àà
        public const int ITEMTYPE_GHOSTGEM_EUDEMON = 33000;    // »ÃÊÞÀà
        public const int ITEMTYPE_GHOSTGEM_RELEASE = 34000;    // ½â³ýÀà
        public const int ITEMTYPE_GHOSTGEM_TRACE = 35000;  // ×·É±Àà
        public const int ITEMTYPE_GHOSTGEM_PROTECTIVE = 36000; // »¤ÉíÀà
        public const int ITEMTYPE_GHOSTGEM_SPECIAL = 37000;    // ÌØÊâÀà
        public const int ITEMTYPE_GHOSTGEM_EMBEDEQUIP = 38000; // ÓÃÓÚÇ¶Èë×°±¸µÄ±¦Ê¯---jinggy
                                                               //---jinggy 2004-11-9 --begin---Ê¥Õ½ÖýÔìÏµÍ³ÓÃµ½µÄ
        public const int ITEMTYPE_GHOSTGEM_FORQUALITY = 1037160; //Ìá¸ß×°±¸Æ·ÖÊµÄ±¦Ê¯ £¨Áé»ê¾§Ê¯£©
        public const int ITEMTYPE_GHOSTGEM_FORGHOSTLEVEL = 1037150;//Éý¼¶×°±¸Ä§»êµÈ¼¶µÄ±¦Ê¯
        public const int ITEMTYPE_GHOSTGEM_UPGRADE_EQUIPLEVEL = 1037170;//»ÃÄ§¾§Ê¯£¨»ÃÄ§¾§Ê¯type£º1037170£©À´Éý¼¶ÎäÆ÷»ò×°±¸µÄµÈ¼¶¡£
                                                                        //---jinggy 2004-11-9 --end---Ê¥Õ½ÖýÔìÏµÍ³ÓÃµ½µÄ
        public const int ITEMTYPE_NOT_DIRECTUSE = 40000;//²»¿ÉÒÔË«»÷Ê¹ÓÃµÄ -- Ä¾²Ä¡¢¿óÊ¯µÈ×ÊÔ´Àà
        public const uint ITEMTYPE_LOTTERY_TICKET = 1020157;

        //===================================
        public const int ITEMTYPE_SPECIAL_USE = 50000; // ´óÓÚ´Ë±àºÅÒÔÉÏµÄÎªÆäËûÓÃÍ¾µÄÎïÆ·

        public const int ITEMTYPE_SPRITE = 50000;// ¾«Áé
        public const int ITEMTYPE_SPRITE_PATK = 50000;// »ðÔªËØ¾«Áé--Ôö¼ÓÎïÀí¹¥»÷Á¦
        public const int ITEMTYPE_SPRITE_PDEF = 51000;// ÍÁÔªËØ¾«Áé--Ôö¼ÓÎïÀí·ÀÓùÁ¦
        public const int ITEMTYPE_SPRITE_MATK = 52000;// ·çÔªËØ¾«Áé--Ôö¼ÓÄ§·¨¹¥»÷Á¦
        public const int ITEMTYPE_SPRITE_MDEF = 53000;// Ë®ÔªËØ¾«Áé--Ôö¼ÓÄ§·¨·ÀÓùÁ¦
        public const int ITEMTYPE_SPRITE_SOUL = 54000;// °µÔªËØ¾«Áé--Ôö¼Ó¾«Éñ

        public const int ITEMTYPE_SPECIAL = 60000;//ÌØÊâÎïÆ·
        public const int ITEMTYPE_SPECIAL_VALUABLES = 60000;//ÌØÊâ¹óÖØÎïÆ·¡£ÈçÒì´ÎÔª´ü
        public const int ITEMTYPE_SPECIAL_UNREPAIRABLE = 61000;//²»¿ÉÐÞ¸´µÄ¹óÖØÎïÆ·

        // »ÃÊÞ -- zlong 2004-02-03
        public const int ITEMTYPE_EUDEMON = 70000; //»ÃÊÞ
        public const int ITEMTYPE_EUDEMON_SPEED = 71000;   // ËÙ¶ÈÐÍ
        public const int ITEMTYPE_EUDEMON_PATK = 72000;    // ¹¥»÷ÐÍ
        public const int ITEMTYPE_EUDEMON_DEF = 73000; // ·ÀÓùÐÍ
        public const int ITEMTYPE_EUDEMON_MATK = 74000;    // Ä§·¨¹¥»÷ÐÍ
        public const int ITEMTYPE_EUDEMON_BOMB = 75000;    // ±¬ÆÆÐÍ
        public const int ITEMTYPE_EUDEMON_PROTECTIVE = 76000;  // ±£»¤ÐÍ
        public const int ITEMTYPE_EUDEMON_ATTACH = 77000;  // ¸½ÉíÐÍ
        public const int ITEMTYPE_EUDEMON_VARIATIONAL = 78000; // ±äÒìÐÍ

        public const int ITEMTYPE_EUDEMON_SORT = 1000000; //»ÃÊÞ

        public const int ITEMTYPE_EUDEMON_EGG = 80000;	// »ÃÊÞµ°

        public const int MAX_LEVEL_EQUIP1 = 9;
        public const int MAX_LEVEL_EQUIP2 = 22;
        public const int MAX_LEVEL_QUALITYANDADDITION = 9;

        //
        public const uint TYPE_DRAGONBALL = 1088000;
        public const uint TYPE_METEOR = 1088001;
        public const uint TYPE_METEORTEAR = 1088002;
        public const uint TYPE_TOUGHDRILL = 1200005;
        public const uint TYPE_STARDRILL = 1200006;
        public const uint TYPE_SUPER_TORTOISE_GEM = 700073;
        //
        public const uint TYPE_DRAGONBALL_SCROLL = 720028; // Amount 10
        public const uint TYPE_METEOR_SCROLL = 720027; // Amount 10
        public const uint TYPE_METEORTEAR_PACK = 723711; // Amount 5
        //
        public const uint TYPE_STONE1 = 730001;
        public const uint TYPE_STONE2 = 730002;
        public const uint TYPE_STONE3 = 730003;
        public const uint TYPE_STONE4 = 730004;
        public const uint TYPE_STONE5 = 730005;
        public const uint TYPE_STONE6 = 730006;
        public const uint TYPE_STONE7 = 730007;
        public const uint TYPE_STONE8 = 730008;

        //
        public const uint TYPE_MOUNT_ID = 300000;

        //
        public const uint TYPE_EXP_BALL = 500001;
        public const uint TYPE_EXP_CRYSTAL = 500002;
        public const uint TYPE_EXP_POTION = 723017;

        public static readonly int[] BowmanArrows =
        {
            1050000, 1050001, 1050002, 1050020, 1050021, 1050022, 1050023, 1050030, 1050031, 1050032, 1050033, 1050040,
            1050041, 1050042, 1050043, 1050050, 1050051, 1050052
        };

        public const uint IRON_ORE = 1072010;
        public const uint COPPER_ORE = 1072020;
        public const uint EUXINITE_ORE = 1072031;
        public const uint SILVER_ORE = 1072040;
        public const uint GOLD_ORE = 1072050;

        public const uint OBLIVION_DEW = 711083;
        public const uint MEMORY_AGATE = 720828;

        public const uint PERMANENT_STONE = 723694;
        public const uint BIGPERMANENT_STONE = 723695;

        public const int LOTTERY_TICKET = 710212;
        public const uint SMALL_LOTTERY_TICKET = 711504;

        public const uint TYPE_JAR = 750000;

        public const uint SASH_SMALL = 1100003;
        public const uint SASH_MEDIUM = 1100006;
        public const uint SASH_LARGE = 1100009;

        public const uint PROTECTION_PILL = 3002029;
        public const uint SUPER_PROTECTION_PILL = 3002030;

        public const uint FREE_TRAINING_PILL = 3002926;
        public const uint FAVORED_TRAINING_PILL = 3003124;
        public const uint SPECIAL_TRAINING_PILL = 3003125;
        public const uint SENIOR_TRAINING_PILL = 3003126;

        public const uint POWER_ERASER = 3005412;

        public static int[][] setAtkModePotential =
        {
            //	ÉúÃüÖ®Éñ	»ìãçËÀÉñ	¹«ÕýÖ®Éñ	ÒõÄ±Ö®Éñ	ÖÇ»ÛÖ®Éñ	Õ½ÕùÖ®Éñ	¸¯°ÜÖ®Éñ	¶ÍÔìÖ®Éñ
          new int[] {0,         0,          0,          0,          0,          0,          0,          0},		// ATK_MODE_NONE
          new int[] { -5,       5,          2,          -5,         -5,         5,          -3,         -2},	// ATK_MODE_ATK
          new int[] { -2,       -2,         5,          5,          5,          -2,         5,          5},		// ATK_MODE_ATKDEF
          new int[] {5,         -5,         -5,         -2,         2,          -5,         -3,         -2},	// ATK_MODE_DEF
        };

        #region Enums

        public enum ItemSort
        {
            ItemsortFinery = 1,
            ItemsortMount = 3,
            ItemsortWeaponSingleHand = 4,
            ItemsortWeaponDoubleHand = 5,
            ItemsortWeaponProfBased = 6,
            ItemsortUsable = 7,
            ItemsortWeaponShield = 9,
            ItemsortExpend = 10,
            ItemsortUsable2 = 10,
            ItemsortUsable3 = 12,
            ItemsortAccessory = 3,
            ItemsortTwohandAccessory = 35,
            ItemsortOnehandAccessory = 36,
            ItemsortBowAccessory = 37,
            ItemsortShieldAccessory = 38
        }

        public enum ItemEffect : ushort
        {
            None = 0,
            Poison = 200,
            Life = 201,
            Mana = 202,
            Shield = 203,
            Horse = 100
        }

        public enum EudemonAtkMode
        {
            None = 0,
            Atk = 1,
            AtkDef = 2,
            Def = 3,

            Limit,
        }


        public enum EudemonAttribType
        {
            None = 0,
            Earth,
            Water,
            Fire,
            Air,
            Thunder
        }

        public enum EudemonGrowthAttrib
        {
            First = LifeGrow,
            LifeGrow = 0,
            PhyAtkGrowMin,
            PhyAtkGrowMax,
            MagicAtkGrowMin,
            MagicAtkGrowMax,
            PhyDefGrow,
            MagicDefGrow,
            Luck,
            InitPhyMin,
            InitPhyMax,
            InitMagicMin,
            InitMagicMax,
            InitDef,
            InitMDef,
            InitLife,
            Last = InitLife + 1,
        }

        public enum SocketGem : byte
        {
            //1038000
            NoSocket = 0,

            NormalEarthProf = 1,
            RefinedEarthProf,
            SuperEarthProf,

            NormaWaterProf,
            RefinedWaterProf,
            SuperWaterProf,

            NormalFireProf,
            RefinedFireProf,
            SuperFireProf,

            NormalAirProf,
            RefinedAirProf,
            SuperAirProf,

            NormalAmber = 16,
            RefinedAmber,
            SuperAmber,

            NormalCitrine,
            RefinedCitrine,
            SuperCitrine,

            NormalBeryl,
            RefinedBeryl,
            SuperBeryl,

            NormalAmethyst,
            RefinedAmethyst,
            SuperAmethyst,

            NormalSapphire,
            RefinedSapphire,
            SuperSapphire,

            EmptySocket = 255
        }

        public enum ItemPosition : ushort
        {
            EquipmentBegin = 1,
            Helmet = 1,
            Necklace = 2,
            Armor = 3,
            Weapon = 4,
            Ring = 7,
            Shoes = 8,
            Sprite = 12,
            Horn = 13,
            Crown = 14,
            Rune = 15,

#if DEBUG_PALADIN
            EquipmentEnd = Sprite,
#else
            EquipmentEnd = Rune,
#endif

            PackBegin = 50,
            Inventory = 50,
            GhostGemPack = 51,
            EudemonGGPack = 52,
            EudemonPack = 53,
            PackEnd = 54,
            BloodSoul = 60,//1072200 eudemonid
            PackLimit = 70,

            UserLimit = 199,

            Storage = 201,
            Trunk = 202,
            Chest = 203,
            PlayerTask = 204,
            EudemonBrooder = 205,
            EudemonStorage = 206,
            AuctionStorage = 207,
            AuctionSysStorage = 208,
            AuctionEudStorage = 209, //probably
            Auction = 210,
            EudemonExtraBrooder = 215,

            Detained = 250,
            Floor = 254,
            None = 255,
        }

        public enum ItemColor : byte
        {
            None,
            Black = 2,
            Orange = 3,
            LightBlue = 4,
            Red = 5,
            Blue = 6,
            Yellow = 7,
            Purple = 8,
            White = 9
        }

        public enum ChangeOwnerType : byte
        {
            DropItem,
            PickupItem,
            TradeItem,
            CreateItem,
            DeleteItem,
            ItemUsage,
            DeleteDroppedItem,
            InvalidItemType,
            BoothSale,
            ClearInventory,
            DetainEquipment
        }

        public enum IdentType
        {
            None = 0,      // ÎÞ
            NotIdent = 1,     // Î´¼ø¶¨
            CannotRepair = 2,     // ²»¿ÉÐÞ¸´
            NeverDamage = 4,      // ÓÀ²»Ä¥Ëð
        };

        public enum TargetMask
        {
            None = 0x0000,           // Ö»ÄÜ¶Ô×Ô¼ºÊ¹ÓÃ ¡ª¡ª ¼æÈÝ¾ÉÏµÍ³Êý¾Ý

            // Ä¿±êÀàÐÍ
            TargetUser = 0x0001,           // ¿ÉÒÔ¶ÔÍæ¼ÒÊ¹ÓÃ
            TargetMonster = 0x0002,            // ¿ÉÒÔ¶Ô¹ÖÎïÊ¹ÓÃ
            TargetEudemon = 0x0004,            // ¿ÉÒÔ¶Ô»ÃÊÞÊ¹ÓÃ

            // ÏÞÖÆÌõ¼þ
            TargetSelf = 0x0010,           // ÏÞÖÆÖ»ÄÜ¶Ô×Ô¼º»òÊôÓÚ×Ô¼ºµÄÄ¿±ê
            TargetOthers = 0x0020,         // ÏÞÖÆÖ»ÄÜ¶ÔÆäËûÍæ¼Ò»òÊôÓÚÆäËûÍæ¼ÒµÄÄ¿±ê
            TargetBody = 0x0040,           // ÏÞÖÆÖ»ÄÜ¶ÔÊ¬ÌåÊ¹ÓÃ

            // ÆäËûÀàÐÍ¼ì²é
            TargetChkPkMode = 0x0100,         // ÐèÒª¼ì²épkÄ£Ê½
            TargetForbidden = 0x0200,          // ½ûÖ¹¶ÔÈÎºÎÄ¿±êÊ¹ÓÃ
        };

#endregion

#endregion

        public DbItemtype Itemtype => itemType;

        public DbMonstertype MonsterType => monsterType;

        public ItemBase(Character user)
        {
            this.user = user;
        }

        public virtual Task<bool> CreateAsync(DbItemtype type, ItemPosition position = ItemPosition.Inventory, bool monopoly = false)
        {
            logger.Error($"ItemBase->CreateAsync(): Method not creaded!");
            return Task.FromResult(false);
        }

        public virtual Task<bool> CreateAsync(DbItem dbEudemon)
        {
            logger.Error($"ItemBase->CreateAsync(): Method not creaded!");
            return Task.FromResult(false);
        }

        public virtual Task<bool> CreateAsync(DbEudemon dbEudemon)
        {
            logger.Error($"ItemBase->CreateAsync(): Method not creaded!");
            return Task.FromResult(false);
        }

        protected virtual Task InitializeAsync()
        {
            // on eudemons we probably will not need it.
            return Task.CompletedTask;
        }

        #region ChangeData

        public void ChangeOwner(Character newOwner)
        {
            PlayerIdentity = newOwner?.Identity ?? 0;
            user = newOwner;
        }

        public virtual Task<bool> ChangeTypeAsync(uint newType)
        {
            logger.Error($"ItemBase->ChangeTypeAsync(): Method not created!");
            return Task.FromResult(false);
        }

        public virtual bool ChangeAddition(int level = -1)
        {
            logger.Error($"ItemBase->ChangeAddition(): Method not created!");
            return false;
        }

        public virtual bool DecAddition()
        {
            logger.Error($"ItemBase->DecAddition(): Method not created!");
            return false;
        }

        public virtual Task<bool> ConvertToCrystalAsync()
        {
            logger.Error($"ItemBase->ConvertToCrystalAsync(): Method not created!");
            return Task.FromResult(false);
        }

        #endregion

        #region Update and Upgrade

        public bool GetUpLevelChance(out int chance, out int nextId)
        {
            nextId = 0;
            chance = 0;
            DbItemtype info = NextItemLevel((int)Type);
            if (info == null)
            {
                return false;
            }

            nextId = (int)info.Type;
            chance = 100;
            if (IsHelmet() || IsArmor() || IsNeck() || IsRing() || IsShoes())
            {
                int nLevel = GetLevel(info.Type);
                if (nLevel >= 0 && nLevel < 2) { chance = 100; }
                else if (nLevel >= 2 && nLevel < 4) { chance = 35; }
                else if (nLevel >= 4 && nLevel < 6) { chance = 20; }
                else if (nLevel >= 6 && nLevel < 7) { chance = 10; }
                else if (nLevel >= 7 && nLevel < 8) { chance = 7; }
                else if (nLevel >= 8 && !IsShoes()) { chance = 4; }
            }
            else
            {
                int nLevel = GetLevel(info.Type);
                if (nLevel >= 0 && nLevel < 4) { chance = 100; }
                else if (nLevel >= 4 && nLevel < 7) { chance = 35; }
                else if (nLevel >= 7 && nLevel < 10) { chance = 20; }
                else if (nLevel >= 10 && nLevel < 13) { chance = 10; }
                else if (nLevel >= 13 && nLevel < 16) { chance = 7; }
                else if (nLevel >= 16 && nLevel < 19) { chance = 4; }
                else if (nLevel >= 19 && nLevel < 24) { chance = 2; }
            }

            return true;
        }

        public DbItemtype NextItemLevel()
        {
            return NextItemLevel((int)Type);
        }

        public DbItemtype NextItemLevel(Int32 id)
        {
            // By CptSky
            Int32 nextId = id;
            var sort = (byte)(id / 100000);
            var type = (byte)(id / 10000);
            var subType = (short)(id / 1000);
            if (sort == 1) //!Weapon
            {
                if (type == 12 && (subType == 120 || subType == 121) || type == 15 || type == 16
                ) //Necklace || Ring || Boots
                {
                    var level = (byte)((id % 1000 - id % 10) / 10);
                    if (type == 12 && level < 8 || type == 15 && subType != 152 && level > 0 && level < 21 ||
                        type == 15 && subType == 152 && level >= 4 && level < 22 ||
                        type == 16 && level > 0 && level < 21)
                    {
                        //Check if it's still the same type of item...
                        if ((Int16)((nextId + 20) / 1000) == subType)
                        {
                            nextId += 20;
                        }
                    }
                    else if (type == 12 && level == 8 || type == 12 && level >= 21 ||
                             type == 15 && subType != 152 && level == 0
                             || type == 15 && subType != 152 && level >= 21 ||
                             type == 15 && subType == 152 && level >= 22 || type == 16 && level >= 21)
                    {
                        //Check if it's still the same type of item...
                        if ((short)((nextId + 10) / 1000) == subType)
                        {
                            nextId += 10;
                        }
                    }
                    else if (type == 12 && level >= 9 && level < 21 || type == 15 && subType == 152 && level == 1)
                    {
                        //Check if it's still the same type of item...
                        if ((short)((nextId + 30) / 1000) == subType)
                        {
                            nextId += 30;
                        }
                    }
                }
                else
                {
                    var Quality = (byte)(id % 10);
                    if (type == 11 || type == 14 || type == 13 || subType == 123) //Head || Armor
                    {
                        var level = (byte)((id % 100 - id % 10) / 10);

                        //Check if it's still the same type of item...
                        if ((short)((nextId + 10) / 1000) == subType)
                        {
                            nextId += 10;
                        }
                    }
                }
            }
            else if (sort == 4 || sort == 5 || sort == 6) //Weapon
            {
                //Check if it's still the same type of item...
                if ((short)((nextId + 10) / 1000) == subType)
                {
                    nextId += 10;
                }

                //Invalid Backsword ID
                if (nextId / 10 == 42103 || nextId / 10 == 42105 || nextId / 10 == 42109 || nextId / 10 == 42111)
                {
                    nextId += 10;
                }
            }
            else if (sort == 9)
            {
                var Level = (byte)((id % 100 - id % 10) / 10);
                if (Level != 30) //!Max...
                {
                    //Check if it's still the same type of item...
                    if ((short)((nextId + 10) / 1000) == subType)
                    {
                        nextId += 10;
                    }
                }
            }

            return ItemManager.GetItemtype((uint)nextId);
        }

        public uint ChkUpEqQuality(uint type)
        {
            if (type == TYPE_MOUNT_ID)
            {
                return 0;
            }

            uint nQuality = type % 10;
            if (nQuality > 4)
            {
                return 0;
            }

            nQuality = Math.Min(4, Math.Max(1, ++nQuality));
            type = type - type % 10 + nQuality;
            return ItemManager.GetItemtype(type)?.Type ?? 0;
        }

        public bool GetUpEpQualityInfo(out double nChance, out uint idNewType)
        {
            nChance = 0;
            idNewType = ChkUpEqQuality(Type);
            switch (Type % 10)
            {
                case 0: nChance = 30; break;
                case 1: nChance = 12; break;
                case 2: nChance = 6; break;
                case 3: nChance = 4; break;
                default: nChance = 0; break;
            }

            if (nChance == 0)
            {
                return false;
            }

            DbItemtype itemtype = ItemManager.GetItemtype(idNewType);
            if (itemtype == null)
            {
                return false;
            }

            return true;
        }

        public uint GetFirstId()
        {
            uint firstId = Type;
            var sort = (byte)(Type / 100000);
            var type = (byte)(Type / 10000);
            var subType = (short)(Type / 1000);
            if (Type == 150000 || Type == 150310 || Type == 150320 || Type == 410301 || Type == 421301 || Type == 500301
                || Type == 601301 || Type == 610301)
            {
                return Type;
            }

            if (Type >= 120310 && Type <= 120319)
            {
                return Type;
            }

            if (sort == 1) //!Weapon
            {
                if (IsNeck()) //Necklace
                {
                    firstId = Type - Type % 1000 + Type % 10;
                }
                else if (IsRing()|| IsShoes()) //Ring || Boots
                {
                    firstId = Type - Type % 1000 + 10 + Type % 10;
                }
                else if (IsHelmet()) //Head
                {
                    firstId = Type - Type % 1000 + Type % 10;
                }
                else if (IsArmor()) //Armor
                {
                    firstId = Type - Type % 1000 + Type % 10;
                }
            }
            else if (sort == 4 || sort == 5 || sort == 6) //Weapon
            {
                firstId = Type - Type % 1000 + 20 + Type % 10;
            }
            else if (sort == 9)
            {
                firstId = Type - Type % 1000 + Type % 10;
            }

            return ItemManager.GetItemtype(firstId)?.Type ?? Type;
        }

        public int GetUpQualityGemAmount()
        {
            if (!GetUpEpQualityInfo(out var nChance, out _))
            {
                return 0;
            }

            return (int)((100 / nChance + 1) * 12 / 10);
        }

        public int GetUpgradeGemAmount()
        {
            if (!GetUpLevelChance(out var nChance, out _))
            {
                return 0;
            }

            return (int)((int)(100d / nChance + 1) * 12d / 10d);
        }

        public async Task<bool> DegradeItemAsync(bool bCheckDura = true)
        {
            if (!IsEquipment())
            {
                return false;
            }

            if (bCheckDura)
            {
                if (Amount / 100 < AmountLimit / 100)
                {
                    await user.SendAsync(StrItemErrRepairItem);
                    return false;
                }
            }

            uint newId = GetFirstId();
            DbItemtype newType = ItemManager.GetItemtype(newId);
            if (newType == null || newType.Type == Type)
            {
                return false;
            }

            return await ChangeTypeAsync(newType.Type);
        }

        public async Task<bool> UpItemQualityAsync(uint gemType)
        {
            if (Amount / 100 < AmountLimit / 100)
            {
                await user.SendAsync(StrItemErrRepairItem);
                return false;
            }

            if (!GetUpEpQualityInfo(out var nChance, out var newId))
            {
                await user.SendAsync(StrItemErrMaxQuality);
                return false;
            }

            DbItemtype newType = ItemManager.GetItemtype(newId);
            if (newType == null)
            {
                await user.SendAsync(StrItemErrMaxLevel);
                return false;
            }

            if (gemType != 1037169 && !(await NextAsync(100) < nChance))
            {
                await user.SendAsync(StrUpgradeQualityFail);
                return false;
            }

            if (gemType == 1037169 && await ChanceCalcAsync(0.5d))
            {
                if (SocketOne == SocketGem.NoSocket)
                {
                    SocketOne = SocketGem.EmptySocket;
                }
                else if (SocketTwo == SocketGem.NoSocket)
                {
                    SocketTwo = SocketGem.EmptySocket;
                }
            }

            return await ChangeTypeAsync(newType.Type);
        }

        /// <summary>
        /// This method will upgrade an equipment level using meteors.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpEquipmentLevelAsync(uint gemType)
        {
            if (Amount / 100 < AmountLimit / 100)
            {
                await user.SendAsync(StrItemErrRepairItem);
                return false;
            }

            if (!GetUpLevelChance(out var nChance, out var newId))
            {
                await user.SendAsync(StrItemErrMaxLevel);
                return false;
            }

            DbItemtype newType = ItemManager.GetItemtype((uint)newId);
            if (newType == null)
            {
                await user.SendAsync(StrItemErrMaxLevel);
                return false;
            }

            if (gemType != 1037179 && !(await NextAsync(100) < nChance))
            {
                //await user.SendAsync(StrUpgradeQualityFail);
                return false;
            }

            if (gemType == 1037179 && await ChanceCalcAsync(0.5d))
            {
                if (SocketOne == SocketGem.NoSocket)
                {
                    SocketOne = SocketGem.EmptySocket;
                }
                else if (SocketTwo == SocketGem.NoSocket)
                {
                    SocketTwo = SocketGem.EmptySocket;
                }
            }

            AmountLimit = newType.AmountLimit;
            Amount = newType.AmountLimit;
            return await ChangeTypeAsync(newType.Type);
        }

        public async Task<bool> UpUltraEquipmentLevelAsync()
        {
            if (Amount / 100 < AmountLimit / 100)
            {
                await user.SendAsync(StrItemErrRepairItem);
                return false;
            }

            DbItemtype newType = NextItemLevel((int)Type);
            if (newType == null || newType.Type == Type)
            {
                await user.SendAsync(StrItemErrMaxLevel);
                return false;
            }

            if (newType.ReqLevel > user.Level)
            {
                await user.SendAsync(StrItemErrNotEnoughLevel);
                return false;
            }

            return await ChangeTypeAsync(newType.Type);
        }

        #endregion

        #region Requirements

        public virtual int RequiredLevel => itemType?.ReqLevel ?? 0;

        public virtual int RequiredProfession => (int)(itemType?.ReqProfession ?? 0);

        public virtual int RequiredGender => itemType?.ReqSex ?? 0;

        public virtual int RequiredForce => itemType?.ReqForce ?? 0;

        public virtual int RequiredAgility => itemType?.ReqDexterity ?? 0;

        public virtual int RequiredVitality => itemType?.ReqHealth ?? 0;

        public virtual int RequiredSpirit => itemType?.ReqSoul ?? 0;

        #endregion

        #region Attributes

        public virtual uint Identity => 0;

        public virtual uint Type => itemType?.Type ?? 0;

        public virtual string Name { get; set; } = string.Empty;

        public virtual string ForgenName { get; set; } = string.Empty;

        public virtual string FullName
        {
            get
            {
                StringBuilder builder = new();
                switch (Type % 10)
                {
                    case 7: builder.Append("Divino"); break;
                    case 6: builder.Append("Divino"); break;
                    case 5: builder.Append("Divino"); break;
                    case 4: builder.Append("Super"); break;
                    case 3: builder.Append("Elite"); break;
                    case 2: builder.Append("Unico"); break;
                    case 1: builder.Append("Refinado"); break;
                }
                builder.Append(Name);
                if (Plus > 0)
                {
                    builder.Append($"(+{Plus})");
                }

                if (SocketOne != SocketGem.NoSocket)
                {
                    if (SocketThree != SocketGem.NoSocket)
                    {
                        builder.Append(" 3-Socket");
                    }
                    else if (SocketTwo != SocketGem.NoSocket)
                    {
                        builder.Append(" 2-Socket");
                    }
                    else
                    {
                        builder.Append(" 1-Socket");
                    }
                }

                //if (Effect != ItemEffect.None && !IsMount())
                //{
                //    builder.Append($" {Effect}");
                //}

                return builder.ToString();
            }
        }

        public virtual uint OwnerIdentity { get; set; }

        public virtual uint PlayerIdentity { get; set; }

        public virtual ushort Amount { get; set; }

        public virtual ushort AmountLimit { get; set; }

        public virtual ushort AccumulateNum
        {
            get
            {
                //if (IsPileEnable())
                //{
                //    return Amount;
                //}

                return (ushort)Math.Max(1u, Amount);
            }
            set => Amount = value;
        }

        public virtual ushort MaxAccumulateNum => Math.Max(AmountLimit, (ushort)1);

        public virtual byte Ident { get; set; }

        public virtual SocketGem SocketOne { get; set; }

        public virtual SocketGem SocketTwo { get; set; }

        public virtual SocketGem SocketThree { get; set; }

        public virtual ItemPosition Position { get; set; }

        public virtual byte Plus { get; set; }

        public virtual uint Progress { get; set; }

        public virtual ItemEffect Effect { get; set; }

        public virtual ushort Magic1 { get; set; }

        public virtual byte Magic2 { get; set; }

        public virtual uint Data{ get; set; }

        public virtual byte Monopoly { get; set; }

        public virtual byte EarthAttr { get; set; }

        public virtual byte FireAttr { get; set; }

        public virtual byte WaterAttr { get; set; }

        public virtual byte AirAttr { get; set; }

        public virtual byte SpecialAttr { get; set; }

        public virtual int BattlePower { get; }

        public virtual int Life { get; }

        public virtual int Mana => itemType?.Mana ?? 0;

        public virtual int MinAttack { get; }

        public virtual int MaxAttack { get; }

        public virtual int MagicAttackMin { get; }

        public virtual int MagicAttackMax { get; }

        public virtual int Defense { get; }

        public virtual int MagicDefense { get; }

        public virtual int MagicDefenseBonus { get; }

        public virtual int Agility { get; }

        public virtual int Accuracy { get; }

        public virtual int Dodge { get; }

        public virtual uint GemType { get; set; }

        public virtual int AmethystGemEffect { get; }

        public virtual int SapphireGemEffect { get; }

        public virtual int BerylGemEffect { get; }

        public virtual int CitrineGemEffect { get; }

        public virtual int AmberGemEffect { get; }

        #endregion

        #region Ident

        public virtual bool IsUnident() => false;

        public virtual bool IsIdent() => true;

        public virtual Task SetIdentAsync(bool ident = false, bool update = false)
        {
            logger.Error($"ItemBase->SetIdentAsync(): Method not creaded!");
            return Task.CompletedTask;
        }

        #endregion

        #region Syndicate

        public virtual uint SyndicateIdentity { get; set; }

        #endregion

        #region Bound/Gift

        public virtual byte GiftFlag { get; set; }

        public virtual bool IsBound { get; set; }

        public virtual bool IsGift { get; set; }

        #endregion

        #region Locked

        public virtual uint Locked { get; set; }

        public virtual bool HasUnlocked() => false;

        public virtual bool IsUnlocking() => false;

        public virtual bool IsLocked() => false;

        public virtual Task<bool> TryUnlockAsync()
        {
            logger.Error($"ItemBase->TryUnlockAsync(): Method not creaded!");
            return Task.FromResult(false);
        }

        public virtual Task SetLockAsync()
        {
            logger.Error($"ItemBase->SetLockAsync(): Method not creaded!");
            return Task.CompletedTask;
        }

        public virtual Task SetUnlockAsync(uint unlockDate = 0)
        {
            logger.Error($"ItemBase->SetUnlockAsync(): Method not creaded!");
            return Task.CompletedTask;
        }

        public virtual Task DoUnlockAsync()
        {
            logger.Error($"ItemBase->DoUnlockAsync(): Method not creaded!");
            return Task.CompletedTask;
        }

        #endregion

        #region Available/Activation

        public virtual int DeleteTime { get; set; }
        public virtual int AvailableTime { get; set; }
        public virtual int RemainingSeconds => item.DeleteTime != 0 ? item.DeleteTime - UnixTimestamp.Now : 0;
        public virtual bool IsActivable() => DeleteTime == 0 && (AvailableTime != 0 || itemType?.SaveTime != 0);
        public virtual bool HasExpired() => DeleteTime != 0 && UnixTimestamp.Now > DeleteTime;
        public virtual bool HasHatched() => false;

        public virtual Task<bool> ActivateAsync()
        {
            logger.Error($"ItemBase->ActivateAsync(): Method not creaded!");
            return Task.FromResult(false);
        }

        public virtual Task ExpireAsync()
        {
            logger.Error($"ItemBase->ExpireAsync(): Method not creaded!");
            return Task.CompletedTask;
        }

        public virtual Task HatchAsync(MsgPackage.StorageType mode, bool sync = true)
        {
            logger.Error($"ItemBase->HatchAsync(): Method not creaded!");
            return Task.CompletedTask;
        }

        #endregion

        #region WarGhost

        public virtual uint WarGhostExp { get; set; }

        public virtual int WarGhostLevel { get; set; }

        public async Task DecWarGhostLevelAsync(int decLevel)
        {
            if (!IsEquipment())
            {
                return;
            }

            uint exp = item.WarGhostExp;
            if (exp <= WARGHOSTVALUE_LIMIT)
            {
                return;
            }

            exp -= (uint)decLevel;
            WarGhostExp = Math.Max(exp, WARGHOSTVALUE_LIMIT);
            await SaveAsync();
        }

        public async Task<bool> WarGhostLevelResetAsync()
        {
            if (!IsEquipment())
            {
                return false;
            }

            if (WarGhostExp < WARGHOSTVALUE_LIMIT)
            {
                return false;
            }

            WarGhostExp = WARGHOSTVALUE_LIMIT;
            return await SaveAsync();
        }

        public async Task WarGhostLevelUpgradeAsync(Character pUser)
        {
            if (!IsEquipment())
            {
                return;
            }

            if (WarGhostLevel >= MAX_LEVEL_WARLEVEL)
            {
                return;
            }

            WarGhostExp++;
            await SaveAsync();
            await pUser.SendAsync(new MsgItemInfo(this, MsgItemInfo.ItemMode.Update));
        }

        #endregion

        #region RelationShip

        public int GetRelationShip(int divineId)
        {
            if (eudemon == null || (divineId < MIN_DIVINE_ID || divineId > MAX_DIVINE_ID))
            {
                return 0;
            }

            return (int)(eudemon.Relationship / Math.Pow(10.0d, divineId - 1) % 10);
        }

        public async Task<bool> AddRelationShipAsync(uint divineId, int nValue)
        {
            if (eudemon == null || (divineId < MIN_DIVINE_ID || divineId > MAX_DIVINE_ID))
            {
                return false;
            }

            uint dwRelationShip = eudemon.Relationship;
            uint dwPow = (uint)Math.Pow(10.0d, (int)divineId - 1);
            int nRelationShip = (int)(dwRelationShip / dwPow % 10);
            nRelationShip = Math.Min(9, Math.Max(0, nRelationShip + nValue)); // ¹ØÏµÈ¡Öµ·¶Î§Îª0 ~ 9
            dwRelationShip = (uint)((dwRelationShip / (dwPow * 10) * dwPow * 10) + nRelationShip * dwPow + dwRelationShip % dwPow);
            eudemon.Relationship = dwRelationShip;
            return await SaveAsync();
        }

        #endregion

        #region Durability

        public int GetRecoverDurCost()
        {
            if (Amount > 0 && Amount < AmountLimit)
            {
                var price = (int)itemType.Price;
                double qualityMultiplier = 0;
                switch (Type % 10)
                {
                    case 4:
                        qualityMultiplier = 1.125;
                        break;
                    case 3:
                        qualityMultiplier = 0.975;
                        break;
                    case 2:
                        qualityMultiplier = 0.9;
                        break;
                    case 1:
                        qualityMultiplier = 0.825;
                        break;
                    default:
                        qualityMultiplier = 0.75;
                        break;
                }

                return (int)Math.Ceiling(price * ((AmountLimit - Amount) / (float)AmountLimit) * qualityMultiplier);
            }

            return 0;
        }

        public virtual Task<bool> RecoverDurabilityAsync()
        {
            logger.Error($"ItemBase->RecoverDurabilityAsync(): Method not creaded!");
            return Task.FromResult(false);
        }

        public virtual Task RepairItemAsync()
        {
            logger.Error($"ItemBase->RepairItemAsync(): Method not creaded!");
            return Task.CompletedTask;
        }

        #endregion

        #region Purchase/Sell

        public int GetSellPrice()
        {
            if (IsBroken() || IsArrowSort() || IsGift || IsLocked())
            {
                return 0;
            }

            int price = (int)(itemType?.Price ?? 0) / 3 * Amount / AmountLimit;
            return price;
        }

        #endregion

        #region Eudemon

        public virtual uint DivineId { get; }

        public virtual uint MonsterTypeId { get; }
        public virtual byte EudemonLevel { get; set; }
        public virtual byte EudemonGodLevel { get; set; }
        public virtual byte EudemonGodStep { get; set; }

        public virtual ulong EudemonExperience { get; set; }
        public virtual ulong EudemonGodExperience { get; set; }

        public virtual int EudemonStarLevel { get; set; }

        public virtual uint EudemonLife { get; set; }
        public virtual uint EudemonMaxLife { get; set; }

        public virtual byte EudemonMountFlag { get; }
        public virtual uint EudemonOfficialPos { get; set; }
        public virtual byte EudemonOfficialIndex { get; set; }
        public virtual byte GetOfficialPos(byte index) { return 0; }

        public virtual Task SendEudemonAttribAsync(Character target = null)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Checks

        public int GetLevel() => GetLevel(Type);

        public ItemSort GetItemSort() => GetItemSort(Type);

        public int GetItemtype() => GetItemtype(Type);

        public int GetItemSubType() => GetItemSubType(Type);

        public int GetQuality() => GetQuality(Type);

        public ItemPosition GetPosition()
        {
            if (IsHelmet())
            {
                return ItemPosition.Helmet;
            }

            if (IsNeck())
            {
                return ItemPosition.Necklace;
            }

            if (IsRing())
            {
                return ItemPosition.Ring;
            }

            if (IsWeapon())
            {
                return ItemPosition.Weapon;
            }

            if (IsArmor())
            {
                return ItemPosition.Armor;
            }

            if (IsShoes())
            {
                return ItemPosition.Shoes;
            }

            if (IsGarment())
            {
                return ItemPosition.Sprite;
            }

            if (IsHorn())
            {
                return ItemPosition.Horn;
            }

            if (IsCrown())
            {
                return ItemPosition.Crown;
            }

            if (IsRune())
            {
                return ItemPosition.Rune;
            }

            return ItemPosition.Inventory;
        }

        public bool IsSuspicious() => false;
        public bool IsMonopoly() => (itemType.Monopoly & ITEM_MONOPOLY_MASK) != 0;
        public bool IsNeverDropWhenDead() => (itemType.Monopoly & ITEM_NEVER_DROP_WHEN_DEAD_MASK) != 0 || IsBound || IsMonopoly() || Plus > 5 || IsLocked();
        public bool IsDisappearWhenDropped() => IsMonopoly() || IsBound || IsGift;
        public bool CanBeDropped()=> !IsMonopoly() && !IsLocked() && !IsSuspicious();// && BattlePower < 8;
        public bool CanBeStored() => (itemType.Monopoly & ITEM_STORAGE_MASK) == 0;
        public bool IsHoldEnable() => IsWeapon();
        public bool IsGem() => IsGem(Type);
        public bool IsCountable() => MaxAccumulateNum > 1;
        public bool IsPileEnable() => IsExpend() && AmountLimit > 1;
        public bool IsMedicine() => IsMedicine(Type);
        public bool IsExpend() => IsExpend(Type);
        public bool IsHelmet() => IsHelmet(Type);
        public bool IsNeck() => IsNeck(Type);
        public bool IsRing() => IsRing(Type);
        public bool IsWeapon() => IsWeapon(Type);
        public bool IsArmor() => IsArmor(Type);
        public bool IsShoes() => IsShoes(Type);
        public bool IsEquipEnable() => IsEquipment() || IsGarment();
        public bool IsGarment() => IsGarment(Type);
        public bool IsHorn() => IsHorn(Type);
        public bool IsCrown() => IsCrown(Type);
        public bool IsRune() => IsRune(Type);
        public bool IsTalisman() => IsCrown() || IsRune() || IsHorn();
        public bool IsEquipment() => IsEquipment(Type);
        public bool IsWeaponOneHand() => IsWeaponOneHand(Type);
        public bool IsArrowSort() => IsArrowSort(Type);
        public bool IsGhostGem() { return IsGhostGem(Type); }
        public bool IsEudemon() { return IsEudemon(Type); }
        public bool IsEudemonEgg() { return IsEudemonEgg(Type); }
        public bool IsEvilEudemon() { return (IsEudemon() && (Type % 10) == EUDEMON_GROUP_EVIL); }
        public bool IsBroken() => !IsEudemon() && Amount == 0;
        public bool IsIspell() => IsSpell(Type);
        public bool IsNeedIdent() => IsNeedIdent(Ident);

        public bool IsEvolveEnable()
        {
            return ((Type % 10 == 0)
                    && (EudemonLevel >= EUDEMON_EVOLVE_LEVEL1)
                    && IsAliveEudemon());
        }

        public bool IsEvolve2Enable()
        {
            if (Type % 10 == 0)
            {
                return false;
            }

            return ((Type % 10 == 1)
                    && (EudemonLevel >= EUDEMON_EVOLVE_LEVEL2)
                    && IsAliveEudemon());
        }

        public bool IsAliveEudemon() { return (IsEudemon() && EudemonLife > 0); }

        #endregion

        #region Static Members
        public static uint HideTypeUnident(uint idType) { if (IsEquipment(idType)) { return ((idType / 10) * 10) % 10000000; } if (IsSpell(idType)) { return ((idType / 100) * 100) % 10000000; } return idType; }
        public static uint HideTypeQuality(uint idType) { if (IsEquipment(idType)) { return ((idType / 10) * 10) % 10000000; } return idType; }
        public static bool IsSpell(uint nType) { return IsExpend(nType) && GetItemtype(nType) == ITEMTYPE_SCROLL; }
        public static bool IsNeedIdent(int nIdent) { return (nIdent & ITEM_STATUS_NOT_IDENT) != 0; }

        public static ItemPosition GetPosition(uint type)
        {
            if (IsHelmet(type))
            {
                return ItemPosition.Helmet;
            }

            if (IsNeck(type))
            {
                return ItemPosition.Necklace;
            }

            if (IsRing(type))
            {
                return ItemPosition.Ring;
            }

            if (IsWeapon(type))
            {
                return ItemPosition.Weapon;
            }

            if (IsArmor(type))
            {
                return ItemPosition.Armor;
            }

            if (IsShoes(type))
            {
                return ItemPosition.Shoes;
            }

            if (IsGarment(type))
            {
                return ItemPosition.Sprite;
            }

            if (IsHorn(type))
            {
                return ItemPosition.Horn;
            }

            if (IsCrown(type))
            {
                return ItemPosition.Crown;
            }

            if (IsRune(type))
            {
                return ItemPosition.Rune;
            }

            return ItemPosition.Inventory;
        }

        public static int GetLevel(uint type)
        {
            return (int)type % 1000 / 10;
        }

        public static bool IsGem(uint type)
        {
            return type / 1000 == 1038;
        }

        public static ItemSort GetItemSort(uint type)
        {
            return (ItemSort)(type % 10000000 / 100000);
        }

        public static int GetItemtype(uint type)
        {
            if (GetItemSort(type) == ItemSort.ItemsortWeaponSingleHand
                || GetItemSort(type) == ItemSort.ItemsortWeaponDoubleHand)
            {
                return (int)(type % 100000 / 1000 * 1000);
            }

            return (int)(type % 100000 / 10000 * 10000);
        }

        public static int GetItemSubType(uint type)
        {
            return (int)(type % 1000000 / 1000);
        }

        public static int GetQuality(uint type)
        {
            return (int)(type % 10);
        }

        public static int GetEudemonType(uint idType) { return (int)((idType / 1000) % 10); }

        public static bool IsGhostGem(uint nType) 
        { 
            return IsExpend(nType) && GetItemtype(nType) == ITEMTYPE_GHOSTGEM;
        }

        public static bool IsEudemon(uint idType)
        {
            return GetItemSort(idType) == ItemSort.ItemsortExpend && GetItemtype(idType) == ITEMTYPE_EUDEMON;
        }

        public static bool IsEudemonEgg(uint idType)
        {
            return GetItemSort(idType) == ItemSort.ItemsortExpend && GetItemtype(idType) == ITEMTYPE_EUDEMON_EGG;
        }
        
        public static bool IsMedicine(uint type)
        {
            return type >= 1000000 && type <= 1049999;
        }

        public static bool IsExpend(uint type)
        {
            return IsArrowSort(type)
                   || GetItemSort(type) == ItemSort.ItemsortUsable
                   || GetItemSort(type) == ItemSort.ItemsortUsable2
                   || GetItemSort(type) == ItemSort.ItemsortUsable3
                   || GetItemSort(type) == (ItemSort)30;
        }

        public static bool IsArrowSort(uint type)
        {
            return GetItemtype(type) == 50000 && type != TYPE_JAR && !IsRing(type);
        }

        public static bool IsHelmet(uint type)
        {
            return type >= 110000 && type < 120000 || type >= 140000 && type < 150000 || type >= 123000 && type < 124000;
        }

        public static bool IsNeck(uint type)
        {
            int subType = GetItemSubType(type);
            return subType == 121 || subType == 123 || subType == 125 || subType == 127;
        }

        public static bool IsRing(uint type)
        {
            int subType = GetItemSubType(type);
            return subType == 141 || subType == 143 || subType == 145 || subType == 147;
        }

        public static bool IsWeapon(uint type)
        {
            return IsWeaponOneHand(type);
        }

        public static bool IsWeaponOneHand(uint type)
        {
            return GetItemSort(type) == ItemSort.ItemsortWeaponSingleHand;
        }

        public static bool IsArmor(uint type)
        {
            return type / 10000 == 13;
        }

        public static bool IsShoes(uint type)
        {
            int subType = GetItemSubType(type);
            return subType == 161 || subType == 163 || subType == 165 || subType == 167;
        }

        public static bool IsGarment(uint type)
        {
            return type >= 170000 && type < 200000;
        }

        public static bool IsEquipment(uint type)
        {
            return IsHelmet(type) || IsNeck(type) || IsRing(type) || IsWeapon(type) || IsArmor(type) || IsShoes(type) || IsHorn(type) || IsCrown(type) || IsRune(type);
        }

        public static bool IsHorn(uint type)
        {
            return type == 1110010;
        }

        public static bool IsCrown(uint type)
        {
            return type == 1110110;
        }

        public static bool IsRune(uint type)
        {
            return type == 1110210;
        }
        #endregion

        public virtual Task<bool> DeleteAsync()
        {
            logger.Error($"ItemBase->DeleteAsync(): Method not creaded!");
            return Task.FromResult(false);
        }

        public virtual Task<bool> SaveAsync()
        {
            logger.Error($"ItemBase->SaveAsync(): Method not creaded!");
            return Task.FromResult(false);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(item);
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
