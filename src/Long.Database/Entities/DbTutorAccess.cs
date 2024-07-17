namespace Long.Database.Entities
{
    [Table("cq_tutorexp")]
    public class DbTutorAccess
    {
        [Key][Column("tutor_id")] public virtual uint GuideIdentity { get; set; }
        [Column("tutor_lev")] public virtual byte TutorLevel { get; set; }
        [Column("exp")] public virtual int Experience { get; set; }
        [Column("tutor_exp")] public virtual int TutorExp { get; set; }
        [Column("god_time")] public virtual ushort Blessing { get; set; }
        [Column("uplevtime")] public virtual int UpLevTime { get; set; }
        [Column("reborn_times")] public virtual ushort RebornTimes { get; set; }
    }
}
