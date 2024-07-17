namespace Long.Database.Entities
{
    [Table("cq_user")]
    public class DbUser
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("account_id")] public virtual uint AccountIdentity { get; set; }
        [Column("name")] public virtual string Name { get; set; }
        [Column("mate")] public virtual string Mate { get; set; }
        [Column("length")] public virtual byte Length { get; set; }
        [Column("fat")] public virtual byte Fat { get; set; }
        [Column("lookface")] public virtual uint Lookface { get; set; }
        [Column("hair")] public virtual ushort Hairstyle { get; set; }
        [Column("money")] public virtual ulong Money { get; set; }
        [Column("money_saved")] public virtual uint MoneySaved { get; set; }
        [Column("task_data")] public virtual uint TaskData { get; set; }
        [Column("level")] public virtual byte Level { get; set; }
        [Column("exp")] public virtual ulong Experience { get; set; }
        [Column("virtue")] public virtual uint Virtue { get; set; }
        [Column("power")] public virtual ushort Power { get; set; }
        [Column("dexterity")] public virtual ushort Dexterity { get; set; }
        [Column("Speed")] public virtual ushort Agility { get; set; }
        [Column("health")] public virtual ushort Vitality { get; set; }
        [Column("soul")] public virtual ushort Spirit { get; set; }
        [Column("additional_point")] public virtual ushort AttributePoints { get; set; }
        [Column("auto_allot")] public virtual byte AutoAllot { get; set; }
        [Column("life")] public virtual uint HealthPoints { get; set; }
        [Column("mana")] public virtual ushort ManaPoints { get; set; }
        [Column("profession")] public virtual byte Profession { get; set; }
        [Column("potential")] public virtual int Potential { get; set; }
        [Column("pk")] public virtual ushort KillPoints { get; set; }
        [Column("nobility")] public virtual byte Nobility { get; set; }
        [Column("medal")] public virtual string Medal { get; set; }
        [Column("medal_select")] public virtual int MedalSelect { get; set; }
        [Column("metempsychosis")] public virtual byte Metempsychosis { get; set; }
        [Column("syndicate_id")] public virtual ushort SynId { get; set; }
        [Column("recordmap_id")] public virtual uint MapId { get; set; }
        [Column("recordx")] public virtual ushort X { get; set; }
        [Column("recordy")] public virtual ushort Y { get; set; }
        [Column("last_login")] public virtual uint LastLoginTime { get; set; }
        [Column("task_mask")] public virtual uint TaskMask { get; set; }
        [Column("time_of_life")] public virtual uint LuckyTime { get; set; }
        [Column("home_id")] public virtual uint HomeId { get; set; }
        [Column("title")] public virtual uint Title { get; set; }
        [Column("title_select")] public virtual byte TitleSelect { get; set; }
        [Column("tutor_exp")] public virtual uint TutorExp { get; set; }
        [Column("tutor_level")] public virtual byte TutorLevel { get; set; }
        [Column("maxlife_percent")] public virtual int MaxLifePercent { get; set; }
        [Column("mercenary_rank")] public virtual int MercenaryRank { get; set; }
        [Column("mercenary_exp")] public virtual int MercenaryExp { get; set; }
        [Column("nobility_rank")] public virtual byte NobilityRank { get; set; }
        [Column("exploit")] public virtual int Exploit { get; set; }
        [Column("eud_pack_size")] public virtual byte EudPackSize { get; set; }
        [Column("disableFlag")] public virtual int DisableFlag { get; set; }
        [Column("reg_time")] public virtual uint RegisterTime { get; set; }
        [Column("accu_online_time")] public virtual byte AccuOnlineTime { get; set; }
        [Column("accu_offline_time")] public virtual byte AccuOfflineTime { get; set; }
        [Column("last_logout")] public virtual uint LogoutTime { get; set; }
        [Column("prompt_ver")] public virtual int PromptVer { get; set; }
        [Column("Friend_share")] public virtual int FriendShare { get; set; }
        [Column("Login_time")] public virtual int LoginTime { get; set; }
        [Column("Login_ip")] public virtual string LoginIp { get; set; }
        [Column("emoney")] public virtual uint Emoney { get; set; }
        [Column("emoney2")] public virtual uint EmoneyMono { get; set; }
        [Column("chk_sum")] public virtual uint EmoneyChkSum { get; set; }
        [Column("soul_value")] public virtual int SoulValue { get; set; }
        [Column("elock")] public virtual string Elock { get; set; }
        [Column("ExpBallUsage")] public virtual uint ExpBallUsage { get; set; }
        [Column("ExpCrystalUsage")] public virtual ushort ExpCrystalUsage { get; set; }
        [Column("cheat_time")] public virtual int CheatTime { get; set; }
        [Column("online_time")] public virtual int OnlineSeconds { get; set; }
        [Column("god_status")] public virtual uint HeavenBlessing { get; set; }
        [Column("auto_exercise")] public virtual ushort AutoExercise { get; set; }
        [Column("hung_last_logout")] public virtual uint HungLastLogout { get; set; } // Offline TG
        [Column("big_prize_fails")] public virtual byte BigPrizeFails { get; set; }
        [Column("small_prize_fails")] public virtual byte SmallPrizeFails { get; set; }
        [Column("bonus_points")] public virtual uint BonusPoints { get; set; }
        [Column("flower")] public virtual uint Flower { get; set; }
        [Column("flower_w")] public virtual uint FlowerWhite { get; set; }
        [Column("flower_date")] public virtual uint FlowerDate { get; set; }

        [Column("income")] public virtual ulong Income { get; set; }
        [Column("illu_Butterfly")] public virtual ulong IlluButtlerfly { get; set; }
        [Column("illu_Fish")] public virtual ulong IlluFish { get; set; }
        [Column("illu_Shell")] public virtual ulong IlluShell { get; set; }
        [Column("illu_Herbal")] public virtual ulong IlluHerbal { get; set; }
        [Column("illu_Ore")] public virtual ulong IlluOre { get; set; }

        [Column("password_id")] public virtual ulong PasswordId { get; set; }

        [Column("quiz_point")] public virtual uint QuizPoints { get; set; }
        [Column("protect_state")] public virtual byte ProtectState { get; set; }

        [Column("donation")] public virtual ulong Donation { get; set; }
        [Column("wood")] public virtual uint Wood { get; set; }
        [Column("stone")] public virtual uint Stone { get; set; }

        [Column("todo_list_mask1")] public virtual uint TodoListMask1 { get; set; }
        [Column("todo_list1")] public virtual ulong TodoList1 { get; set; }
        [Column("todo_list_hex1")] public virtual ulong TodoListHex1 { get; set; }

        [Column("coach")] public virtual uint Coach { get; set; }
        [Column("coach_time")] public virtual uint CoachTime { get; set; }
        [Column("coach_deed")] public virtual uint CoachDeed { get; set; }
        [Column("coach_date")] public virtual uint CoachDay { get; set; }

        [Column("extra_hatch_size")] public virtual byte ExtraHatchSize { get; set; }
        [Column("shadiness_money")] public virtual ulong ShadinessMoney { get; set; }
        [Column("shadiness_emoney")] public virtual uint ShadinessEmoney { get; set; }

        [Column("soul_value_lev")] public virtual byte SoulValueLev { get; set; }
        [Column("business")] public virtual uint Business { get; set; }
        [Column("type")] public virtual byte Type { get; set; }
        [Column("god_strength")] public virtual ulong GodStrength { get; set; }
        [Column("god_level")] public virtual byte GodLevel { get; set; }

        [Column("deification")] public virtual byte Deification { get; set; }
        [Column("tmoney")] public virtual uint Tmoney { get; set; }


        [Column("demon_level")] public virtual byte DemonLevel { get; set; }
        [Column("demon_exp")] public virtual ulong DemonExp { get; set; }
        [Column("socialclass")] public virtual uint SocialClass { get; set; }



        [Column("credit_point")] public virtual int CreditPoint { get; set; }
        [Column("credit_limit")] public virtual int CreditLimit { get; set; }
        [Column("credit_return")] public virtual ushort CreditReturn { get; set; }

        [Column("eudkernel_id")] public virtual uint EudKernelId { get; set; }
        [Column("refinery_time_end")] public virtual uint RefineryTimeEnd { get; set; }
    }
}
