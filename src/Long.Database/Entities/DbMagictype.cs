namespace Long.Database.Entities
{
    [Table("cq_magictype")]
    public class DbMagictype
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("type")] public virtual uint Type { get; set; }
        [Column("sort")] public virtual uint Sort { get; set; }
        [Column("name")] public virtual string Name { get; set; }
        [Column("crime")] public virtual byte Crime { get; set; }
        [Column("ground")] public virtual byte Ground { get; set; }
        [Column("multi")] public virtual byte Multi { get; set; }
        [Column("target")] public virtual uint Target { get; set; }
        [Column("level")] public virtual uint Level { get; set; }
        [Column("use_mp")] public virtual uint UseMp { get; set; }
        [Column("use_potential")] public virtual uint UsePotential { get; set; }
        [Column("power")] public virtual int Power { get; set; }
        [Column("intone_speed")] public virtual uint IntoneSpeed { get; set; }
        [Column("percent")] public virtual uint Percent { get; set; }
        [Column("step_secs")] public virtual uint StepSecs { get; set; }
        [Column("range")] public virtual uint Range { get; set; }
        [Column("distance")] public virtual uint Distance { get; set; }
        [Column("status_chance")] public virtual int StatusChance { get; set; }
        [Column("status")] public virtual int Status { get; set; }
        [Column("need_prof")] public virtual uint NeedProf { get; set; }
        [Column("need_exp")] public virtual int NeedExp { get; set; }
        [Column("need_level")] public virtual uint NeedLevel { get; set; }
        [Column("need_gemtype")] public virtual uint NeedGemType { get; set; }
        [Column("use_xp")] public virtual byte UseXp { get; set; }
        [Column("weapon_subtype")] public virtual uint WeaponSubtype { get; set; }
        [Column("active_times")] public virtual uint ActiveTimes { get; set; }
        [Column("auto_active")] public virtual uint AutoActive { get; set; }
        [Column("floor_attr")] public virtual uint FloorAttr { get; set; }
        [Column("auto_learn")] public virtual byte AutoLearn { get; set; }
        [Column("learn_level")] public virtual uint LearnLevel { get; set; }
        [Column("drop_weapon")] public virtual byte DropWeapon { get; set; }
        [Column("use_ep")] public virtual uint UseEp { get; set; }
        [Column("weapon_hit")] public virtual byte WeaponHit { get; set; }
        [Column("use_item")] public virtual uint UseItem { get; set; }
        [Column("next_magic")] public virtual uint NextMagic { get; set; }
        [Column("delay_ms")] public virtual uint DelayMs { get; set; }
        [Column("use_item_num")] public virtual uint UseItemNum { get; set; }
        [Column("width")] public virtual uint Width { get; set; }
        [Column("durability")] public virtual uint Durability { get; set; }
        [Column("apply_ms")] public virtual uint ApplyMs { get; set; }
        [Column("track_id")] public virtual uint TrackId { get; set; }
        [Column("track_id2")] public virtual uint TrackId2 { get; set; }
        [Column("auto_learn_prob")] public virtual ushort AutoLearnProbability { get; set; }
        [Column("group_type")] public virtual byte GroupType { get; set; }
        [Column("group_member1_pos")] public virtual uint GroupMemberPos1 { get; set; }
        [Column("group_member2_pos")] public virtual uint GroupMemberPos2 { get; set; }
        [Column("group_member3_pos")] public virtual uint GroupMemberPos3 { get; set; }
        [Column("magic1")] public virtual uint Magic1 { get; set; }
        [Column("magic2")] public virtual uint Magic2 { get; set; }
        [Column("magic3")] public virtual uint Magic3 { get; set; }
        [Column("magic4")] public virtual uint Magic4 { get; set; }
        [Column("attack_combine")] public virtual byte AttackCombine { get; set; }
        [Column("flag")] public virtual uint Flag { get; set; }
        [Column("use_soul")] public virtual int UseSoul { get; set; }
        [Column("need_uplevtime")] public virtual uint NeedUpLevTime { get; set; }
        [Column("req_god")] public virtual byte ReqGod { get; set; }
        [Column("req_god_lev")] public virtual byte ReqGodLevel { get; set; }
        [Column("learn_level_god")] public virtual byte LearnLevelGod { get; set; }
        [Column("life_precent_dmg")] public virtual uint LifePercentDamage { get; set; }
        [Column("req_star_lev")] public virtual byte ReqStarLevel { get; set; }

        [NotMapped] public virtual List<DbTrack> TrackS { get; set; } = new();
        [NotMapped] public virtual List<DbTrack> TrackT { get; set; } = new();
    }
}
