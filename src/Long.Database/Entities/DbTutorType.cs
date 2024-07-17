namespace Long.Database.Entities
{
    [Table("cq_tutor_type")]
    public class DbTutorType
    {
        [Key][Column("id")] public virtual uint Id { get; set; }
        [Column("user_lev")]public virtual byte UserLevel { get; set; }
        [Column("student_num")] public virtual byte StudentNum { get; set; }
        [Column("battle_lev_share")] public virtual byte BattleLevelShare { get; set; }
        [Column("exp_need")] public virtual uint ExpNeed { get; set; }
        [Column("fetch_factor")] public virtual byte FetchFactor { get; set; }
        [Column("coach_time")] public virtual ushort CoachTime { get; set; }
    }
}
