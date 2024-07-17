namespace Long.Database.Entities
{
    [Table("cq_talentgain")]
    public class DbTalentGain
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("eudemon_type")] public virtual uint EudemonType { get; set; }
        [Column("hatch_rate")] public virtual int HatchRate { get; set; }
        [Column("hatch_talent1")] public virtual int HatchTalent1 { get; set; }
        [Column("hatch_talent1_rate")] public virtual int HatchTalent1Rate { get; set; }
        [Column("hatch_talent2")] public virtual int HatchTalent2 { get; set; }
        [Column("hatch_talent2_rate")] public virtual int HatchTalent2Rate { get; set; }
        [Column("hatch_talent3")] public virtual int HatchTalent3 { get; set; }
        [Column("hatch_talent3_rate")] public virtual int HatchTalent3Rate { get; set; }
        [Column("rank1_rate")] public virtual int Rank1Rate { get; set; }
        [Column("rank2_rate")] public virtual int Rank2Rate { get; set; }
        [Column("rank3_rate")] public virtual int Rank3Rate { get; set; }
        [Column("rank1_prefer1")] public virtual int Rank1Prefer1 { get; set; }
        [Column("rank1_prefer1_rate")] public virtual int Rank1Prefer1Rate { get; set; }
        [Column("rank1_prefer2")] public virtual int Rank1Prefer2 { get; set; }
        [Column("rank1_prefer2_rate")] public virtual int Rank1Prefer2Rate { get; set; }
        [Column("rank1_prefer3")] public virtual int Rank1Prefer3 { get; set; }
        [Column("rank1_prefer3_rate")] public virtual int Rank1Prefer3Rate { get; set; }
        [Column("rank2_prefer1")] public virtual int Rank2Prefer1 { get; set; }
        [Column("rank2_prefer1_rate")] public virtual int Rank2Prefer1Rate { get; set; }
        [Column("rank2_prefer2")] public virtual int Rank2Prefer2 { get; set; }
        [Column("rank2_prefer2_rate")] public virtual int Rank2Prefer2Rate { get; set; }
        [Column("rank2_prefer3")] public virtual int Rank2Prefer3 { get; set; }
        [Column("rank2_prefer3_rate")] public virtual int Rank2Prefer3Rate { get; set; }
        [Column("rank3_prefer1")] public virtual int Rank3Prefer1 { get; set; }
        [Column("rank3_prefer1_rate")] public virtual int Rank3Prefer1Rate { get; set; }
        [Column("rank3_prefer2")] public virtual int Rank3Prefer2 { get; set; }
        [Column("rank3_prefer2_rate")] public virtual int Rank3Prefer2Rate { get; set; }
        [Column("rank3_prefer3")] public virtual int Rank3Prefer3 { get; set; }
        [Column("rank3_prefer3_rate")] public virtual int Rank3Prefer3Rate { get; set; }
        [Column("opentalent1_rate")] public virtual int OpenTalent1Rate { get; set; }
        [Column("opentalent2_rate")] public virtual int OpenTalent2Rate { get; set; }
        [Column("opentalent3_rate")] public virtual int OpenTalent3Rate { get; set; }
        [Column("opentalent4_rate")] public virtual int OpenTalent4Rate { get; set; }
        [Column("opentalent5_rate")] public virtual int OpenTalent5Rate { get; set; }
    }
}
