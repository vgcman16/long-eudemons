using Long.Kernel.Database;
using Long.Kernel.Network.Game.Packets;
using Long.Kernel.States.User;
using System.Collections.Concurrent;

namespace Long.Kernel.States.Status
{
    public sealed class StatusSet
    {
        public const int 
                STATUS_NORMAL = 0,
                STATUS_TEAMLEADER = 1,										// ¶Ó³¤
                STATUS_DIE = 2,										// ËÀÍö
                STATUS_GHOST = 3,                                      // ÖÐ¶¾
                STATUS_DISAPPEARING = 4,										// Ê¬ÌåÏûÊ§×´Ì¬
                STATUS_CRIME = 5,										// ·¸×ïÉÁÀ¶×´Ì¬
                STATUS_PKVALUERED = 6,										// PK×´Ì¬								// ¶Ó³¤
                STATUS_PKVALUEBLACK = 7,
                STATUS_FREEZE = 11,									// ±ù¶³×´Ì¬
                STATUS_SILENCED = 12,									// ±ù¶³×´Ì¬
                STATUS_FLY = 24,									// ±ù¶³×´Ì¬
                STATUS_TOP2 = 25,									// ±ù¶³×´Ì¬
                STATUS_TOP3 = 26,                                   // ±ù¶³×´Ì¬
                STATUS_TOP4 = 27,									// ±ù¶³×´Ì¬
                STATUS_TOP5 = 28,                                   // ±ù¶³×´Ì¬
                STATUS_NOJUMP = 29,									// ±ù¶³×´Ì¬
                STATUS_NOMOUNT = 30,                                   // ±ù¶³×´Ì¬
                STATUS_SUCKMAGIC = 31,									// ±ù¶³×´Ì¬
                STATUS_CURSED = 32,
                STATUS_TOP1 = 60,
                STATUS_SPECIALTRAINING = 61,
                STATUS_DRAGONMORPHSTATUS = 62,
                STATUS_DRAGONMORPH = 65,
                STATUS_BLOCK = 68,
                STATUS_EXAUSTED = 69,
                STATUS_FLY2 = 70,
                STATUS_CANNOTSPEAK = 71, 
                STATUS_XPSKILLFLYUNKNOWN = 72,
                STATUS_EXTRAEXPEFFECT = 73, 
                STATUS_PROTECTED = 74,
                STATUS_PROTECTED2 = 75,
                STATUS_HEARTSEFFECT = 76,
                STATUS_REDROSESPETALS = 78,
                STATUS_DIVINESUCCESS = 79,
                STATUS_LARGEGODDESSEFFECT = 80,
                STATUS_BLACKFOG = 81,
                STATUS_SUPERSOLDIERBLUE = 82,
                STATUS_GROUNDSTUCK = 83,
                STATUS_BLACKGHOSTSEFFECT = 84,
                STATUS_MINDSEALEDSILENCE = 85,
                STATUS_SOMESTATUSEFFECT = 86,
                STATUS_WEIRDTOXEFFECT = 87,
                STATUS_DEADLYSONGEFFECT = 88,
                STATUS_DECADENCE = 89,
                STATUS_POSESSED = 90,
                STATUS_FRENEZY = 91,
                STATUS_XPSKILLUNKNOWN3 = 92,
                STATUS_CYCLONEDETAINED = 93,
                STATUS_SOMEEFFECT = 94,
                STATUS_BLACKTORNADO = 95,
                STATUS_BLOCK2 = 100,
                STATUS_EXAUSTED2 = 101,

                STATUS_DETACH_BADLY = 6,										// Çå³ýËùÓÐ²»Á¼×´Ì¬
                STATUS_DETACH_ALL = 7,										// Çå³ýËùÓÐÄ§·¨×´Ì¬
                STATUS_VAMPIRE = 8,										// ATKSTATUSÖÐÎüÑª
                STATUS_MAGICDEFENCE = 10,									// Ä§·¨·ÀÓùÌáÉý/ÏÂ½µ
                STATUS_SUPER_MDEF = 11,									// ³¬¼¶Ä§·À
                STATUS_ATTACK = 12,									// ¹¥»÷ÌáÉý/ÏÂ½µ
                STATUS_REFLECT = 13,									// ¹¥»÷·´Éä
                STATUS_HIDDEN = 14,									// ÒþÉí
                STATUS_MAGICDAMAGE = 15,									// Ä§·¨ÉËº¦ÌáÉý/ÏÂ½µ
                STATUS_ATKSPEED = 16,									// ¹¥»÷ËÙ¶ÈÌáÉý/ÏÂ½µ
                STATUS_LURKER = 17,									// user only			// Ç±ÐÐ£¬´Ë×´Ì¬ÏÂ²»ÊÜNPC¹¥»÷£¬¶ÔÍæ¼ÒÎÞÐ§
                STATUS_SYNCRIME = 18,									// °ïÅÉ·¸×ï
                STATUS_REFLECTMAGIC = 19,									// Ä§·¨·´Éä
                STATUS_SUPER_DEF = 20,									// ³¬¼¶·ÀÓù
                STATUS_SUPER_ATK = 21,									// self only	// ³¬¼¶¹¥»÷
                STATUS_SUPER_MATK = 22,		 							// self only	// ³¬¼¶Ä§¹¥
                STATUS_STOP = 23,
                STATUS_DEFENCE1 = 24,									// ·ÀÓùÌá¸ß/½µµÍ1
                STATUS_DEFENCE2 = 25,									// ·ÀÓùÌá¸ß/½µµÍ2
                STATUS_DEFENCE3 = 26,									// ·ÀÓùÌá¸ß/½µµÍ3
                STATUS_SMOKE = 28,									// ÑÌÎíÐ§¹û
                STATUS_DARKNESS = 29,									// ºÚ°µÐ§¹û
                STATUS_PALSY = 30,									// Âé±ÔÐ§¹û

                STATUS_TEAM_BEGIN = 31,
                STATUS_TEAMHEALTH = 31,									// Ò½ÁÆ½á½ç
                STATUS_TEAMATTACK = 32,									// ¹¥»÷½á½ç
                STATUS_TEAMDEFENCE = 33,									// »¤Ìå½á½ç
                STATUS_TEAMSPEED = 34,									// ËÙ¶È½á½ç
                STATUS_TEAMEXP = 35,									// ÐÞÁ¶½á½ç
                STATUS_TEAMSPIRIT = 36,									// ÐÄÁé½á½ç
                STATUS_TEAMCLEAN = 37,									// ¾»»¯½á½ç
                STATUS_TEAM_END = 37,

                STATUS_SLOWDOWN1 = 38,									// ÒÆ¶¯ËÙ¶ÈÌáÉý/ÏÂ½µ
                STATUS_SLOWDOWN2 = 39,									// ½µµÍËÙ¶È£¨½öÔÚÉúÃüµÍÓÚÒ»°ëµÄÊ±ºò¡£¿Í»§¶Ë±íÏÖ£©
                STATUS_MAXLIFE = 40,									// ×î´óÉúÃüÔö¼Ó/½µµÍ
                STATUS_MAXENERGY = 41,									// ×î´óÌåÁ¦Ôö¼Ó/½µµÍ
                STATUS_DEF2ATK = 42,									// ·ÀÓù×ª»»Îª¹¥»÷(power=±»×ª»»µÄ·ÀÓù°Ù·Ö±È)
                STATUS_ADD_EXP = 43,						// Õ½¶·¾­ÑéÔö¼Ó -- Ö»ÄÜ¶Ô¶Ó³¤Ê¹ÓÃ£¬ÀàËÆÐÞÁ¶½á½çÐ§¹û
                STATUS_DMG2LIFE = 44,									// Ã¿´Î¹¥»÷ÉËº¦²¿·Ö×ª»»Îª×Ô¼ºµÄÉúÃü(power=±»×ª»»µÄ°Ù·Ö±È)
                STATUS_ATTRACT_MONSTER = 45,								// ÎüÒý¹ÖÎï
                STATUS_XPFULL = 38,                                 // XPÂú
                                                                    //---jinggy---begin
                STATUS_ADJUST_DODGE = 47,			//µ÷½Ú×ÜµÄ¶ã±ÜÖµ
                STATUS_ADJUST_XP = 48,			//µ÷½ÚÃ¿´ÎÔö¼ÓXPÖµ
                STATUS_ADJUST_DROPMONEY = 49,       //µ÷½Ú¹ÖÎïÃ¿´ÎµôÇ®
                                                    //---jinggy---end
                STATUS_PK_PROTECT = 50,									// PK±£»¤×´Ì¬
                STATUS_PELT = 51,									// ¼²ÐÐ×´Ì¬
                STATUS_ADJUST_EXP = 52,                                 // Õ½¶·»ñµÃ¾­Ñéµ÷Õû¡ª¡ªÔÊÐí¶ÔÈÎºÎÈËÊ¹ÓÃ

                /////////////////////////////////
                // »ÃÊÞ¼¼ÄÜÐèÒªÓÃµ½µÄ×´Ì¬
                STATUS_HEAL = 100,			// ÖÎÓú
                STATUS_FAINT = 101,			// ´òÔÎ
                STATUS_TRUCULENCE = 102,			// Ò°Âù
                STATUS_DAMAGE = 103,			// µ÷ÕûÊÜµ½µÄÉËº¦
                STATUS_ATKER_DAMAGE = 104,			// µ÷Õû¶ÔÄ¿±êÔì³ÉµÄÉËº¦
                STATUS_CONFUSION = 105,			// »ìÂÒ
                STATUS_FRENZY = 106,			// ¿ñ±©
                STATUS_EXTRA_POWER = 107,			// ÉñÁ¦
                STATUS_TRANSFER_SHIELD = 108,			// »¤¶Ü
                STATUS_SORB_REFLECT = 109,			// ÎüÊÕ·´Éä
                STATUS_FRENZY2 = 110,           // ÁíÍâÒ»ÖÖ¿ñ±©×´Ì¬


                STATUS_LOCK = 251,									// ±»Ëø¶¨×´Ì¬ -- only used in server

                STATUS_LIMIT = 256;						// ½ÇÉ«×´Ì¬²»ÄÜ³¬¹ýÕâ¸öÖµ

        private const int TOTAL_STATUS_SIZE = 4;

        private readonly Role owner;
        public ConcurrentDictionary<int, IStatus> Status = new();

        public StatusSet(Role role)
        {
            if (role == null)
            {
                return;
            }

            owner = role;
        }

        public IStatus this[int nKey]
        {
            get
            {
                try
                {
                    return Status.TryGetValue(nKey, out IStatus ret) ? ret : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public IStatus GetObjByIndex(int nKey)
        {
            return Status.TryGetValue(nKey, out IStatus ret) ? ret : null;
        }

        public async Task<bool> AddObjAsync(IStatus status)
        {
            if (status.Identity > STATUS_LIMIT)
            {
                return false;
            }

            var info = new StatusInfoStruct();
            status.GetInfo(ref info);
            if (Status.ContainsKey(info.Status))
            {
                return false; // status already exists
            }

            int index = status.Identity / 32;
            int flagRef = (status.Identity - 1) % 32;
            uint uFlag = 1U << flagRef;
            if (index < 4)
            {
                owner.StatusFlag[index] |= uFlag;
            }

            Status.TryAdd(info.Status, status);

            switch (status.Identity)
            {
                case STATUS_FREEZE:
                    {
                        //owner.BattleSystem.ResetBattle();
                        //await owner.MagicData.AbortMagicAsync(true);
                        break;
                    }
            }

            await owner.SynchroAttributesAsync(ClientUpdateType.KeepStatus1, owner.StatusFlag[0], owner.StatusFlag[1], owner.StatusFlag[2], owner.StatusFlag[3], true);
            return true;
        }

        public async Task<bool> DelObjAsync(int flag)
        {
            if (flag > STATUS_LIMIT)
            {
                return false;
            }

            if (!Status.TryRemove(flag, out IStatus status))
            {
                return false;
            }

            int index = flag / 32;
            int flagRef = (flag - 1) % 32;
            uint uFlag = 1U << flagRef;
            if (index < 4)
            {
                owner.StatusFlag[index] &= ~uFlag;
            }

            if (status?.Model != null)
            {
                await ServerDbContext.DeleteAsync(status.Model);
            }

            await owner.SynchroAttributesAsync(ClientUpdateType.KeepStatus1, owner.StatusFlag[0], owner.StatusFlag[1], owner.StatusFlag[2], owner.StatusFlag[3], true);
            return true;
        }

        public async Task SendAllStatusAsync()
        {
            if (owner is Character)
            {
                await owner.SynchroAttributesAsync(ClientUpdateType.KeepStatus1, owner.StatusFlag[0], owner.StatusFlag[1], owner.StatusFlag[2], owner.StatusFlag[3], true);
            }
        }

        public static int GetRealStatus(int status)
        {
            switch (status)
            {
                
            }

            return status;
        }
    }
}