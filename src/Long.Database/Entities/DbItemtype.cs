namespace Long.Database.Entities
{
    [Table("cq_itemtype")]
    public class DbItemtype
    {
        [Key][Column("id")] public virtual uint Type { get; set; }
        [Column("name")] public virtual string Name { get; set; }
        [Column("req_profession")] public virtual uint ReqProfession { get; set; }
        //[Column("req_weaponskill")] public virtual byte ReqWeaponskill { get; set; }
        [Column("level")] public virtual byte Level { get; set; }
        [Column("req_level")] public virtual byte ReqLevel { get; set; }
        [Column("req_sex")] public virtual byte ReqSex { get; set; }
        [Column("req_force")] public virtual ushort ReqForce { get; set; }
        [Column("req_dex")] public virtual ushort ReqDexterity { get; set; }
        [Column("req_health")] public virtual ushort ReqHealth { get; set; }
        [Column("req_soul")] public virtual ushort ReqSoul { get; set; }
        [Column("monopoly")] public virtual uint Monopoly { get; set; }
        [Column("weight")] public virtual ushort Weight { get; set; }
        [Column("price")] public virtual uint Price { get; set; }
        [Column("id_action")] public virtual uint IdAction { get; set; }
        [Column("attack_max")] public virtual ushort AttackMax { get; set; }
        [Column("attack_min")] public virtual ushort AttackMin { get; set; }
        [Column("defense")] public virtual int Defense { get; set; }
        [Column("magic_def")] public virtual int MagicDef { get; set; }
        [Column("magic_atk_max")] public virtual int MgcAttackMax { get; set; }
        [Column("magic_atk_min")] public virtual int MgcAttackMin { get; set; }
        [Column("dodge")] public virtual int Dodge { get; set; }
        [Column("life")] public virtual int Life { get; set; }
        [Column("mana")] public virtual int Mana { get; set; }
        [Column("amount")] public virtual ushort Amount { get; set; }
        [Column("amount_limit")] public virtual ushort AmountLimit { get; set; }
        [Column("ident")] public virtual byte Ident { get; set; }
        [Column("gem1")] public virtual byte Gem1 { get; set; }
        [Column("gem2")] public virtual byte Gem2 { get; set; }
        [Column("magic1")] public virtual uint Magic1 { get; set; }
        [Column("magic2")] public virtual byte Magic2 { get; set; }
        [Column("magic3")] public virtual byte Magic3 { get; set; }
        [Column("atk_range")] public virtual int AtkRange { get; set; }
        [Column("atk_speed")] public virtual int AtkSpeed { get; set; }
        [Column("hitrate")] public virtual int HitRate { get; set; }
        [Column("monster_type")] public virtual uint MonsterType { get; set; }
        [Column("target")] public virtual int Target { get; set; }
        [Column("able_mask")] public virtual byte AbleMask { get; set; }
        [Column("exp_type")] public virtual byte ExpType { get; set; }
        [Column("emoney")] public virtual uint EmoneyPrice { get; set; }
        [Column("official1")] public virtual byte Official1 { get; set; }
        [Column("official2")] public virtual byte Official2 { get; set; }
        [Column("official3")] public virtual byte Official3 { get; set; }
        [Column("official4")] public virtual byte Official4 { get; set; }
        [Column("official5")] public virtual byte Official5 { get; set; }
        [Column("official6")] public virtual byte Official6 { get; set; }
        [Column("official7")] public virtual byte Official7 { get; set; }
        [Column("official8")] public virtual byte Official8 { get; set; }
        [Column("official9")] public virtual byte Official9 { get; set; }
        [Column("official10")] public virtual byte Official10 { get; set; }
        [Column("official11")] public virtual byte Official11 { get; set; }
        [Column("soul_value")] public virtual uint SoulValue { get; set; }
        [Column("chance")] public virtual uint Chance { get; set; }
        [Column("req_god")] public virtual byte ReqGod { get; set; }
        [Column("save_time")] public virtual uint SaveTime { get; set; }
        [Column("sacrifice_id")] public virtual uint SacrificeId { get; set; }
        [Column("tmoney")] public virtual uint Tmoney { get; set; }
        [Column("demon_flag")] public virtual byte DemonFlag { get; set; }
        [Column("flagbit")] public virtual uint Flagbit { get; set; }
        [Column("discount_price")] public virtual uint DiscountPrice { get; set; }
    }
}
