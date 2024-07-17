namespace Long.Database.Entities
{
    [Table("cq_eudemon")]
    public class DbEudemon
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("item_id")] public virtual uint ItemIdentity { get; set; }
        [Column("ori_owner_name")] public virtual string OriginalOwnerName { get; set; }
        [Column("name")] public virtual string Name { get; set; }
        [Column("relationship")] public virtual uint Relationship { get; set; }
        [Column("phyatk_grow_rate")] public virtual ushort PhyAtkGrowRate { get; set; }
        [Column("magicatk_grow_rate")] public virtual ushort MagicAtkGrowRate { get; set; }
        [Column("life_grow_rate")] public virtual ushort LifeGrowRate { get; set; }
        [Column("phyatk_grow_rate_max")] public virtual int PhyAtkGrowRateMax { get; set; }
        [Column("magicatk_grow_rate_max")] public virtual int MagicAtkGrowRateMax { get; set; }
        [Column("availabletime")] public virtual int AvailableTime { get; set; }
        [Column("life")] public virtual uint Life { get; set; }
        [Column("star_lev")] public virtual int StarLevel { get; set; }
        [Column("phy_atk_min")] public virtual int PhyAtkMin { get; set; }
        [Column("phy_atk_max")] public virtual int PhyAtkMax { get; set; }
        [Column("magic_atk_min")] public virtual int MagicAtkMin { get; set; }
        [Column("magic_atk_max")] public virtual int MagicAtkMax { get; set; }
        [Column("contract_time")] public virtual int ContractTime { get; set; }
        [Column("magic_defence")] public virtual int MagicDefence { get; set; }
        [Column("luck")] public virtual byte Luck { get; set; }
        [Column("damage_type")] public virtual byte DamageType { get; set; }
        [Column("level")] public virtual byte Level { get; set; }
        [Column("exp")] public virtual ulong Experience { get; set; }
        [Column("fidelity")] public virtual ushort Fidelity { get; set; }
        [Column("talent1")] public virtual byte Talent1 { get; set; }
        [Column("talent2")] public virtual byte Talent2 { get; set; }
        [Column("talent3")] public virtual byte Talent3 { get; set; }
        [Column("skill_num_limit")] public virtual byte SkillNumLimit { get; set; }
        [Column("reborn_times")] public virtual ushort RebornTimes { get; set; }
        [Column("card_id")] public virtual uint CardId { get; set; }
        [Column("talent4")] public virtual byte Talent4 { get; set; }
        [Column("talent5")] public virtual byte Talent5 { get; set; }
        [Column("initial_phy")] public virtual ushort InitialPhy { get; set; }
        [Column("initial_magic")] public virtual ushort InitialMagic { get; set; }
        [Column("initial_def")] public virtual ushort InitialDef { get; set; }
        [Column("initial_life")] public virtual ushort InitialLife { get; set; }
        [Column("phydef_grow_rate")] public virtual ushort PhyDefGrowRate { get; set; }
        [Column("magicdef_grow_rate")] public virtual ushort MagicDefGrowRate { get; set; }
        [Column("mete_lev")] public virtual int MeteLev { get; set; }
        [Column("chksum")] public virtual uint Chksum { get; set; }
        [Column("item_type")] public virtual uint ItemType { get; set; }
        [Column("owner_id")] public virtual uint OwnerId { get; set; }
        [Column("player_id")] public virtual uint PlayerId { get; set; }
        [Column("position")] public virtual byte Position { get; set; }
        [Column("syndicate_id")] public virtual uint SyndicateId { get; set; }
        [Column("plunder")] public virtual ushort Plunder { get; set; }
        [Column("reborn_day")] public virtual byte RebornDay { get; set; }
        [Column("reborn_limit_add")] public virtual byte RebornLimitAdd { get; set; }
        [Column("cinnabar")] public virtual ushort CinnBar { get; set; }
        [Column("god_strength")] public virtual ulong GodStrength { get; set; }
        [Column("god_level")] public virtual byte GodLevel { get; set; }
        [Column("god")] public virtual byte God { get; set; }
        [Column("major_attr")] public virtual ushort MajorAttr { get; set; }
        [Column("minor_attr1")] public virtual ushort MinorAttr1 { get; set; }
        [Column("minor_attr2")] public virtual ushort MinorAttr2 { get; set; }
        [Column("panacea")] public virtual ushort Panacea { get; set; }
        [Column("god_step")] public virtual byte GodStep { get; set; }
        [Column("beast_soul")] public virtual ushort BeastSoul { get; set; }
        [Column("expballuse")] public virtual uint ExpBallUse { get; set; }
        [Column("expcrystaluse")] public virtual uint ExpCrystalUse { get; set; }
    }
}
