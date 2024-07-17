namespace Long.Database.Entities
{
    [Table("cq_tutor_contributions")]
    public class DbTutorContribution
    {
        [Key][Column("id")] public virtual uint Identity { get; set; }
        [Column("tutor_id")] public virtual uint TutorIdentity { get; set; }
        [Column("student_id")] public virtual uint StudentIdentity { get; set; }
        [Column("student_name")] public virtual string StudentName { get; set; }
        [Column("god_time")] public virtual ushort GodTime { get; set; }
        [Column("uplevtime")] public virtual uint UpLevTime { get; set; }
        [Column("reborn_times")] public virtual uint RebornTimes { get; set; }
    }
}
