namespace Long.Database.Entities
{
    [Table("cq_talenttype")]
    public class DbTalentType
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("sort")] public virtual int sort { get; set; }
        [Column("name")] public virtual string Name { get; set; }
        [Column("crime")] public virtual byte Crime { get; set; }
        [Column("ground")] public virtual byte Ground { get; set; }
        [Column("multi")] public virtual byte Multi { get; set; }
        [Column("target")] public virtual byte Target { get; set; }
        [Column("level")] public virtual byte Level { get; set; }
        [Column("use_mp")] public virtual int UseMp { get; set; }
        [Column("use_potential")] public virtual int UsePotential { get; set; }
        [Column("power")] public virtual int Power { get; set; }
        [Column("intone_speed")] public virtual int IntoneSpeed { get; set; }
        [Column("percent")] public virtual int Percent { get; set; }
        [Column("step_secs")] public virtual int StepSecs { get; set; }
        [Column("range")] public virtual int Range { get; set; }
        [Column("distance")] public virtual int Distance { get; set; }
        [Column("status_chance")] public virtual int StatusChance { get; set; }
        [Column("status")] public virtual int Status { get; set; }
        [Column("need_prof")] public virtual int NeedProf { get; set; }
        [Column("need_exp")] public virtual int NeedExp { get; set; }
        [Column("need_level")] public virtual int NeedLevel { get; set; }
        [Column("need_gemtype")] public virtual int NeedGemType { get; set; }
        [Column("use_xp")] public virtual int UseXp { get; set; }
        [Column("weapon_subtype")] public virtual int WeaponSubType { get; set; }
        [Column("active_times")] public virtual int ActiveTimes { get; set; }
        [Column("auto_active")] public virtual int AutoActive { get; set; }
        [Column("floor_attr")] public virtual int FloorAttr { get; set; }
        [Column("auto_learn")] public virtual int AutoLearn { get; set; }
        [Column("learn_level")] public virtual int LearnLevel { get; set; }
        [Column("drop_weapon")] public virtual int DropWeapon { get; set; }
        [Column("use_ep")] public virtual int UseEp { get; set; }
        [Column("weapon_hit")] public virtual int WeaponHit { get; set; }
        [Column("next_magic")] public virtual int NextMagic { get; set; }
        [Column("delay_ms")] public virtual int DelayMs { get; set; }
        [Column("use_item_num")] public virtual int UseItemNum { get; set; }
        [Column("width")] public virtual int Width { get; set; }
        [Column("durability")] public virtual int Durability { get; set; }
        [Column("apply_ms")] public virtual int ApplyMs { get; set; }
        [Column("flag")] public virtual int Flag { get; set; }
        [Column("grade")] public virtual int Grade { get; set; }
        [Column("rank")] public virtual int Rank { get; set; }
        [Column("need_damagetype")] public virtual int NeedDamageType { get; set; }
        [Column("need_eudemontype1")] public virtual int NeedEudemonType1 { get; set; }
        [Column("need_eudemontype2")] public virtual int NeedEudemonType2 { get; set; }
        [Column("need_eudemontype3")] public virtual int NeedEudemonType3 { get; set; }
        [Column("cost")] public virtual int Cost { get; set; }
        [Column("data1")] public virtual int Data1 { get; set; }
        [Column("data2")] public virtual int Data2 { get; set; }
        [Column("data3")] public virtual int Data3 { get; set; }
        [Column("active_mode")] public virtual int ActiveMode { get; set; }
    }
}
